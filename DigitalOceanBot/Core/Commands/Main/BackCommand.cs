using System.Threading.Tasks;
using DigitalOceanBot.Core.Attributes;
using DigitalOceanBot.Keyboards;
using DigitalOceanBot.Messages;
using DigitalOceanBot.Services;
using DigitalOceanBot.Types.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DigitalOceanBot.Core.Commands.Main
{
    [BotCommand(BotCommandType.Back)]
    public sealed class BackCommand : IBotCommand
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly StorageService _storageService;

        public BackCommand(ITelegramBotClient telegramBotClient, StorageService storageService)
        {
            _telegramBotClient = telegramBotClient;
            _storageService = storageService;
        }

        public async Task ExecuteCommandAsync(Message message)
        {
            _storageService.AddOrUpdate(StorageKeys.BotCurrentState, BotStateType.None);
            
            await _telegramBotClient.SendTextMessageAsync(
                chatId:message.Chat.Id, 
                text:CommonMessage.GetMainMenuMessage(), 
                replyMarkup: CommonKeyboard.GetMainMenuKeyboard());
        }
    }
}