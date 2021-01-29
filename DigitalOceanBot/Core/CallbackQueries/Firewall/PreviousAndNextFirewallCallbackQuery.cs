using System.Threading.Tasks;
using DigitalOceanBot.Services.Paginators;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace DigitalOceanBot.Core.CallbackQueries.Firewall
{
    public class PreviousAndNextFirewallCallbackQuery: ICallbackQuery
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly FirewallPaginatorService _firewallPaginatorService;
        
        public PreviousAndNextFirewallCallbackQuery(ITelegramBotClient telegramBotClient, FirewallPaginatorService firewallPaginatorService)
        {
            _telegramBotClient = telegramBotClient;
            _firewallPaginatorService = firewallPaginatorService;
        }
        
        public async Task ExecuteCallbackQueryAsync(long chatId, int messageId, string callbackQueryId, string payload)
        {
            var pageCount = int.Parse(payload);
            var pageModel = _firewallPaginatorService.GetPage(pageCount);
            
            await _telegramBotClient.EditMessageTextAsync(
                chatId:chatId, 
                messageId:messageId, 
                text:pageModel.MessageText, 
                parseMode:ParseMode.Markdown, 
                replyMarkup:pageModel.Keyboard);
        }
    }
}