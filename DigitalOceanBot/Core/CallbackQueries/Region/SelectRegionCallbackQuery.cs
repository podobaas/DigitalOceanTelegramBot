using System.Threading.Tasks;
using DigitalOceanBot.Services;
using DigitalOceanBot.Services.Paginators;
using DigitalOceanBot.Types.Attributes;
using DigitalOceanBot.Types.Enums;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace DigitalOceanBot.Core.CallbackQueries.Region
{
    [BotCallbackQuery(BotCallbackQueryType.RegionSelect)]
    public sealed class SelectRegionCallbackQuery: IBotCallbackQuery
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly RegionPaginatorService _regionPaginatorService;
        private readonly StorageService _storageService;

        public SelectRegionCallbackQuery(
            ITelegramBotClient telegramBotClient, 
            RegionPaginatorService regionPaginatorService,
            StorageService storageService)
        {
            _telegramBotClient = telegramBotClient;
            _regionPaginatorService = regionPaginatorService;
            _storageService = storageService;
        }
        
        public async Task ExecuteCallbackQueryAsync(long chatId, int messageId, string callbackQueryId, string payload)
        {
            var paginator = _regionPaginatorService.Select(payload);
            _storageService.AddOrUpdate(StorageKeys.RegionId, payload);
            _storageService.Remove(StorageKeys.Regions);

            await _telegramBotClient.DeleteMessageAsync(chatId, messageId);
            
            await _telegramBotClient.SendTextMessageAsync(
                chatId:chatId, 
                text:paginator.MessageText, 
                parseMode:ParseMode.Markdown);

            _regionPaginatorService?.OnSelectCallback();
        }
    }
}