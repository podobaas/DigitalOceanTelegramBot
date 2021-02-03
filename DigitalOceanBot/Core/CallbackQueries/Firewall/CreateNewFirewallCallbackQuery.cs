using System.Threading.Tasks;
using DigitalOceanBot.Core.Attributes;
using DigitalOceanBot.Messages;
using DigitalOceanBot.Services;
using DigitalOceanBot.Services.Paginators;
using DigitalOceanBot.Types.Enums;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace DigitalOceanBot.Core.CallbackQueries.Firewall
{
    [BotCallbackQuery(BotCallbackQueryType.FirewallCreateNew)]
    public sealed class CreateNewFirewallCallbackQuery: IBotCallbackQuery
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly FirewallPaginatorService _firewallPaginatorService;
        private readonly StorageService _storageService;
        
        public CreateNewFirewallCallbackQuery(
            ITelegramBotClient telegramBotClient, 
            FirewallPaginatorService firewallPaginatorService,
            StorageService storageService)
        {
            _telegramBotClient = telegramBotClient;
            _firewallPaginatorService = firewallPaginatorService;
            _storageService = storageService;
        }
        
        public async Task ExecuteCallbackQueryAsync(long chatId, int messageId, string callbackQueryId, string payload)
        {
            _storageService.AddOrUpdate(StorageKeys.BotCurrentState, BotStateType.FirewallCreateWaitingEnterName);

            await _telegramBotClient.DeleteMessageAsync(
                chatId: chatId, 
                messageId: messageId);
            
            await _telegramBotClient.SendTextMessageAsync(
                chatId:chatId, 
                text:FirewallMessage.GetEnterNameMessage());
        }
    }
}