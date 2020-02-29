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

namespace DigitalOceanBot.Commands.ProjectCommands
{
    public class CreateNewProjectCommand : IBotCommand
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly IRepository<Session> _sessionRepo;
        private readonly ILogger<DigitalOceanWorker> _logger;
        private readonly IDigitalOceanClientFactory _digitalOceanClientFactory;

        public CreateNewProjectCommand(
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
                switch (sessionState)
                {
                    case SessionState.ProjectsMenu:
                        await InputNameForProject(message).ConfigureAwait(false);
                        break;
                    case SessionState.WaitInputNameForNewProject:
                        await InputPurposeForProject(message).ConfigureAwait(false);
                        break;
                    case SessionState.WaitInputPurposeForNewProject:
                        await InputDescriptionForProject(message).ConfigureAwait(false);
                        break;
                    case SessionState.WaitInputDescriptionForNewProject:
                        await CreateProject(message).ConfigureAwait(false);
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
                session.State = SessionState.WaitInputNameForNewProject;
            });

            await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Input name for new project:");
        }
        
        private async Task InputPurposeForProject(Message message)
        {
            _sessionRepo.Update(message.From.Id, session =>
            {
                session.Data = new Project
                {
                    Name = message.Text
                };

                session.State = SessionState.WaitInputPurposeForNewProject;
            });

            await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Select purpose for new project:", replyMarkup: Keyboards.GetPurposeKeyboard());
        }
        
        private async Task InputDescriptionForProject(Message message)
        {
            _sessionRepo.Update(message.From.Id, session =>
            {
                var project = session.Data.CastObject<Project>();
                project.Purpose = message.Text;
                session.Data = project;

                session.State = SessionState.WaitInputDescriptionForNewProject;
            });

            await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Input description for new project:", replyMarkup: Keyboards.GetProjectsMenuKeyboard());
        }
        
        private async Task CreateProject(Message message)
        {
            var session = _sessionRepo.Get(message.From.Id);
            var project = session.Data.CastObject<Project>();
            project.Description = message.Text;
            var digitalOceanApi = _digitalOceanClientFactory.GetInstance(message.From.Id);
            
            await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, $"\U0001F4C0 Create project...");
            
            var response = await digitalOceanApi.Projects.Create(project);

            _sessionRepo.Update(message.From.Id, session =>
            {
                session.Data = response.Id;
                session.State = SessionState.SelectedProject;
            });
            
            await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Done \U00002705", replyMarkup: Keyboards.GetSelectedProjectMenuKeyboard());
        }
    }
}
