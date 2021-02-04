using System.Linq;
using System.Threading.Tasks;
using DigitalOcean.API;
using DigitalOceanBot.Messages;
using DigitalOceanBot.Services;
using DigitalOceanBot.Types.Attributes;
using DigitalOceanBot.Types.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DigitalOceanBot.Core.Commands.Firewall
{
    [BotCommand(BotCommandType.FirewallRemoveDroplets)]
    public sealed class RemoveDropletsCommand : IBotCommand
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly IDigitalOceanClient _digitalOceanClient;
        private readonly StorageService _storageService;

        public RemoveDropletsCommand(
            ITelegramBotClient telegramBotClient,
            IDigitalOceanClient digitalOceanClient,
            StorageService storageService)
        {
            _telegramBotClient = telegramBotClient;
            _digitalOceanClient = digitalOceanClient;
            _storageService = storageService;
        }
        
        public async Task ExecuteCommandAsync(Message message)
        {
            var firewallId = _storageService.Get<string>(StorageKeys.FirewallId);

            if (!string.IsNullOrEmpty(firewallId))
            {
                _storageService.AddOrUpdate(StorageKeys.BotCurrentState, BotStateType.FirewallUpdateWaitingEnterRemoveDroplet);

                var firewall = await _digitalOceanClient.Firewalls.Get(firewallId);
                var droplets = await _digitalOceanClient.Droplets.GetAll();
                var associatedDroplets = droplets.Where(x => firewall.DropletIds.Contains(x.Id))
                    .OrderBy(x => x.CreatedAt)
                    .ToList();

                if (associatedDroplets is null or {Count: <= 0})
                {
                    await _telegramBotClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: FirewallMessage.GetNoAssociatedDropletsMessage());
                    
                    _storageService.AddOrUpdate(StorageKeys.BotCurrentState, BotStateType.None);

                    return;
                }
                
                _storageService.AddOrUpdate(StorageKeys.Droplets, associatedDroplets);

                await _telegramBotClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: FirewallMessage.GetDropletsListMessage(associatedDroplets),
                    parseMode: ParseMode.Markdown);
            }
        }
    }
}