using System;
using System.Threading.Tasks;
using DigitalOcean.API;
using DigitalOceanBot.Core.Attributes;
using DigitalOceanBot.Keyboards;
using DigitalOceanBot.Messages;
using DigitalOceanBot.Services;
using DigitalOceanBot.Types.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;
using ProjectRequest = DigitalOcean.API.Models.Requests.Project;

namespace DigitalOceanBot.Core.StateHandlers.Project
{
    [BotStateHandler(BotStateType.ProjectCreateWaitingEnterName)]
    [BotStateHandler(BotStateType.ProjectCreateWaitingEnterPurpose)]
    [BotStateHandler(BotStateType.ProjectCreateWaitingEnterDescription)]
    public sealed class CreateNewProjectStateHandler: IBotStateHandler
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly IDigitalOceanClient _digitalOceanClient;
        private readonly StorageService _storageService;

        public CreateNewProjectStateHandler(
            ITelegramBotClient telegramBotClient,
            IDigitalOceanClient digitalOceanClient,
            StorageService storageService)
        {
            _telegramBotClient = telegramBotClient;
            _digitalOceanClient = digitalOceanClient;
            _storageService = storageService;
        }
        
        public async Task ExecuteHandlerAsync(Message message)
        {
            var state = _storageService.Get<BotStateType>(StorageKeys.BotCurrentState);

            switch (state)
            {
                case BotStateType.ProjectCreateWaitingEnterName:
                    await EnterName(message);
                    break;
                case BotStateType.ProjectCreateWaitingEnterDescription:
                    await EnterDescription(message);
                    break;
                case BotStateType.ProjectCreateWaitingEnterPurpose:
                    await EnterPurpose(message);
                    _storageService.Remove(StorageKeys.Projects);
                    _storageService.Remove(StorageKeys.NewProject);
                    _storageService.AddOrUpdate(StorageKeys.BotCurrentState, BotStateType.None);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state));
            }
        }

        private async Task EnterName(Message message)
        {
            var newProject = new ProjectRequest
            {
                Name = message.Text
            };

            _storageService.AddOrUpdate(StorageKeys.NewProject, newProject);
            _storageService.AddOrUpdate(StorageKeys.BotCurrentState, BotStateType.ProjectCreateWaitingEnterDescription);
            
            await _telegramBotClient.SendTextMessageAsync(
                chatId:message.Chat.Id, 
                text:ProjectMessage.GetEnterDescriptionMessage());
        }

        private async Task EnterDescription(Message message)
        {
            var project = _storageService.Get<ProjectRequest>(StorageKeys.NewProject);
            project.Description = message.Text;
            
            _storageService.AddOrUpdate(StorageKeys.BotCurrentState, BotStateType.ProjectCreateWaitingEnterPurpose);
            
            
            await _telegramBotClient.SendTextMessageAsync(
                chatId:message.Chat.Id, 
                text:ProjectMessage.GetChoosePurposeMessage(),
                replyMarkup:ProjectKeyboard.GetProjectPurposeKeyboard());
        }

        private async Task EnterPurpose(Message message)
        {
            var project = _storageService.Get<ProjectRequest>(StorageKeys.NewProject);
            project.Purpose = message.Text;

            await _digitalOceanClient.Projects.Create(project);
            
            await _telegramBotClient.SendTextMessageAsync(
                chatId:message.Chat.Id, 
                text:CommonMessage.GetDoneMessage(),
                replyMarkup:CommonKeyboard.GetMainMenuKeyboard());

        }
    }
}