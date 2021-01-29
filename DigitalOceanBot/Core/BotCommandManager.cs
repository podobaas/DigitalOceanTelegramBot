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
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DigitalOceanBot.Core
{
    public class BotCommandManager
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly StorageService _storageService;
        private Dictionary<string, Dictionary<CommandType, ICommand>> _commands = new();
        private Dictionary<StateType, IStateHandler> _states = new();
        private Dictionary<CallbackQueryType, ICallbackQuery> _callbacks = new();
        
        
        public BotCommandManager(
            IServiceProvider serviceProvider,
            ILogger logger,
            ITelegramBotClient telegramBotClient,
            StorageService storageService)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _telegramBotClient = telegramBotClient;
            _storageService = storageService;
        }

        public BotCommandManager OnCommand(CommandType commandType, Type botCommandType)
        {
            var command = (ICommand) ActivatorUtilities.CreateInstance(_serviceProvider, botCommandType);
            var commandString = commandType.GetStringCommand();
            _commands.Add(commandString, new Dictionary<CommandType, ICommand>
            {
                {commandType, command}
            });
            
            return this;
        }
        
        public BotCommandManager OnState(StateType stateType, Type stateHandlerType)
        {
            var handler = (IStateHandler) ActivatorUtilities.CreateInstance(_serviceProvider, stateHandlerType);
            _states.Add(stateType, handler);
            return this;
        }
        
        public BotCommandManager OnCallbackQuery(CallbackQueryType callbackType, Type stateHandlerType)
        {
            var handler = (ICallbackQuery) ActivatorUtilities.CreateInstance(_serviceProvider, stateHandlerType);
            _callbacks.Add(callbackType, handler);
            return this;
        }
        
        public async Task StartCommandAsync(Message message)
        {
            var isCommand = _commands.ContainsKey(message.Text);
            
            try
            {
                if (isCommand)
                {
                    var commandType = _commands[message.Text].Keys.FirstOrDefault();
                    var command = _commands[message.Text].Values.FirstOrDefault();

                    _logger.Information($"Start command {commandType}. UserId={message.From.Id}, ChatId={message.Chat.Id}");
                    await _telegramBotClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
                    await command!.ExecuteCommandAsync(message);
                    _logger.Information($"End command {commandType}. UserId={message.From.Id}, ChatId={message.Chat.Id}");
                }
                else
                {
                    var state = _storageService.Get<StateType>(StorageKeys.BotCurrentState);

                    if (state is not StateType.None)
                    {
                        _logger.Information($"Start handler. UserId={message.From.Id}, ChatId={message.Chat.Id}, State={state}, Payload={message.Text}");
                        await _telegramBotClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
                        await _states[state].ExecuteHandlerAsync(message);
                        _logger.Information($"End handler. UserId={message.From.Id}, ChatId={message.Chat.Id}, State={state}, Payload={message.Text}");
                    }
                }
            }
            catch (ApiException ex)
            {
                _logger.Error(ex, $"Error={ex.Message}, UserId={message.From.Id}, Payload={message.Text}");

                await _telegramBotClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: CommonMessage.GetDigitalOceanApiErrorMessage(ex.Message));
            }
            catch (OperationCanceledException ex)
            {
                _logger.Error(ex, $"Error={ex.Message}, UserId={message.From.Id}, Payload={message.Text}");
                
                await _telegramBotClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: CommonMessage.GetOperationCanceledErrorMessage());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error={ex.Message}, UserId={message.From.Id}, Payload={message.Text}");
                 
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
                var type = Enum.Parse<CallbackQueryType>(callbackData.First());

                if (type is not CallbackQueryType.None)
                {

                    var payload = callbackData.Last();
                    _logger.Information($"Start callback query {type}. Data={data}");
                    await _callbacks[type].ExecuteCallbackQueryAsync(chatId, messageId, callbackQueryId, payload);
                    _logger.Information($"End callback query {type}. Data={data}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error={ex.Message}, UserId={chatId}, Data={data}");
                 
                await _telegramBotClient.AnswerCallbackQueryAsync(
                    callbackQueryId: callbackQueryId,
                    text:CommonMessage.GetErrorMessage(),
                    showAlert:true);
            }
        }
    }
}