using System.Threading.Tasks;
using DigitalOceanBot.Core.Attributes;
using DigitalOceanBot.Services.Paginators;
using DigitalOceanBot.Types.Enums;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace DigitalOceanBot.Core.CallbackQueries.Project
{
    [BotCallbackQuery(BotCallbackQueryType.ProjectNext)]
    [BotCallbackQuery(BotCallbackQueryType.ProjectPrevious)]
    public sealed class PreviousAndNextProjectCallbackQuery: IBotCallbackQuery
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly ProjectPaginatorService _projectPaginatorService;
        
        public PreviousAndNextProjectCallbackQuery(
            ITelegramBotClient telegramBotClient, 
            ProjectPaginatorService projectPaginatorService)
        {
            _telegramBotClient = telegramBotClient;
            _projectPaginatorService = projectPaginatorService;
        }
        
        public async Task ExecuteCallbackQueryAsync(long chatId, int messageId, string callbackQueryId, string payload)
        {
            var pageCount = int.Parse(payload);
            var pageModel = _projectPaginatorService.GetPage(pageCount);
            
            await _telegramBotClient.EditMessageTextAsync(
                chatId:chatId, 
                messageId:messageId, 
                text:pageModel.MessageText, 
                parseMode:ParseMode.Markdown, 
                replyMarkup:pageModel.Keyboard);
        }
    }
}