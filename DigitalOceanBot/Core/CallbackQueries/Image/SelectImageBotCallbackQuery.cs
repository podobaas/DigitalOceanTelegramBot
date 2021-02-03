using System.Threading.Tasks;
using DigitalOceanBot.Core.Attributes;
using DigitalOceanBot.Core.StateHandlers;
using DigitalOceanBot.Services;
using DigitalOceanBot.Services.Paginators;
using DigitalOceanBot.Types.Enums;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace DigitalOceanBot.Core.CallbackQueries.Image
{
    [BotCallbackQuery(BotCallbackQueryType.ImageSelect)]
    public sealed class SelectImageBotCallbackQuery: IBotCallbackQuery
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly ImagePaginatorService _imagePaginatorService;
        private readonly StorageService _storageService;

        public SelectImageBotCallbackQuery(
            ITelegramBotClient telegramBotClient, 
            ImagePaginatorService imagePaginatorService,
            StorageService storageService)
        {
            _telegramBotClient = telegramBotClient;
            _imagePaginatorService = imagePaginatorService;
            _storageService = storageService;
        }
        
        public async Task ExecuteCallbackQueryAsync(long chatId, int messageId, string callbackQueryId, string payload)
        {
            var paginator = _imagePaginatorService.Select(payload);
            _storageService.AddOrUpdate(StorageKeys.ImageId, long.Parse(payload));
            _storageService.Remove(StorageKeys.Images);

            await _telegramBotClient.DeleteMessageAsync(chatId, messageId);
            
            await _telegramBotClient.SendTextMessageAsync(
                chatId:chatId, 
                text:paginator.MessageText, 
                parseMode:ParseMode.Markdown);
        }
    }
}