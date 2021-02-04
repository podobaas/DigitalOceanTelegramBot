using System.Threading.Tasks;
using DigitalOcean.API;
using DigitalOceanBot.Keyboards;
using DigitalOceanBot.Messages;
using DigitalOceanBot.Services;
using DigitalOceanBot.Services.Paginators;
using DigitalOceanBot.Types.Attributes;
using DigitalOceanBot.Types.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DigitalOceanBot.Core.Commands.Firewall
{
    [BotCommand(BotCommandType.Firewalls)]
    public sealed class GetFirewallsCommand : IBotCommand
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly IDigitalOceanClient _digitalOceanClient;
        private readonly StorageService _storageService;
        private readonly FirewallPaginatorService _firewallPaginatorService;

        public GetFirewallsCommand(
            ITelegramBotClient telegramBotClient,
            IDigitalOceanClient digitalOceanClient,
            StorageService storageService,
            FirewallPaginatorService firewallPaginatorService)
        {
            _telegramBotClient = telegramBotClient;
            _digitalOceanClient = digitalOceanClient;
            _storageService = storageService;
            _firewallPaginatorService = firewallPaginatorService;
        }

        public async Task ExecuteCommandAsync(Message message)
        {
            var firewalls = await _digitalOceanClient.Firewalls.GetAll();

            if (firewalls is not null and {Count: > 0})
            {
                _storageService.AddOrUpdate(StorageKeys.Firewalls, firewalls);
                var droplets = await _digitalOceanClient.Droplets.GetAll();
                _storageService.AddOrUpdate(StorageKeys.Droplets, droplets);
                var pageModel = _firewallPaginatorService.GetPage(0);
                
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
                    text:FirewallMessage.GetFirewallsNotFoundMessage());
            }
        }
    }
}
