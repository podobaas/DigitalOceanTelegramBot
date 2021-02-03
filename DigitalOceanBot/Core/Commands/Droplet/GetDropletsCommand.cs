using System.Threading.Tasks;
using DigitalOcean.API;
using DigitalOceanBot.Core.Attributes;
using DigitalOceanBot.Keyboards;
using DigitalOceanBot.Messages;
using DigitalOceanBot.Services;
using DigitalOceanBot.Services.Paginators;
using DigitalOceanBot.Types.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DigitalOceanBot.Core.Commands.Droplet
{
    [BotCommand(BotCommandType.Droplets)]
    public sealed class GetDropletsCommand : IBotCommand
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly IDigitalOceanClient _digitalOceanClient;
        private readonly StorageService _storageService;
        private readonly DropletPaginatorService _dropletPaginatorService;

        public GetDropletsCommand(
            ITelegramBotClient telegramBotClient,
            IDigitalOceanClient digitalOceanClient,
            StorageService storageService,
            DropletPaginatorService dropletPaginatorService)
        {
            _telegramBotClient = telegramBotClient;
            _digitalOceanClient = digitalOceanClient;
            _storageService = storageService;
            _dropletPaginatorService = dropletPaginatorService;
        }

        public async Task ExecuteCommandAsync(Message message)
        {
            var droplets = await _digitalOceanClient.Droplets.GetAll();

            if (droplets.Count > 0)
            {
                _storageService.AddOrUpdate(StorageKeys.Droplets, droplets);
                var pageModel = _dropletPaginatorService.GetPage(0);

                await _telegramBotClient.SendTextMessageAsync(
                    chatId:message.Chat.Id, 
                    text:pageModel.MessageText, 
                    parseMode:ParseMode.Markdown, 
                    replyMarkup: pageModel.Keyboard);
            }
            else
            {
                await _telegramBotClient.SendTextMessageAsync(
                    chatId:message.Chat.Id, 
                    text:DropletMessage.GetDropletsNotFoundMessage());
            }
        }
    }
}
