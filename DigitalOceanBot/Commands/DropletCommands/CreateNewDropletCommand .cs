using System;
using System.Text;
using System.Threading.Tasks;
using DigitalOcean.API.Exceptions;
using DigitalOcean.API.Models.Requests;
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
    public class CreateNewDropletCommand : IBotCommand, IBotCallback
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly IRepository<Session> _sessionRepo;
        private readonly IRepository<HandlerCallback> _handlerCallbackRepo;
        private readonly ILogger<DigitalOceanWorker> _logger;
        private readonly IPageFactory _pageFactory;
        private readonly IDigitalOceanClientFactory _digitalOceanClientFactory;

        public CreateNewDropletCommand(
            ILogger<DigitalOceanWorker> logger,
            ITelegramBotClient telegramBotClient,
            IRepository<Session> sessionRepo,
            IRepository<HandlerCallback> handlerCallbackRepo,
            IPageFactory pageFactory,
            IDigitalOceanClientFactory digitalOceanClientFactory)
        {
            _telegramBotClient = telegramBotClient;
            _sessionRepo = sessionRepo;
            _handlerCallbackRepo = handlerCallbackRepo;
            _logger = logger;
            _pageFactory = pageFactory;
            _digitalOceanClientFactory = digitalOceanClientFactory;
        }

        #region Commands

        public async void Execute(Message message, SessionState sessionState)
        {
            try
            {
                await _telegramBotClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                switch (sessionState)
                {
                    case SessionState.DropletsMenu:
                        await InputNameDroplet(message);
                        break;
                    case SessionState.WaitInputNameDroplet:
                        await ChooseImage(message);
                        break;
                    case SessionState.WaitChooseImageDroplet:
                        await ChooseRegion(message);
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

        private async Task InputNameDroplet(Message message)
        {
            _sessionRepo.Update(message.From.Id, session =>
            {
                session.State = SessionState.WaitInputNameDroplet;
            });

            await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Input name for new droplet:");
        }

        private async Task ChooseImage(Message message)
        {
            var digitalOceanApi = _digitalOceanClientFactory.GetInstance(message.From.Id);
            var images = await digitalOceanApi.Images.GetAll(ImageType.Distribution);

            _sessionRepo.Update(message.From.Id, session =>
            {
                var droplet = new Droplet
                {
                    Name = message.Text,
                };

                var createDroplet = new CreateDroplet
                {
                    Droplet = droplet,
                    Images = images
                };

                session.Data = createDroplet;
                session.State = SessionState.WaitChooseImageDroplet;
            });

            var page = _pageFactory.GetInstance<ImagePage>();
            var pageModel = page.GetPage(message.From.Id);

            var sendMessage = await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, pageModel.Message, ParseMode.Markdown, replyMarkup: pageModel.Keyboard);
            
            _handlerCallbackRepo.Update(message.From.Id, calllback =>
            {
                calllback.MessageId = sendMessage.MessageId;
                calllback.UserId = message.From.Id;
                calllback.HandlerType = this.GetType().FullName;
            });
        }

        private async Task ChooseRegion(Message message)
        {
            var digitalOceanApi = _digitalOceanClientFactory.GetInstance(message.From.Id);
            var regions = await digitalOceanApi.Regions.GetAll();

            _sessionRepo.Update(message.From.Id, session =>
            {
                var createDroplet = session.Data.CastObject<CreateDroplet>();
                createDroplet.Regions = regions;
                session.Data = createDroplet;
                session.State = SessionState.WaitChooseRegionDroplet;
            });

            var page = _pageFactory.GetInstance<RegionPage>();
            var pageModel = page.GetPage(message.From.Id);

            var sendMessage = await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, pageModel.Message, ParseMode.Markdown, replyMarkup: pageModel.Keyboard);
            
            _handlerCallbackRepo.Update(message.From.Id, calllback =>
            {
                calllback.MessageId = sendMessage.MessageId;
                calllback.UserId = message.From.Id;
                calllback.HandlerType = this.GetType().FullName;
            });
        }

        private async Task ChooseSize(Message message)
        {
            var digitalOceanApi = _digitalOceanClientFactory.GetInstance(message.From.Id);
            var sizes = await digitalOceanApi.Sizes.GetAll();

            _sessionRepo.Update(message.From.Id, session =>
            {
                var createDroplet = session.Data.CastObject<CreateDroplet>();
                createDroplet.Sizes = sizes;
                session.Data = createDroplet;
                session.State = SessionState.WaitChooseSizeDroplet;
            });

            var page = _pageFactory.GetInstance<SizePage>();
            var pageModel = page.GetPage(message.From.Id);
            
            var sendMessage = await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, pageModel.Message, ParseMode.Markdown, replyMarkup: pageModel.Keyboard);
            
            _handlerCallbackRepo.Update(message.From.Id, calllback =>
            {
                calllback.MessageId = sendMessage.MessageId;
                calllback.UserId = message.From.Id;
                calllback.HandlerType = this.GetType().FullName;
            });
        }

        private async Task CreateDroplet(Message message)
        {
            await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, $"\U0001F4C0 Create droplet...");
            var digitalOceanApi = _digitalOceanClientFactory.GetInstance(message.From.Id);
            var session = _sessionRepo.Get(message.From.Id);
            var droplet = session.Data.CastObject<CreateDroplet>().Droplet;
            var createdDroplet = await digitalOceanApi.Droplets.Create(droplet);
            if (createdDroplet != null)
            {
                _sessionRepo.Update(message.From.Id, session =>
                {
                    session.State = SessionState.MainMenu;
                });

                var stringBuilder = new StringBuilder();

                stringBuilder.Append($"*All Done!* \U0001F973 \n\n");
                stringBuilder.Append($"Name: *{createdDroplet.Name} (Created at {createdDroplet.CreatedAt.ToString("dd:MM:yyyy HH:mm:ss")})*\n");
                stringBuilder.Append($"Image: *{createdDroplet.Image.Distribution} {createdDroplet.Image.Name}*\n");
                stringBuilder.Append($"vCPUs: *{createdDroplet.Vcpus.ToString()}*\n");
                stringBuilder.Append($"Memory: *{createdDroplet.Memory.ToString()}MB*\n");
                stringBuilder.Append($"Disk: *{createdDroplet.Disk.ToString()}GB*\n");
                stringBuilder.Append($"Region: *{createdDroplet.Region.Name}*\n");

                await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, stringBuilder.ToString(), ParseMode.Markdown, replyMarkup: Keyboards.GetMainMenuKeyboard());
            }
        }

        #endregion
        
        #region Callbacks

        public async void Execute(CallbackQuery callback, Message message, SessionState sessionState)
        {
            try
            {
                await _telegramBotClient.SendChatActionAsync(callback.From.Id, ChatAction.Typing);
                var callBackData = callback.Data.Split(';');

                switch (sessionState)
                {
                    case SessionState.WaitChooseImageDroplet when callBackData[0] == "NextImage" || callBackData[0] == "BackImage":
                        await NextOrBackImage(callback, message);
                        break;
                    case SessionState.WaitChooseImageDroplet when callBackData[0] == "SelectImage":
                        await SelectImage(callback, message);
                        break;
                    case SessionState.WaitChooseRegionDroplet when callBackData[0] == "NextRegion" || callBackData[0] == "BackRegion":
                        await NextOrBackRegion(callback, message);
                        break;
                    case SessionState.WaitChooseRegionDroplet when callBackData[0] == "SelectRegion":
                        await SelectRegion(callback, message);
                        break;
                    case SessionState.WaitChooseSizeDroplet when callBackData[0] == "NextSize" || callBackData[0] == "BackSize":
                        await NextOrBackSize(callback, message);
                        break;
                    case SessionState.WaitChooseSizeDroplet when callBackData[0] == "SelectSize":
                        await SelectSize(callback, message);
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

        private async Task NextOrBackImage(CallbackQuery callback, Message message)
        {
            var pageCount = int.Parse(callback.Data.Split(';')[1]);
            var page = _pageFactory.GetInstance<ImagePage>();
            var pageModel = page.GetPage(callback.From.Id, pageCount);

            await _telegramBotClient.EditMessageTextAsync(message.Chat.Id, message.MessageId, pageModel.Message, ParseMode.Markdown, replyMarkup: pageModel.Keyboard);
        }

        private async Task SelectImage(CallbackQuery callback, Message message)
        {
            var imageId = int.Parse(callback.Data.Split(';')[1]);
            var page = _pageFactory.GetInstance<ImagePage>();
            var pageModel = page.SelectPage(callback.From.Id, imageId);

            _sessionRepo.Update(callback.From.Id, session =>
            {
                var createDroplet = session.Data.CastObject<CreateDroplet>();
                createDroplet.Droplet.Image = imageId;
                createDroplet.Images = null;
                session.Data = createDroplet;
            });

            await _telegramBotClient.EditMessageTextAsync(message.Chat.Id, message.MessageId, pageModel.Message, ParseMode.Markdown);
            message.From = callback.From;

            await ChooseRegion(message);
        }

        private async Task NextOrBackRegion(CallbackQuery callback, Message message)
        {
            var pageCount = int.Parse(callback.Data.Split(';')[1]);
            var page = _pageFactory.GetInstance<RegionPage>();
            var pageModel = page.GetPage(callback.From.Id, pageCount);

            await _telegramBotClient.EditMessageTextAsync(message.Chat.Id, message.MessageId, pageModel.Message, ParseMode.Markdown, replyMarkup: pageModel.Keyboard);
        }

        private async Task SelectRegion(CallbackQuery callback, Message message)
        {
            var regionId = callback.Data.Split(';')[1]?.ToString();
            var page = _pageFactory.GetInstance<RegionPage>();

            var pageModel = page.SelectPage(callback.From.Id, regionId);

            _sessionRepo.Update(callback.From.Id, session =>
            {
                var createDroplet = session.Data.CastObject<CreateDroplet>();
                createDroplet.Droplet.Region = regionId;
                createDroplet.Regions = null;
                session.Data = createDroplet;
            });

            await _telegramBotClient.EditMessageTextAsync(message.Chat.Id, message.MessageId, pageModel.Message, ParseMode.Markdown);
            message.From = callback.From;
            
            await ChooseSize(message);
        }

        private async Task NextOrBackSize(CallbackQuery callback, Message message)
        {
            var pageCount = int.Parse(callback.Data.Split(';')[1]);
            var page = _pageFactory.GetInstance<SizePage>();
            var pageModel = page.GetPage(callback.From.Id, pageCount);

            await _telegramBotClient.EditMessageTextAsync(message.Chat.Id, message.MessageId, pageModel.Message, ParseMode.Markdown, replyMarkup: pageModel.Keyboard);
        }

        private async Task SelectSize(CallbackQuery callback, Message message)
        {
            var sizeId = callback.Data.Split(';')[1]?.ToString();
            var page = _pageFactory.GetInstance<SizePage>();

            var pageModel = page.SelectPage(callback.From.Id, sizeId);

            _sessionRepo.Update(callback.From.Id, session =>
            {
                var createDroplet = session.Data.CastObject<CreateDroplet>();
                createDroplet.Droplet.Size = sizeId;
                createDroplet.Sizes = null;
                session.Data = createDroplet;
            });

            await _telegramBotClient.EditMessageTextAsync(message.Chat.Id, message.MessageId, pageModel.Message, ParseMode.Markdown);
            message.From = callback.From;
            
            await CreateDroplet(message);
        }

        #endregion
    }
}
