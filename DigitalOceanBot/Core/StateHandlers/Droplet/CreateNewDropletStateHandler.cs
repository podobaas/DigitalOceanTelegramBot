using System;
using System.Threading.Tasks;
using DigitalOcean.API;
using DigitalOcean.API.Models.Requests;
using DigitalOceanBot.Core.Attributes;
using DigitalOceanBot.Services;
using DigitalOceanBot.Services.Paginators;
using DigitalOceanBot.Types.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using DropletRequest = DigitalOcean.API.Models.Requests.Droplet;

namespace DigitalOceanBot.Core.StateHandlers.Droplet
{
    [BotStateHandler(BotStateType.DropletCreateWaitingEnterName)]
    [BotStateHandler(BotStateType.DropletCreateWaitingEnterImage)]
    public sealed class CreateNewDropletStateHandler: IBotStateHandler
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly IDigitalOceanClient _digitalOceanClient;
        private readonly StorageService _storageService;
        private readonly ImagePaginatorService _imagePaginatorService;

        public CreateNewDropletStateHandler(
            ITelegramBotClient telegramBotClient,
            IDigitalOceanClient digitalOceanClient,
            StorageService storageService,
            ImagePaginatorService imagePaginatorService)
        {
            _telegramBotClient = telegramBotClient;
            _digitalOceanClient = digitalOceanClient;
            _storageService = storageService;
            _imagePaginatorService = imagePaginatorService;
        }
        
        public async Task ExecuteHandlerAsync(Message message)
        {
            var state = _storageService.Get<BotStateType>(StorageKeys.BotCurrentState);

            switch (state)
            {
                case BotStateType.DropletCreateWaitingEnterName:
                    await EnterName(message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state));
            }
        }
        
        private async Task EnterName(Message message)
        {
            var droplet = new DropletRequest
            {
                Name = message.Text
            };
            
            _storageService.AddOrUpdate(StorageKeys.NewDroplet, droplet);
            
            var images = await _digitalOceanClient.Images.GetAll(ImageType.Distribution);
            _storageService.AddOrUpdate(StorageKeys.Images, images);
            
            var paginator = _imagePaginatorService.GetPage(0);

            await _telegramBotClient.SendTextMessageAsync(
                chatId:message.Chat.Id, 
                text:paginator.MessageText,
                parseMode:ParseMode.Markdown,
                replyMarkup:paginator.Keyboard);
        }

        private async Task EnterRegion(Message message)
        {
            var droplet = _storageService.Get<DropletRequest>(StorageKeys.NewDroplet);
            droplet.Image = _storageService.Get<long>(StorageKeys.ImageId);;
            
            var regions = await _digitalOceanClient.Regions.GetAll();
            _storageService.AddOrUpdate(StorageKeys.Regions, regions);
            
            var paginator = _imagePaginatorService.GetPage(0);
            _storageService.AddOrUpdate(StorageKeys.NewDroplet, droplet);
            _storageService.AddOrUpdate(StorageKeys.BotCurrentState, BotStateType.DropletCreateWaitingEnterImage);
        }
    }
}