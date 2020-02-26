using System;
using System.Threading.Tasks;
using DigitalOcean.API.Exceptions;
using DigitalOceanBot.Factory;
using DigitalOceanBot.Helpers;
using DigitalOceanBot.MongoDb;
using DigitalOceanBot.MongoDb.Models;
using DigitalOceanBot.Pages;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DigitalOceanBot.Commands.DropletCommands
{
    public class GetDropletsCommand : IBotCommand, IBotCallback
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly IRepository<Session> _sessionRepo;
        private readonly IRepository<HandlerCallback> _handlerCallbackRepo;
        private readonly ILogger<DigitalOceanWorker> _logger;
        private readonly IPageFactory _pageFactory;
        private readonly IDigitalOceanClientFactory _digitalOceanClientFactory;

        public GetDropletsCommand(
            ILogger<DigitalOceanWorker> logger,
            ITelegramBotClient telegramBotClient,
            IRepository<Session> sessionRepo,
            IRepository<HandlerCallback> handlerCallbackRepo,
            IPageFactory pageFactory,
            IDigitalOceanClientFactory digitalOceanClientFactory)
        {
            _logger = logger;
            _telegramBotClient = telegramBotClient;
            _sessionRepo = sessionRepo;
            _handlerCallbackRepo = handlerCallbackRepo;
            _pageFactory = pageFactory;
            _digitalOceanClientFactory = digitalOceanClientFactory;
        }


        #region Commands

        public async Task Execute(Message message, SessionState sessionState)
        {
            try
            {
                await _telegramBotClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                switch (sessionState)
                {
                    case SessionState.DropletsMenu:
                    case SessionState.MainMenu:
                        await GetDroplets(message).ConfigureAwait(false);;
                        break;

                }
            }
            catch (ApiException ex)
            {
                _logger.LogError($"UserId={message.From.Id.ToString()}, Error={ex.Message}");
                await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, $"DigitalOcean API Error: {ex.Message.Replace(".", "\\.")}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"UserId={message.From.Id.ToString()}, Error={ex.Message}, StackTrace={ex.StackTrace}");
                await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Sorry, аn error has occurred \U0001F628");
            }
        }

        private async Task GetDroplets(Message message)
        {
            await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "\U0001F4C0 Loading your droplets...", replyMarkup: Keyboards.GetDropletsMenuKeyboard());
            var digitalOceanApi = _digitalOceanClientFactory.GetInstance(message.From.Id);
            var droplets = await digitalOceanApi.Droplets.GetAll();

            if (droplets.Count > 0)
            {
                _sessionRepo.Update(message.From.Id, session =>
                {
                    session.Data = droplets;
                    session.State = SessionState.DropletsMenu;
                });

                var page = _pageFactory.GetInstance<DropletPage>();
                var pageModel = page.GetPage(message.From.Id);

                var sendMessage = await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, pageModel.Message, ParseMode.Markdown, replyMarkup: pageModel.Keyboard);
                
                _handlerCallbackRepo.Update(message.From.Id, calllback =>
                {
                    calllback.MessageId = sendMessage.MessageId;
                    calllback.UserId = message.From.Id;
                    calllback.HandlerType = GetType().FullName;
                });
            }
            else
            {
                await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "You dont have a droplets \U0001F914");
            }
        }

        #endregion
        
        #region Callbacks

        public async Task Execute(CallbackQuery callback, Message message, SessionState sessionState)
        {
            try
            {
                await _telegramBotClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
                var callBackData = callback.Data.Split(';');

                switch (sessionState)
                {
                    case SessionState.DropletsMenu when callBackData[0] == "NextDroplet" || callBackData[0] == "BackDroplet":
                        await NextOrBackDroplet(callback, message).ConfigureAwait(false);
                        break;
                    case SessionState.DropletsMenu when callBackData[0] == "SelectDroplet":
                        await SelectDroplet(callback, message).ConfigureAwait(false);
                        break;
                }
            }
            catch (ApiException ex)
            {
                _logger.LogError($"UserId={message.From.Id.ToString()}, Error={ex.Message}");
                await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, $"DigitalOcean API Error: {ex.Message.Replace(".", "\\.")}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"UserId={message.From.Id.ToString()}, Error={ex.Message}, StackTrace={ex.StackTrace}");
                await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Sorry, аn error has occurred \U0001F628");
            }
        }

        private async Task SelectDroplet(CallbackQuery callback, Message message)
        {
            var dropletId = int.Parse(callback.Data.Split(';')[1]);
            var page = _pageFactory.GetInstance<DropletPage>();
            var pageModel = page.SelectPage(callback.From.Id, dropletId);

            _sessionRepo.Update(callback.From.Id, session =>
            {
                session.State = SessionState.SelectedDroplet;
                session.Data = dropletId;
            });
            
            await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, $"Good! Now select one of the actions for this droplet", ParseMode.Markdown, replyMarkup: Keyboards.GetSelectedDropletsMenuKeyboard());
            await _telegramBotClient.EditMessageTextAsync(message.Chat.Id, message.MessageId, pageModel.Message, ParseMode.Markdown);
        }

        private async Task NextOrBackDroplet(CallbackQuery callback, Message message)
        {
            var pageCount = int.Parse(callback.Data.Split(';')[1]);
            var page = _pageFactory.GetInstance<DropletPage>();
            var pageModel = page.GetPage(callback.From.Id, pageCount);

            await _telegramBotClient.EditMessageTextAsync(message.Chat.Id, message.MessageId, pageModel.Message, ParseMode.Markdown, replyMarkup: pageModel.Keyboard);
        }

        #endregion
    }
}
