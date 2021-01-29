using System.Threading.Tasks;
using DigitalOceanBot.Services.Paginators;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace DigitalOceanBot.Core.CallbackQueries.Droplet
{
    public class PreviousAndNextDropletCallbackQuery: ICallbackQuery
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly DropletPaginatorService _dropletPaginatorService;
        
        public PreviousAndNextDropletCallbackQuery(ITelegramBotClient telegramBotClient, DropletPaginatorService dropletPaginatorService)
        {
            _telegramBotClient = telegramBotClient;
            _dropletPaginatorService = dropletPaginatorService;
        }
        
        public async Task ExecuteCallbackQueryAsync(long chatId, int messageId, string callbackQueryId, string payload)
        {
            var pageCount = int.Parse(payload);
            var pageModel = _dropletPaginatorService.GetPage(pageCount);
            
            await _telegramBotClient.EditMessageTextAsync(
                chatId:chatId, 
                messageId:messageId, 
                text:pageModel.MessageText, 
                parseMode:ParseMode.Markdown, 
                replyMarkup:pageModel.Keyboard);
        }
    }
}