using System.Threading.Tasks;
using DigitalOceanBot.Services;
using DigitalOceanBot.Services.Paginators;
using DigitalOceanBot.Types.Attributes;
using DigitalOceanBot.Types.Enums;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace DigitalOceanBot.Core.CallbackQueries.Project
{
    [BotCallbackQuery(BotCallbackQueryType.ProjectSelect)]
    public sealed class SelectProjectCallbackQuery: IBotCallbackQuery
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly ProjectPaginatorService _projectPaginatorService;
        private readonly StorageService _storageService;
        
        public SelectProjectCallbackQuery(
            ITelegramBotClient telegramBotClient, 
            ProjectPaginatorService projectPaginatorService,
            StorageService storageService)
        {
            _telegramBotClient = telegramBotClient;
            _projectPaginatorService = projectPaginatorService;
            _storageService = storageService;
        }
        
        public async Task ExecuteCallbackQueryAsync(long chatId, int messageId, string callbackQueryId, string payload)
        {
            var paginator = _projectPaginatorService.Select(payload);
            
            _storageService.AddOrUpdate(StorageKeys.ProjectId, payload);
            _storageService.Remove(StorageKeys.Projects);
            
            await _telegramBotClient.DeleteMessageAsync(chatId, messageId);
            
            await _telegramBotClient.SendTextMessageAsync(
                chatId:chatId, 
                text:paginator.MessageText, 
                parseMode:ParseMode.Markdown, 
                replyMarkup: paginator.Keyboard);
        }
    }
}