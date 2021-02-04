using System;
using System.Threading;
using System.Threading.Tasks;
using DigitalOceanBot.Core;
using DigitalOceanBot.Keyboards;
using DigitalOceanBot.Messages;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

namespace DigitalOceanBot.Workers
{
    internal sealed class DigitalOceanWorker : IHostedService
    {
        private readonly BotCommandResolver _botCommandResolver;
        private readonly ILogger<DigitalOceanWorker> _logger;
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly int _userId;
        
        public DigitalOceanWorker(
            BotCommandResolver botCommandResolver, 
            ITelegramBotClient telegramBotClient, 
            ILogger<DigitalOceanWorker> logger)
        {
            _botCommandResolver = botCommandResolver;
            _logger = logger;
            _telegramBotClient = telegramBotClient;
            _userId = EnvironmentVars.GetUserId();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Starting bot...");
            
                _telegramBotClient.OnMessage += OnMessage;
                _telegramBotClient.OnCallbackQuery += OnCallbackQuery;
                _telegramBotClient.StartReceiving(new[]
                {
                    UpdateType.CallbackQuery, 
                    UpdateType.Message
                }, cancellationToken);
            
                await TrySendStartMessage();
            
                _logger.LogInformation("Bot started successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed start: {ex.Message}");
            }
        }
        
        public Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Stopping bot...");

                _telegramBotClient?.StopReceiving();

                _logger.LogInformation("Bot stopped!");

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed stop: {ex.Message}");
            }
            
            return Task.CompletedTask;
        }

        private async void OnMessage(object sender, MessageEventArgs e)
        {
            if (e.Message.Type == MessageType.Text && e.Message.From.Id == _userId)
            {
                await _botCommandResolver.StartCommandAsync(e.Message);
            }
        }

        private async void OnCallbackQuery(object sender, CallbackQueryEventArgs e)
        {
            if (e.CallbackQuery.From.Id == _userId)
            {
                await _botCommandResolver.StartCallbackQueryAsync(
                    chatId:e.CallbackQuery.Message.Chat.Id, 
                    messageId:e.CallbackQuery.Message.MessageId, 
                    callbackQueryId:e.CallbackQuery.Id, 
                    data:e.CallbackQuery.Data);
            }
        }

        private async Task TrySendStartMessage()
        {
            try
            {
                await _telegramBotClient.SendTextMessageAsync(
                    chatId:_userId, 
                    text:CommonMessage.GetMainMenuMessage(), 
                    replyMarkup: CommonKeyboard.GetMainMenuKeyboard());
            }
            catch (Exception ex)
            {
                if (ex.Message != "Forbidden: bot was blocked by the user")
                {
                    _logger.LogError($"Error while trying to send message: {ex.Message}");
                    throw;
                }
            }
        }
    }
}
