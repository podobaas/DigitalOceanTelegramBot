using System.Threading.Tasks;
using DigitalOcean.API;
using DigitalOcean.API.Models.Requests;
using DigitalOceanBot.Messages;
using DigitalOceanBot.Services;
using DigitalOceanBot.Types.Attributes;
using DigitalOceanBot.Types.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DigitalOceanBot.Core.Commands.Project
{
    [BotCommand(BotCommandType.RenameProject)]
    public sealed class RenameCommand : IBotCommand
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly IDigitalOceanClient _digitalOceanClient;
        private readonly StorageService _storageService;

        public RenameCommand(
            ITelegramBotClient telegramBotClient,
            IDigitalOceanClient digitalOceanClient,
            StorageService storageService)
        {
            _telegramBotClient = telegramBotClient;
            _digitalOceanClient = digitalOceanClient;
            _storageService = storageService;
        }

        public async Task ExecuteCommandAsync(Message message)
        {
            var projectId = _storageService.Get<string>(StorageKeys.ProjectId);

            if (!string.IsNullOrEmpty(projectId))
            {
                _storageService.AddOrUpdate(StorageKeys.BotCurrentState, BotStateType.ProjectUpdateWaitingEnterNewName);
                
                await _telegramBotClient.SendTextMessageAsync(
                    chatId:message.Chat.Id, 
                    text:ProjectMessage.GetEnterNameMessage());
            }
        }
    }
}
