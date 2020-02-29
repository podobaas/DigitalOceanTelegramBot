using System;
using System.Threading.Tasks;
using DigitalOcean.API.Exceptions;
using DigitalOcean.API.Models.Requests;
using DigitalOceanBot.Factory;
using DigitalOceanBot.Helpers;
using DigitalOceanBot.MongoDb;
using DigitalOceanBot.MongoDb.Models;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DigitalOceanBot.Commands.ProjectCommands
{
    public class ChangeEnvironmentProjectCommand : IBotCommand
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly IRepository<Session> _sessionRepo;
        private readonly ILogger<DigitalOceanWorker> _logger;
        private readonly IDigitalOceanClientFactory _digitalOceanClientFactory;

        public ChangeEnvironmentProjectCommand(
            ILogger<DigitalOceanWorker> logger,
            ITelegramBotClient telegramBotClient,
            IRepository<Session> sessionRepo,
            IDigitalOceanClientFactory digitalOceanClientFactory)
        {
            _logger = logger;
            _telegramBotClient = telegramBotClient;
            _sessionRepo = sessionRepo;
            _digitalOceanClientFactory = digitalOceanClientFactory;
        }
        
        public async Task Execute(Message message, SessionState sessionState)
        {
            try
            {
                if (sessionState == SessionState.SelectedProject)
                {
                    await InputEnvironmentForProject(message).ConfigureAwait(false);
                }
                else if (sessionState == SessionState.WaitInputNewEnvironmentForProject)
                {
                    await ChangeEnvironment(message).ConfigureAwait(false);
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

        private async Task InputEnvironmentForProject(Message message)
        {
            _sessionRepo.Update(message.From.Id, session =>
            {
                session.State = SessionState.WaitInputNewEnvironmentForProject;
            });

            await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Select new environment for project:", replyMarkup:Keyboards.GetEnvironmentKeyboard());
        }

        private async Task ChangeEnvironment(Message message)
        {
            var digitalOceanApi = _digitalOceanClientFactory.GetInstance(message.From.Id);
            var session = _sessionRepo.Get(message.From.Id);
            var projectId = session.Data.CastObject<string>();

            await digitalOceanApi.Projects.Patch(projectId, new PatchProject
            {
                Environment = message.Text
            });

            _sessionRepo.Update(message.From.Id, session => { session.State = SessionState.SelectedProject; });

            await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Done \U00002705", replyMarkup:Keyboards.GetSelectedProjectMenuKeyboard());
        }
    }
}
