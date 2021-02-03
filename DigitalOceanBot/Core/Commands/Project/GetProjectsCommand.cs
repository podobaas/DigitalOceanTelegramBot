using System.Threading.Tasks;
using DigitalOcean.API;
using DigitalOceanBot.Core.Attributes;
using DigitalOceanBot.Keyboards;
using DigitalOceanBot.Messages;
using DigitalOceanBot.Services;
using DigitalOceanBot.Services.Paginators;
using DigitalOceanBot.Types.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DigitalOceanBot.Core.Commands.Droplet
{
    [BotCommand(BotCommandType.Projects)]
    public sealed class GetProjectsCommand : IBotCommand
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly IDigitalOceanClient _digitalOceanClient;
        private readonly StorageService _storageService;
        private readonly ProjectPaginatorService _projectPaginatorService;

        public GetProjectsCommand(
            ITelegramBotClient telegramBotClient,
            IDigitalOceanClient digitalOceanClient,
            StorageService storageService,
            ProjectPaginatorService projectPaginatorService)
        {
            _telegramBotClient = telegramBotClient;
            _digitalOceanClient = digitalOceanClient;
            _storageService = storageService;
            _projectPaginatorService = projectPaginatorService;
        }

        public async Task ExecuteCommandAsync(Message message)
        {
            var projects = await _digitalOceanClient.Projects.GetAll();

            if (projects.Count > 0)
            {
                _storageService.AddOrUpdate(StorageKeys.Projects, projects);
                var pageModel = _projectPaginatorService.GetPage(0);

                await _telegramBotClient.SendTextMessageAsync(
                    chatId:message.Chat.Id, 
                    text:pageModel.MessageText, 
                    parseMode:ParseMode.Markdown, 
                    replyMarkup: pageModel.Keyboard);
            }
            else
            {
                await _telegramBotClient.SendTextMessageAsync(
                    chatId:message.Chat.Id, 
                    text:ProjectMessage.GetProjectsNotFoundMessage());
            }
        }
    }
}
