using System;
using System.Threading.Tasks;
using DigitalOcean.API.Exceptions;
using DigitalOceanBot.Factory;
using DigitalOceanBot.MongoDb;
using DigitalOceanBot.MongoDb.Models;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DigitalOceanBot.Commands.DropletCommands
{
    public class RebootDropletCommand : DigitalOceanActionBase, IBotCommand
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly ILogger<DigitalOceanWorker> _logger;

        public RebootDropletCommand(
            ILogger<DigitalOceanWorker> logger,
            ITelegramBotClient telegramBotClient,
            IRepository<Session> sessionRepo,
            IDigitalOceanClientFactory digitalOceanClientFactory) : base(logger, telegramBotClient, sessionRepo, digitalOceanClientFactory)
        {
            _telegramBotClient = telegramBotClient;
            _logger = logger;
        }

        public async Task Execute(Message message, SessionState sessionState)
        {
            try
            {
                if (sessionState == SessionState.SelectedDroplet)
                {
                    await ConfirmMessage(message, SessionState.WaitConfirmReboot).ConfigureAwait(false);
                }
                else if (sessionState == SessionState.WaitConfirmReboot)
                {
                    await RebootDroplet(message).ConfigureAwait(false);
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

        private async Task RebootDroplet(Message message)
        {
            await StartActionWithConfirm(message, "Reboot", async (digitalOceanClient, dropletId) => await digitalOceanClient.DropletActions.Reboot(dropletId));
        }
    }
}
