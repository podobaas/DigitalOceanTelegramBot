using System.Threading.Tasks;
using DigitalOceanBot.Keyboards;
using DigitalOceanBot.Messages;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DigitalOceanBot.Core.Commands.Main
{
    public class StartCommand : ICommand
    {
        private readonly ITelegramBotClient _telegramBotClient;

        public StartCommand(ITelegramBotClient telegramBotClient)
        {
            _telegramBotClient = telegramBotClient;
        }

        public async Task ExecuteCommandAsync(Message message)
        {
            await _telegramBotClient.SendTextMessageAsync(
                chatId:message.Chat.Id, 
                text:CommonMessage.GetMainMenuMessage(), 
                replyMarkup: CommonKeyboard.GetMainMenuKeyboard());
        }
    }
}