using System.Threading.Tasks;
using DigitalOceanBot.Messages;
using DigitalOceanBot.Services;
using DigitalOceanBot.Types.Attributes;
using DigitalOceanBot.Types.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DigitalOceanBot.Core.Commands.Firewall
{
    [BotCommand(BotCommandType.FirewallAddOutboundRule)]
    public sealed class AddOutboundRuleCommand : IBotCommand
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly StorageService _storageService;

        public AddOutboundRuleCommand(
            ITelegramBotClient telegramBotClient,
            StorageService storageService)
        {
            _telegramBotClient = telegramBotClient;
            _storageService = storageService;
        }
        
        public async Task ExecuteCommandAsync(Message message)
        {
            var firewallId = _storageService.Get<string>(StorageKeys.FirewallId);

            if (!string.IsNullOrEmpty(firewallId))
            {
                _storageService.AddOrUpdate(StorageKeys.BotCurrentState, BotStateType.FirewallUpdateWaitingEnterOutboundRule);
                
                await _telegramBotClient.SendTextMessageAsync(
                    chatId:message.Chat.Id, 
                    text:FirewallMessage.GetEnterBoundRuleMessage(), 
                    parseMode:ParseMode.Html);
            }
        }
    }
}
