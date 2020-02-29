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
    public class CreateSnapshotDropletCommand : DigitalOceanActionBase, IBotCommand
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly IRepository<Session> _sessionRepo;
        private readonly ILogger<DigitalOceanWorker> _logger;

        public CreateSnapshotDropletCommand(
            ILogger<DigitalOceanWorker> logger,
            ITelegramBotClient telegramBotClient,
            IRepository<Session> sessionRepo,
            IDigitalOceanClientFactory digitalOceanClientFactory) : base(logger, telegramBotClient, sessionRepo, digitalOceanClientFactory)
        {
            _telegramBotClient = telegramBotClient;
            _sessionRepo = sessionRepo;
            _logger = logger;
        }

        public async Task Execute(Message message, SessionState sessionState)
        {
            try
            {
                if (sessionState == SessionState.SelectedDroplet)
                {
                    await InputNameSnapshotDroplet(message).ConfigureAwait(false);
                }
                else if (sessionState == SessionState.WaitInputSnapshotName)
                {
                    await CreateSnapshotDroplet(message).ConfigureAwait(false);
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
        
        private async Task InputNameSnapshotDroplet(Message message)
        {
            _sessionRepo.Update(message.From.Id, session =>
            {
                session.State = SessionState.WaitInputSnapshotName;
            });

            await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Input snapshot name:");
        }
        
        private async Task CreateSnapshotDroplet(Message message)
        {
            await StartActionWithoutConfirm(message, "Create snapshot", async (digitalOceanApi, dropletId) => await digitalOceanApi.DropletActions.Snapshot(dropletId, message.Text));
        }
    }
}
