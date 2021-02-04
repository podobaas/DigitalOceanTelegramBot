using System.Threading.Tasks;
using DigitalOceanBot.Keyboards;
using DigitalOceanBot.Messages;
using DigitalOceanBot.Services;
using DigitalOceanBot.Types.Attributes;
using DigitalOceanBot.Types.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DigitalOceanBot.Core.Commands.Project
{
    [BotCommand(BotCommandType.ChangePurposeProject)]
    public sealed class ChangePurposeCommand : IBotCommand
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly StorageService _storageService;

        public ChangePurposeCommand(
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
                _storageService.AddOrUpdate(StorageKeys.BotCurrentState, BotStateType.ProjectUpdateWaitingEnterNewPurpose);
                
                await _telegramBotClient.SendTextMessageAsync(
                    chatId:message.Chat.Id, 
                    text:ProjectMessage.GetChoosePurposeMessage(),
                    replyMarkup:ProjectKeyboard.GetProjectPurposeKeyboard());
            }
        }
    }
}
