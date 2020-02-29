using System;
using System.Linq;
using System.Threading.Tasks;
using DigitalOcean.API.Exceptions;
using DigitalOceanBot.CheckLists;
using DigitalOceanBot.Factory;
using DigitalOceanBot.MongoDb;
using DigitalOceanBot.MongoDb.Models;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Requests = DigitalOcean.API.Models.Requests;
using Responses = DigitalOcean.API.Models.Responses;

namespace DigitalOceanBot.Commands.FirewallCommands
{
    public class RemoveDropletsCommand : IBotCommand, IBotCallback
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly IRepository<Session> _sessionRepo;
        private readonly IRepository<HandlerCallback> _handlerCallbackRepo;
        private readonly ILogger<DigitalOceanWorker> _logger;
        private readonly IDigitalOceanClientFactory _digitalOceanClientFactory;
        private readonly ICheckListPageFactory _checkListPageFactory;

        public RemoveDropletsCommand(
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

        public async Task Execute(Message message, SessionState sessionState)
        {
            try
            {
                if (sessionState == SessionState.SelectedFirewall)
                {
                    await GetDroplets(message).ConfigureAwait(false);
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

        private async Task GetDroplets(Message message)
        {
            _sessionRepo.Update(message.From.Id, session =>
            {
                session.State = SessionState.WaitChooseDropletsForFirewall;
            });

            var fireWallId = (string)_sessionRepo.Get(message.From.Id).Data;
            var digitalOceanApi = _digitalOceanClientFactory.GetInstance(message.From.Id);
            var fireWall = await digitalOceanApi.Firewalls.Get(fireWallId);
            if(fireWall.DropletIds == null || fireWall.DropletIds.Count == 0)
            {
                await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Firewall don't have related droplets  \U0001F914");
                return;
            }

            var droplets = await digitalOceanApi.Droplets.GetAll();
            var checkListPage = _checkListPageFactory.GetInstance<Responses.Droplet, DropletCheckList>();
            var pageModel = checkListPage.GetCheckListPage(droplets.Where(d => fireWall.DropletIds.Contains(d.Id)), false);
            
            var sendMessage = await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, pageModel.Message, ParseMode.Markdown, replyMarkup: pageModel.Keyboard);
                
            _handlerCallbackRepo.Update(message.From.Id, callback =>
            {
                callback.MessageId = sendMessage.MessageId;
                callback.UserId = message.From.Id;
                callback.HandlerType = GetType().FullName;
            });
        }
        
        #endregion

        public async Task Execute(CallbackQuery callback, Message message, SessionState sessionState)
        {
            try
            {
                var callBackData = callback.Data.Split(';');

                switch (sessionState)
                {
                    case SessionState.WaitChooseDropletsForFirewall when callBackData[0] == "SelectDroplet":
                        await SelectDroplet(callback, message).ConfigureAwait(false);
                        break;
                    case SessionState.WaitChooseDropletsForFirewall when callBackData[0] == "Ok":
                        await RemoveDroplets(callback, message).ConfigureAwait(false);
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
        
        private async Task SelectDroplet(CallbackQuery callback, Message message)
        {
            var dropletId = callback.Data.Split(';')[1];
            var checkListPage = _checkListPageFactory.GetInstance<Responses.Droplet, DropletCheckList>();
            var keyboard = checkListPage.SelectItem(message.ReplyMarkup, dropletId);
            
            await _telegramBotClient.EditMessageReplyMarkupAsync(message.Chat.Id, message.MessageId, keyboard);
        }
        
        private async Task RemoveDroplets(CallbackQuery callback, Message message)
        {
            await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "\U0001F4C0 Removing droplets...");
            
            var session = _sessionRepo.Get(callback.From.Id);
            var digitalOceanApi = _digitalOceanClientFactory.GetInstance(callback.From.Id);
            var checkListPage = _checkListPageFactory.GetInstance<Responses.Droplet, DropletCheckList>();
            var dropletIds = checkListPage.GetSelectedItems(message.ReplyMarkup).Select(int.Parse).ToList();

            if (dropletIds.Any())
            {
                await digitalOceanApi.Firewalls.RemoveDroplets((string) session.Data, new Requests.FirewallDroplets
                {
                    DropletIds = dropletIds
                });

                await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Done \U00002705");
            }
        }
    }
}
