using System.Threading.Tasks;
using DigitalOcean.API;
using DigitalOceanBot.Messages;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DigitalOceanBot.Core.Commands.Account
{
    public class GetAccountCommand : ICommand
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly IDigitalOceanClient _digitalOceanClient;

        public GetAccountCommand(ITelegramBotClient telegramBotClient, IDigitalOceanClient digitalOceanClient)
        {
            _telegramBotClient = telegramBotClient;
            _digitalOceanClient = digitalOceanClient;
        }

        public async Task ExecuteCommandAsync(Message message)
        {
            var account = await _digitalOceanClient.Account.Get();
            var balance = await _digitalOceanClient.BalanceClient.Get();

            await _telegramBotClient.SendTextMessageAsync(
                chatId:message.Chat.Id, 
                text:AccountMessage.GetAccountInfoMessage(account, balance), 
                parseMode:ParseMode.Markdown);
        }
    }
}
