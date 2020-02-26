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
    public class RenameProjectCommand : IBotCommand
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly IRepository<Session> _sessionRepo;
        private readonly ILogger<DigitalOceanWorker> _logger;
        private readonly IDigitalOceanClientFactory _digitalOceanClientFactory;

        public RenameProjectCommand(
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
                await _telegramBotClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                switch (sessionState)
                {
                    case SessionState.SelectedProject:
                        await InputNameForProject(message).ConfigureAwait(false);
                        break;
                    case SessionState.WaitInputNewNameForProject:
                        await RenameProject(message).ConfigureAwait(false);
                        break;
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

        private async Task InputNameForProject(Message message)
        {
            _sessionRepo.Update(message.From.Id, session =>
            {
                session.State = SessionState.WaitInputNewNameForProject;
            });

            await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Input new name for project:");
        }
        
        private async Task RenameProject(Message message)
        {
            var digitalOceanApi = _digitalOceanClientFactory.GetInstance(message.From.Id);
            var session = _sessionRepo.Get(message.From.Id);
            var projectId = session.Data.CastObject<string>();

            await digitalOceanApi.Projects.Patch(projectId, new PatchProject
            {
                Name = message.Text
            });
            
            _sessionRepo.Update(message.From.Id, session =>
            {
                session.State = SessionState.SelectedProject;
            });
            
            await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Done \U00002705");
        }
        
    }
}
