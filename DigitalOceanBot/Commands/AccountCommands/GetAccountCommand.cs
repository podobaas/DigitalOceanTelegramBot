using System;
using System.Text;
using System.Threading.Tasks;
using DigitalOcean.API.Exceptions;
using DigitalOceanBot.Factory;
using DigitalOceanBot.MongoDb.Models;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DigitalOceanBot.Commands.AccountCommands
{
    public class GetAccountCommand : IBotCommand
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly ILogger<DigitalOceanWorker> _logger;
        private readonly IDigitalOceanClientFactory _digitalOceanClientFactory;

        public GetAccountCommand(
            ILogger<DigitalOceanWorker> logger,
            ITelegramBotClient telegramBotClient,
            IDigitalOceanClientFactory digitalOceanClientFactory)
        {
            _logger = logger;
            _telegramBotClient = telegramBotClient;
            _digitalOceanClientFactory = digitalOceanClientFactory;
        }

        public async Task Execute(Message message, SessionState sessionState)
        {
            try
            {
                if (sessionState == SessionState.MainMenu)
                {
                    await GetAccount(message).ConfigureAwait(false);
                }
            }
            catch (ApiException ex)
            {
                _logger.LogError($"UserId={message.From.Id.ToString()}, Error={ex.Message}");
                await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, $"DigitalOcean API Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"UserId={message.From.Id.ToString()}, Error={ex.Message}, StackTrace={ex.StackTrace}");
                await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Sorry, аn error has occurred \U0001F628");
            }
        }

        private async Task GetAccount(Message message)
        {
            var stringBuilder = new StringBuilder(string.Empty);
            var digitalOceanApi = _digitalOceanClientFactory.GetInstance(message.From.Id);
            var account = await digitalOceanApi.Account.Get();
            var balance = await digitalOceanApi.BalanceClient.Get();

            stringBuilder.Append($"\U0001F4C4 Account info: \n");
            stringBuilder.Append($"Email: *{account.Email}* \n");
            var emailIconState = account.EmailVerified ? "\U00002705" : "\U0000274C";
            stringBuilder.Append($"Email verified: *{emailIconState}* \n");
            stringBuilder.Append($"Account status: *{account.Status}* \n");
            stringBuilder.Append($"Droplet limit: *{account.DropletLimit.ToString()}* \n");
            stringBuilder.Append($"Floating IP limit: *{account.FloatingIpLimit.ToString()}* \n\n");

            stringBuilder.Append($"\U0001F4B0 Balance info: \n");
            stringBuilder.Append($"Balance as of the generated at time: *{balance.MonthToDateBalance}* \n");
            stringBuilder.Append($"Billing activity balance: *{balance.AccountBalance}* \n");
            stringBuilder.Append($"Amount used in the current billing period: *{balance.MonthToDateUsage}* \n");

            await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, stringBuilder.ToString(), ParseMode.Markdown);
        }
    }
}
