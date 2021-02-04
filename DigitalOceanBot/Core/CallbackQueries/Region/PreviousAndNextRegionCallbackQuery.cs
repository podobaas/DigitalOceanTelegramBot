using System.Threading.Tasks;
using DigitalOceanBot.Services.Paginators;
using DigitalOceanBot.Types.Attributes;
using DigitalOceanBot.Types.Enums;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace DigitalOceanBot.Core.CallbackQueries.Region
{
    [BotCallbackQuery(BotCallbackQueryType.RegionNext)]
    [BotCallbackQuery(BotCallbackQueryType.RegionPrevious)]
    public sealed class PreviousAndNextRegionCallbackQuery: IBotCallbackQuery
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly RegionPaginatorService _regionPaginatorService;
        
        public PreviousAndNextRegionCallbackQuery(
            ITelegramBotClient telegramBotClient, 
            RegionPaginatorService regionPaginatorService)
        {
            _telegramBotClient = telegramBotClient;
            _regionPaginatorService = regionPaginatorService;
        }
        
        public async Task ExecuteCallbackQueryAsync(long chatId, int messageId, string callbackQueryId, string payload)
        {
            var pageCount = int.Parse(payload);
            var pageModel = _regionPaginatorService.GetPage(pageCount);
            
            await _telegramBotClient.EditMessageTextAsync(
                chatId:chatId, 
                messageId:messageId, 
                text:pageModel.MessageText, 
                parseMode:ParseMode.Markdown, 
                replyMarkup:pageModel.Keyboard);
        }
    }
}