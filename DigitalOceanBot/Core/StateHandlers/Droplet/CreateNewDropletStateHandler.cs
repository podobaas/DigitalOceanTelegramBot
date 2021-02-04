using System;
using System.Linq;
using System.Threading.Tasks;
using DigitalOcean.API;
using DigitalOcean.API.Models.Requests;
using DigitalOceanBot.Messages;
using DigitalOceanBot.Services;
using DigitalOceanBot.Services.Paginators;
using DigitalOceanBot.Types.Attributes;
using DigitalOceanBot.Types.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using DropletRequest = DigitalOcean.API.Models.Requests.Droplet;

namespace DigitalOceanBot.Core.StateHandlers.Droplet
{
    [BotStateHandler(BotStateType.DropletCreateWaitingEnterName)]
    [BotStateHandler(BotStateType.DropletCreateWaitingEnterImage)]
    [BotStateHandler(BotStateType.DropletCreateWaitingEnterRegion)]
    [BotStateHandler(BotStateType.DropletCreateWaitingEnterSize)]
    public sealed class CreateNewDropletStateHandler: IBotStateHandler
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly IDigitalOceanClient _digitalOceanClient;
        private readonly StorageService _storageService;
        private readonly ImagePaginatorService _imagePaginatorService;
        private readonly RegionPaginatorService _regionPaginatorService;
        private readonly SizePaginatorService _sizePaginatorService;

        public CreateNewDropletStateHandler(
            ITelegramBotClient telegramBotClient,
            IDigitalOceanClient digitalOceanClient,
            StorageService storageService,
            ImagePaginatorService imagePaginatorService,
            RegionPaginatorService regionPaginatorService,
            SizePaginatorService sizePaginatorService)
        {
            _telegramBotClient = telegramBotClient;
            _digitalOceanClient = digitalOceanClient;
            _storageService = storageService;
            _imagePaginatorService = imagePaginatorService;
            _regionPaginatorService = regionPaginatorService;
            _sizePaginatorService = sizePaginatorService;
        }
        
        public async Task ExecuteHandlerAsync(Message message)
        {
            _imagePaginatorService.OnSelectCallback = async () => await ExecuteHandlerAsync(message);
            _regionPaginatorService.OnSelectCallback = async () => await ExecuteHandlerAsync(message);
            _sizePaginatorService.OnSelectCallback = async () => await ExecuteHandlerAsync(message);
            
            var state = _storageService.Get<BotStateType>(StorageKeys.BotCurrentState);

            switch (state)
            {
                case BotStateType.DropletCreateWaitingEnterName:
                    await EnterName(message);
                    break;
                case BotStateType.DropletCreateWaitingEnterImage:
                    await EnterImage(message);
                    break;
                case BotStateType.DropletCreateWaitingEnterRegion:
                    await EnterRegion(message);
                    break;
                case BotStateType.DropletCreateWaitingEnterSize:
                    await Create(message);
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
            var distributionImages = await _digitalOceanClient.Images.GetAll(ImageType.Distribution);
            var privateImages = await _digitalOceanClient.Images.GetAll(ImageType.Private);
            distributionImages.ToList().AddRange(privateImages.ToList());
            _storageService.AddOrUpdate(StorageKeys.Images, distributionImages);
            var paginator = _imagePaginatorService.GetPage(0);

            _storageService.AddOrUpdate(StorageKeys.BotCurrentState, BotStateType.DropletCreateWaitingEnterImage);
            
            await _telegramBotClient.SendTextMessageAsync(
                chatId:message.Chat.Id, 
                text:paginator.MessageText,
                parseMode:ParseMode.Markdown,
                replyMarkup:paginator.Keyboard);
        }

        private async Task EnterImage(Message message)
        {
            var droplet = _storageService.Get<DropletRequest>(StorageKeys.NewDroplet);
            droplet.Image = _storageService.Get<long>(StorageKeys.ImageId);;
            var regions = await _digitalOceanClient.Regions.GetAll();
            _storageService.AddOrUpdate(StorageKeys.Regions, regions);
            var paginator = _regionPaginatorService.GetPage(0);
            
            _storageService.AddOrUpdate(StorageKeys.BotCurrentState, BotStateType.DropletCreateWaitingEnterRegion);
            
            await _telegramBotClient.SendTextMessageAsync(
                chatId:message.Chat.Id, 
                text:paginator.MessageText,
                parseMode:ParseMode.Markdown,
                replyMarkup:paginator.Keyboard);
        }
        
        private async Task EnterRegion(Message message)
        {
            var droplet = _storageService.Get<DropletRequest>(StorageKeys.NewDroplet);
            droplet.Region = _storageService.Get<string>(StorageKeys.RegionId);
            
            var sizes = await _digitalOceanClient.Sizes.GetAll();
            _storageService.AddOrUpdate(StorageKeys.Sizes, sizes);
            
            _storageService.AddOrUpdate(StorageKeys.BotCurrentState, BotStateType.DropletCreateWaitingEnterSize);
            var paginator = _sizePaginatorService.GetPage(0);
            
            await _telegramBotClient.SendTextMessageAsync(
                chatId:message.Chat.Id, 
                text:paginator.MessageText,
                parseMode:ParseMode.Markdown,
                replyMarkup:paginator.Keyboard);
        }

        private async Task Create(Message message)
        {
            var droplet = _storageService.Get<DropletRequest>(StorageKeys.NewDroplet);
            droplet.Size = _storageService.Get<string>(StorageKeys.SizeId);
            
            await _telegramBotClient.SendTextMessageAsync(
                chatId:message.Chat.Id, 
                text:CommonMessage.GetCreatingMessage());

            await _digitalOceanClient.Droplets.Create(droplet);
            
            await _telegramBotClient.SendTextMessageAsync(
                chatId:message.Chat.Id, 
                text:CommonMessage.GetDoneMessage());
            
            _storageService.AddOrUpdate(StorageKeys.BotCurrentState, BotStateType.None);

            _imagePaginatorService.OnSelectCallback = null;
            _regionPaginatorService.OnSelectCallback = null;
            _sizePaginatorService.OnSelectCallback = null;
        }
    }
}