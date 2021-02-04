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
    [BotCommand(BotCommandType.FirewallAddDroplets)]
    public sealed class AddDropletsCommand : IBotCommand
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly IDigitalOceanClient _digitalOceanClient;
        private readonly StorageService _storageService;

        public AddDropletsCommand(
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
                _storageService.AddOrUpdate(StorageKeys.BotCurrentState, BotStateType.FirewallUpdateWaitingEnterAddDroplet);

                var droplets = await _digitalOceanClient.Droplets.GetAll();
                
                if (droplets is not null and {Count: > 0})
                {
                    _storageService.AddOrUpdate(StorageKeys.Droplets, droplets);
                    
                    await _telegramBotClient.SendTextMessageAsync(
                        chatId:message.Chat.Id, 
                        text:FirewallMessage.GetDropletsListMessage(droplets.OrderBy(x => x.CreatedAt)), 
                        parseMode:ParseMode.Markdown);
                }
                else
                {
                    _storageService.AddOrUpdate(StorageKeys.BotCurrentState, BotStateType.None);
                    
                    await _telegramBotClient.SendTextMessageAsync(
                        chatId:message.Chat.Id, 
                        text:DropletMessage.GetDropletsNotFoundMessage(), 
                        parseMode:ParseMode.Markdown);
                }
            }
        }
    }
}