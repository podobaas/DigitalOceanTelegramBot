using System.Threading.Tasks;
using DigitalOcean.API;
using DigitalOcean.API.Models.Requests;
using DigitalOceanBot.Messages;
using DigitalOceanBot.Services;
using DigitalOceanBot.Types.Attributes;
using DigitalOceanBot.Types.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DigitalOceanBot.Core.StateHandlers.Project
{
    [BotStateHandler(BotStateType.ProjectUpdateWaitingEnterNewDescription)]
    public sealed class ChangeDescriptionStateHandler : IBotStateHandler
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly IDigitalOceanClient _digitalOceanClient;
        private readonly StorageService _storageService;

        public ChangeDescriptionStateHandler(
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
            var projectId = _storageService.Get<string>(StorageKeys.ProjectId);

            if (!string.IsNullOrEmpty(projectId))
            {
                await _digitalOceanClient.Projects.Patch(projectId, new PatchProject()
                {
                    Description = message.Text
                });

                await _telegramBotClient.SendTextMessageAsync(
                    chatId:message.Chat.Id, 
                    text:CommonMessage.GetDoneMessage());
             
                _storageService.AddOrUpdate(StorageKeys.BotCurrentState, BotStateType.None);
            }
        }
    }
}