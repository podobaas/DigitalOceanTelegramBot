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
    public class PowerOnDropletCommand : DigitalOceanActionBase, IBotCommand
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly ILogger<DigitalOceanWorker> _logger;

        public PowerOnDropletCommand(
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
                    await PowerOnDroplet(message).ConfigureAwait(false);
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


        private async Task PowerOnDroplet(Message message)
        {
            await StartActionWithoutConfirm(message, "Power on", async (digitalOceanApi, dropletId) => await digitalOceanApi.DropletActions.PowerOn(dropletId));
        }
    }
}
