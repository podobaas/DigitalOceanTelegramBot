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

namespace DigitalOceanBot.Commands.FirewallCommands
{
    public class GetFirewallsCommand : IBotCommand, IBotCallback
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly IRepository<Session> _sessionRepo;
        private readonly IRepository<HandlerCallback> _handlerCallbackRepo;
        private readonly ILogger<DigitalOceanWorker> _logger;
        private readonly IPageFactory _pageFactory;
        private readonly IDigitalOceanClientFactory _digitalOceanClientFactory;

        public GetFirewallsCommand(
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
                if (sessionState == SessionState.FirewallsMenu || sessionState == SessionState.MainMenu)
                {
                    await GetFirewalls(message).ConfigureAwait(false);
                }
            }
            catch (ApiException ex)
            {
                _logger.LogError($"UserId={message.From.Id.ToString()}, Error={ex.Message}");
                await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, $"DigitalOcean API Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"UserId={message.From.Id.ToString()}, Error={ex.Message}, StackTrace={ex.StackTrace}");
                await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Sorry, аn error has occurred \U0001F628");
            }
        }

        private async Task GetFirewalls(Message message)
        {
            await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "\U0001F4C0 Loading your firewalls...", replyMarkup: Keyboards.GetFirewallMenuKeyboard());
            var digitalOceanApi = _digitalOceanClientFactory.GetInstance(message.From.Id);
            var firewalls = await digitalOceanApi.Firewalls.GetAll();
            
            if (firewalls.Count > 0)
            {
                _sessionRepo.Update(message.From.Id, session =>
                {
                    session.Data = firewalls;
                    session.State = SessionState.FirewallsMenu;
                });

                var droplets = await digitalOceanApi.Droplets.GetAll();
                var page = _pageFactory.GetInstance<FirewallPage>(droplets);
                var pageModel = page.GetPage(message.From.Id, 0);
                
                var sendMessage = await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, pageModel.Message, ParseMode.Markdown, replyMarkup: pageModel.Keyboard);
                
                _handlerCallbackRepo.Update(message.From.Id, callback =>
                {
                    callback.MessageId = sendMessage.MessageId;
                    callback.UserId = message.From.Id;
                    callback.HandlerType = GetType().FullName;
                });
            }
            else
            {
                await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "You dont have a firewalls \U0001F914");
            }
        }

        #endregion


        #region Callbacks

        public async Task Execute(CallbackQuery callback, Message message, SessionState sessionState)
        {
            try
            {
                var callBackData = callback.Data.Split(';');

                switch (sessionState)
                {
                    case SessionState.FirewallsMenu when callBackData[0] == "NextFirewall" || callBackData[0] == "BackFirewall":
                        await NextOrBackFirewall(callback, message).ConfigureAwait(false);
                        break;
                    case SessionState.FirewallsMenu when callBackData[0] == "SelectFirewall":
                        await SelectFirewall(callback, message).ConfigureAwait(false);
                        break;
                }
            }
            catch (ApiException ex)
            {
                _logger.LogError($"UserId={message.From.Id.ToString()}, Error={ex.Message}");
                await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, $"DigitalOcean API Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"UserId={message.From.Id.ToString()}, Error={ex.Message}, StackTrace={ex.StackTrace}");
                await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Sorry, аn error has occurred \U0001F628");
            }
        }

        private async Task NextOrBackFirewall(CallbackQuery callback, Message message)
        {
            var digitalOceanApi = _digitalOceanClientFactory.GetInstance(callback.From.Id);
            var pageCount = int.Parse(callback.Data.Split(';')[1]);
            var droplets = await digitalOceanApi.Droplets.GetAll();
            var page = _pageFactory.GetInstance<FirewallPage>(droplets);
            var pageModel = page.GetPage(callback.From.Id, pageCount);

            await _telegramBotClient.EditMessageTextAsync(message.Chat.Id, message.MessageId, pageModel.Message, ParseMode.Markdown, replyMarkup: pageModel.Keyboard);
        }

        private async Task SelectFirewall(CallbackQuery callback, Message message)
        {
            var firewallId = callback.Data.Split(';')[1];
            var page = _pageFactory.GetInstance<FirewallPage>();
            var pageModel = page.SelectPage(callback.From.Id, firewallId);

            _sessionRepo.Update(callback.From.Id, session =>
            {
                session.State = SessionState.SelectedFirewall;
                session.Data = firewallId;
            });
            
            await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, $"Good! Now select one of the actions for this firewall", replyMarkup: Keyboards.GetSelectedFirewallMenuKeyboard());
            await _telegramBotClient.EditMessageTextAsync(message.Chat.Id, message.MessageId, pageModel.Message, ParseMode.Markdown);
        }

        #endregion
    }
}
