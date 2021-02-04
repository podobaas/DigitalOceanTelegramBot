using System.Threading.Tasks;
using DigitalOceanBot.Messages;
using DigitalOceanBot.Services;
using DigitalOceanBot.Types.Attributes;
using DigitalOceanBot.Types.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DigitalOceanBot.Core.Commands.Project
{
    [BotCommand(BotCommandType.ChangeDescriptionProject)]
    public sealed class ChangeDescriptionCommand : IBotCommand
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly StorageService _storageService;

        public ChangeDescriptionCommand(
            ITelegramBotClient telegramBotClient,
            StorageService storageService)
        {
            _telegramBotClient = telegramBotClient;
            _storageService = storageService;
        }

        public async Task ExecuteCommandAsync(Message message)
        {
            var projectId = _storageService.Get<string>(StorageKeys.ProjectId);

            if (!string.IsNullOrEmpty(projectId))
            {
                _storageService.AddOrUpdate(StorageKeys.BotCurrentState, BotStateType.ProjectUpdateWaitingEnterNewDescription);
                
                await _telegramBotClient.SendTextMessageAsync(
                    chatId:message.Chat.Id, 
                    text:ProjectMessage.GetEnterDescriptionMessage());
            }
        }
    }
}
