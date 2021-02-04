using System.Threading.Tasks;
using DigitalOceanBot.Services.Paginators;
using DigitalOceanBot.Types.Attributes;
using DigitalOceanBot.Types.Enums;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace DigitalOceanBot.Core.CallbackQueries.Size
{
    [BotCallbackQuery(BotCallbackQueryType.SizeNext)]
    [BotCallbackQuery(BotCallbackQueryType.SizePrevious)]
    public sealed class PreviousAndNextSizeCallbackQuery: IBotCallbackQuery
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly SizePaginatorService _sizePaginatorService;
        
        public PreviousAndNextSizeCallbackQuery(
            ITelegramBotClient telegramBotClient, 
            SizePaginatorService sizePaginatorService)
        {
            _telegramBotClient = telegramBotClient;
            _sizePaginatorService = sizePaginatorService;
        }
        
        public async Task ExecuteCallbackQueryAsync(long chatId, int messageId, string callbackQueryId, string payload)
        {
            var pageCount = int.Parse(payload);
            var pageModel = _sizePaginatorService.GetPage(pageCount);
            
            await _telegramBotClient.EditMessageTextAsync(
                chatId:chatId, 
                messageId:messageId, 
                text:pageModel.MessageText, 
                parseMode:ParseMode.Markdown, 
                replyMarkup:pageModel.Keyboard);
        }
    }
}