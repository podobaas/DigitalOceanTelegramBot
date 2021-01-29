using System.Threading.Tasks;
using DigitalOceanBot.Services;
using DigitalOceanBot.Services.Paginators;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace DigitalOceanBot.Core.CallbackQueries.Droplet
{
    public class SelectDropletCallbackQuery: ICallbackQuery
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly DropletPaginatorService _dropletPaginatorService;
        private readonly StorageService _storageService;
        
        public SelectDropletCallbackQuery(
            ITelegramBotClient telegramBotClient, 
            DropletPaginatorService dropletPaginatorService,
            StorageService storageService)
        {
            _telegramBotClient = telegramBotClient;
            _dropletPaginatorService = dropletPaginatorService;
            _storageService = storageService;
        }
        
        public async Task ExecuteCallbackQueryAsync(long chatId, int messageId, string callbackQueryId, string payload)
        {
            var paginator = _dropletPaginatorService.Select(payload);
            
            _storageService.AddOrUpdate(StorageKeys.SelectedDroplet, long.Parse(payload));
            _storageService.Remove(StorageKeys.MyDroplets);
            
            await _telegramBotClient.DeleteMessageAsync(chatId, messageId);
            
            await _telegramBotClient.SendTextMessageAsync(
                chatId:chatId, 
                text:paginator.MessageText, 
                parseMode:ParseMode.Markdown, 
                replyMarkup: paginator.Keyboard);
        }
    }
}