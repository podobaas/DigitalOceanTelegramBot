using System.Threading.Tasks;
using DigitalOceanBot.Services;
using DigitalOceanBot.Services.Paginators;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace DigitalOceanBot.Core.CallbackQueries.Firewall
{
    public class SelectFirewallCallbackQuery: ICallbackQuery
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly FirewallPaginatorService _firewallPaginatorService;
        private readonly StorageService _storageService;
        
        public SelectFirewallCallbackQuery(
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
            var paginator = _firewallPaginatorService.Select(payload);
            
            _storageService.AddOrUpdate(StorageKeys.SelectedFirewall, payload);
            _storageService.Remove(StorageKeys.MyFirewalls);
            
            await _telegramBotClient.DeleteMessageAsync(chatId, messageId);
            
            await _telegramBotClient.SendTextMessageAsync(
                chatId:chatId, 
                text:paginator.MessageText, 
                parseMode:ParseMode.Markdown, 
                replyMarkup: paginator.Keyboard);
        }
    }
}