using System.Threading.Tasks;
using DigitalOceanBot.Core.Attributes;
using DigitalOceanBot.Services.Paginators;
using DigitalOceanBot.Types.Enums;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace DigitalOceanBot.Core.CallbackQueries.Image
{
    [BotCallbackQuery(BotCallbackQueryType.ImageNext)]
    [BotCallbackQuery(BotCallbackQueryType.ImagePrevious)]
    public sealed class PreviousAndNextImageBotCallbackQuery: IBotCallbackQuery
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly ImagePaginatorService _imagePaginatorService;
        
        public PreviousAndNextImageBotCallbackQuery(
            ITelegramBotClient telegramBotClient, 
            ImagePaginatorService imagePaginatorService)
        {
            _telegramBotClient = telegramBotClient;
            _imagePaginatorService = imagePaginatorService;
        }
        
        public async Task ExecuteCallbackQueryAsync(long chatId, int messageId, string callbackQueryId, string payload)
        {
            var pageCount = int.Parse(payload);
            var pageModel = _imagePaginatorService.GetPage(pageCount);
            
            await _telegramBotClient.EditMessageTextAsync(
                chatId:chatId, 
                messageId:messageId, 
                text:pageModel.MessageText, 
                parseMode:ParseMode.Markdown, 
                replyMarkup:pageModel.Keyboard);
        }
    }
}