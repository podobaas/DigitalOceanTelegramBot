using System.Threading.Tasks;
using DigitalOceanBot.Services;
using DigitalOceanBot.Services.Paginators;
using DigitalOceanBot.Types.Attributes;
using DigitalOceanBot.Types.Enums;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace DigitalOceanBot.Core.CallbackQueries.Size
{
    [BotCallbackQuery(BotCallbackQueryType.SizeSelect)]
    public sealed class SelectSizeCallbackQuery: IBotCallbackQuery
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly SizePaginatorService _sizePaginatorService;
        private readonly StorageService _storageService;

        public SelectSizeCallbackQuery(
            ITelegramBotClient telegramBotClient, 
            SizePaginatorService sizePaginatorService,
            StorageService storageService)
        {
            _telegramBotClient = telegramBotClient;
            _sizePaginatorService = sizePaginatorService;
            _storageService = storageService;
        }
        
        public async Task ExecuteCallbackQueryAsync(long chatId, int messageId, string callbackQueryId, string payload)
        {
            var paginator = _sizePaginatorService.Select(payload);
            _storageService.AddOrUpdate(StorageKeys.SizeId, payload);
            _storageService.Remove(StorageKeys.Sizes);

            await _telegramBotClient.DeleteMessageAsync(chatId, messageId);
            
            await _telegramBotClient.SendTextMessageAsync(
                chatId:chatId, 
                text:paginator.MessageText, 
                parseMode:ParseMode.Markdown);

            _sizePaginatorService?.OnSelectCallback();
        }
    }
}