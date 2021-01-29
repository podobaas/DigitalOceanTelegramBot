using System.Threading.Tasks;
using DigitalOceanBot.Messages;
using DigitalOceanBot.Services;
using DigitalOceanBot.Types.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DigitalOceanBot.Core.Commands.Droplet
{
    public class SnapshotCommand : ICommand
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly StorageService _storageService;

        public SnapshotCommand(ITelegramBotClient telegramBotClient, StorageService storageService)
        {
            _telegramBotClient = telegramBotClient;
            _storageService = storageService;
        }

        public async Task ExecuteCommandAsync(Message message)
        {
            var dropletId = _storageService.Get<long>(StorageKeys.SelectedDroplet);
            
            if (dropletId > 0)
            {
                _storageService.AddOrUpdate(StorageKeys.BotCurrentState, StateType.DropletWaitEnterSnapshotName);
                
                await _telegramBotClient.SendTextMessageAsync(
                    chatId:message.Chat.Id, 
                    text:DropletMessage.GetEnterSnapshotNameMessage(), 
                    parseMode:ParseMode.Markdown);
            }
        }
    }
}
