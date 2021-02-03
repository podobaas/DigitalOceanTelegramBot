using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DigitalOcean.API.Exceptions;
using DigitalOceanBot.Core.CallbackQueries;
using DigitalOceanBot.Core.Commands;
using DigitalOceanBot.Core.StateHandlers;
using DigitalOceanBot.Extensions;
using DigitalOceanBot.Messages;
using DigitalOceanBot.Services;
using DigitalOceanBot.Types.Enums;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DigitalOceanBot.Core
{
    public sealed class BotCommandResolver
    {
        private readonly ILogger<BotCommandResolver> _logger;
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly StorageService _storageService;
        private readonly IEnumerable<IBotCommand> _commands;
        private readonly IEnumerable<IBotStateHandler> _stateHandlers;
        private readonly IEnumerable<IBotCallbackQuery> _callbackQueries;


        public BotCommandResolver(
            ILogger<BotCommandResolver> logger,
            ITelegramBotClient telegramBotClient,
            StorageService storageService,
            IEnumerable<IBotCommand> commands,
            IEnumerable<IBotStateHandler> stateHandlers,
            IEnumerable<IBotCallbackQuery> callbackQueries)
        {
            _logger = logger;
            _telegramBotClient = telegramBotClient;
            _storageService = storageService;
            _commands = commands;
            _stateHandlers = stateHandlers;
            _callbackQueries = callbackQueries;
        }
        
        public async Task StartCommandAsync(Message message)
        {
            var isCommand = _commands.TryGetCommand(message.Text, out var command);
            
            try
            {
                if (isCommand)
                {
                    _logger.LogInformation($"Start command {command.GetType().Name}. UserId={message.From.Id.ToString()}, ChatId={message.Chat.Id.ToString()}");
                    await _telegramBotClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
                    await command!.ExecuteCommandAsync(message);
                    _logger.LogInformation($"End command {command.GetType().Name}. UserId={message.From.Id.ToString()}, ChatId={message.Chat.Id.ToString()}");
                }
                else
                {
                    var state = _storageService.Get<BotStateType>(StorageKeys.BotCurrentState);
                    var isState = _stateHandlers.TryGetStateHandler(state, out var stateHandler);

                    if (isState)
                    {
                        _logger.LogInformation($"Start handler {stateHandler.GetType().Name}. UserId={message.From.Id.ToString()}, ChatId={message.Chat.Id.ToString()}, State={state.ToString()}, Payload={message.Text}");
                        await _telegramBotClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
                        await stateHandler.ExecuteHandlerAsync(message);
                        _logger.LogInformation($"End handler {stateHandler.GetType().Name}. UserId={message.From.Id.ToString()}, ChatId={message.Chat.Id.ToString()}, State={state.ToString()}, Payload={message.Text}");
                    }
                }
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, $"Error={ex.Message}, UserId={message.From.Id.ToString()}, Payload={message.Text}");

                await _telegramBotClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: CommonMessage.GetDigitalOceanApiErrorMessage(ex.Message));
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogError(ex, $"Error={ex.Message}, UserId={message.From.Id.ToString()}, Payload={message.Text}");
                
                await _telegramBotClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: CommonMessage.GetOperationCanceledErrorMessage());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error={ex.Message}, UserId={message.From.Id.ToString()}, Payload={message.Text}");
                 
                await _telegramBotClient.SendTextMessageAsync(
                    chatId:message.Chat.Id,
                    text:CommonMessage.GetCriticalErrorMessage(ex.Message));
            }
        }

        public async Task StartCallbackQueryAsync(long chatId, int messageId, string callbackQueryId, string data)
        {
            try
            {
                var callbackData = data.Split(";");
                var callbackType = Enum.Parse<BotCallbackQueryType>(callbackData.First());
                var isCallbackQuery = _callbackQueries.TryGetCallbackQuery(callbackType, out var callbackQuery);

                if (isCallbackQuery)
                {
                    var payload = callbackData.Last();
                    _logger.LogInformation($"Start callback query {callbackQuery.GetType().Name}. CallbackQuery={callbackType.ToString()}, Data={data}");
                    await callbackQuery.ExecuteCallbackQueryAsync(chatId, messageId, callbackQueryId, payload);
                    _logger.LogInformation($"End callback query {callbackQuery.GetType().Name}. CallbackQuery={callbackType.ToString()}, Data={data}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error={ex.Message}, UserId={chatId.ToString()}, Data={data}");
                 
                await _telegramBotClient.AnswerCallbackQueryAsync(
                    callbackQueryId: callbackQueryId,
                    text:CommonMessage.GetErrorMessage(),
                    showAlert:true);
            }
        }
    }
}