using System;
using System.Linq;
using System.Threading.Tasks;
using DigitalOcean.API.Exceptions;
using DigitalOcean.API.Models.Requests;
using DigitalOceanBot.Factory;
using DigitalOceanBot.MongoDb;
using DigitalOceanBot.MongoDb.Models;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using DigitalOceanBot.CheckLists;
using Droplet = DigitalOcean.API.Models.Responses.Droplet;

namespace DigitalOceanBot.Commands.FirewallCommands
{
    public class AddDropletsCommand : IBotCommand, IBotCallback
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly IRepository<Session> _sessionRepo;
        private readonly IRepository<HandlerCallback> _handlerCallbackRepo;
        private readonly ILogger<DigitalOceanWorker> _logger;
        private readonly IDigitalOceanClientFactory _digitalOceanClientFactory;
        private readonly ICheckListPageFactory _checkListPageFactory;

        public AddDropletsCommand(
            ILogger<DigitalOceanWorker> logger,
            ITelegramBotClient telegramBotClient,
            IRepository<Session> sessionRepo,
            IRepository<HandlerCallback> handlerCallbackRepo,
            IDigitalOceanClientFactory digitalOceanClientFactory,
            ICheckListPageFactory checkListPageFactory)
        {
            _logger = logger;
            _telegramBotClient = telegramBotClient;
            _sessionRepo = sessionRepo;
            _handlerCallbackRepo = handlerCallbackRepo;
            _digitalOceanClientFactory = digitalOceanClientFactory;
            _checkListPageFactory = checkListPageFactory;
        }


        #region Commands

        public async void Execute(Message message, SessionState sessionState)
        {
            try
            {
                await _telegramBotClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                switch (sessionState)
                {
                    case SessionState.SelectedFirewall:
                        await GetDroplets(message);
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
            _sessionRepo.Update(message.From.Id, session =>
            {
                session.State = SessionState.WaitChooseDropletsForFirewall;
            });

            var digitalOceanApi = _digitalOceanClientFactory.GetInstance(message.From.Id);
            var droplets = await digitalOceanApi.Droplets.GetAll();
            if(droplets.Count == 0)
            {
                await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "You dont have a droplets \U0001F914", ParseMode.Markdown);
                return;
            }

            var checkListPage = _checkListPageFactory.GetInstance<Droplet, DropletCheckList>();
            var pageModel = checkListPage.GetCheckListPage(droplets);
            
            var sendMessage = await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, pageModel.Message, replyMarkup:pageModel.Keyboard);
                
            _handlerCallbackRepo.Update(message.From.Id, calllback =>
            {
                calllback.MessageId = sendMessage.MessageId;
                calllback.UserId = message.From.Id;
                calllback.HandlerType = this.GetType().FullName;
            });
        }
        
        #endregion

        public async void Execute(CallbackQuery callback, Message message, SessionState sessionState)
        {
            try
            {
                await _telegramBotClient.SendChatActionAsync(callback.From.Id, ChatAction.Typing);
                var callBackData = callback.Data.Split(';');

                switch (sessionState)
                {
                    case SessionState.WaitChooseDropletsForFirewall when callBackData[0] == "SelectDroplet":
                        await SelectDropletForFirewall(callback, message);
                        break;
                    case SessionState.WaitChooseDropletsForFirewall when callBackData[0] == "Ok":
                        await AddDropletsToFirewall(callback, message);
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
        
        private async Task SelectDropletForFirewall(CallbackQuery callback, Message message)
        {
            var dropletId = callback.Data.Split(';')[1];
            var checkListPage = _checkListPageFactory.GetInstance<Droplet, DropletCheckList>();
            var keyboard = checkListPage.SelectItem(message.ReplyMarkup, dropletId);
            
            await _telegramBotClient.EditMessageReplyMarkupAsync(message.Chat.Id, message.MessageId, keyboard);
        }
        
        private async Task AddDropletsToFirewall(CallbackQuery callback, Message message)
        {
            await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "\U0001F4C0 Adding droplets...");
            
            var session = _sessionRepo.Get(callback.From.Id);
            var digitalOceanApi = _digitalOceanClientFactory.GetInstance(callback.From.Id);
            var checkListPage = _checkListPageFactory.GetInstance<Droplet, DropletCheckList>();
            var dropletIds = checkListPage.GetSelectedItems(message.ReplyMarkup).Select(int.Parse).ToList();

            if (dropletIds.Any())
            {
                await digitalOceanApi.Firewalls.AddDroplets((string) session.Data, new FirewallDroplets
                {
                    DropletIds = dropletIds
                });
                
                await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Done \U00002705");
            }
        }
    }
}
