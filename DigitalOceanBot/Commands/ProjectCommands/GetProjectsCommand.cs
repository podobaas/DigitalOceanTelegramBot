using System;
using System.Threading.Tasks;
using DigitalOcean.API.Exceptions;
using DigitalOceanBot.Factory;
using DigitalOceanBot.Helpers;
using DigitalOceanBot.MongoDb;
using DigitalOceanBot.MongoDb.Models;
using DigitalOceanBot.Pages;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DigitalOceanBot.Commands.ProjectCommands
{
    public class GetProjectsCommand : IBotCommand, IBotCallback
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly IRepository<Session> _sessionRepo;
        private readonly IRepository<HandlerCallback> _handlerCallbackRepo;
        private readonly ILogger<DigitalOceanWorker> _logger;
        private readonly IPageFactory _pageFactory;
        private readonly IDigitalOceanClientFactory _digitalOceanClientFactory;

        public GetProjectsCommand(
            ILogger<DigitalOceanWorker> logger,
            ITelegramBotClient telegramBotClient,
            IRepository<Session> sessionRepo,
            IRepository<HandlerCallback> handlerCallbackRepo,
            IPageFactory pageFactory,
            IDigitalOceanClientFactory digitalOceanClientFactory)
        {
            _logger = logger;
            _telegramBotClient = telegramBotClient;
            _sessionRepo = sessionRepo;
            _handlerCallbackRepo = handlerCallbackRepo;
            _pageFactory = pageFactory;
            _digitalOceanClientFactory = digitalOceanClientFactory;
        }


        #region Commands

        public async void Execute(Message message, SessionState sessionState)
        {
            try
            {
                await _telegramBotClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                switch (sessionState)
                {
                    case SessionState.ProjectsMenu:
                    case SessionState.MainMenu:
                        await GetProjects(message).ConfigureAwait(false);
                        break;

                }
            }
            catch (ApiException ex)
            {
                _logger.LogError($"UserId={message.From.Id.ToString()}, Error={ex.Message}");
                await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, $"DigitalOcean API Error: {ex.Message.Replace(".", "\\.")}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"UserId={message.From.Id.ToString()}, Error={ex.Message}, StackTrace={ex.StackTrace}");
                await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Sorry, аn error has occurred \U0001F628");
            }
        }

        private async Task GetProjects(Message message)
        {
            await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "\U0001F4C0 Loading your projects...", replyMarkup: Keyboards.GetProjectsMenuKeyboard());
            var digitalOceanApi = _digitalOceanClientFactory.GetInstance(message.From.Id);
            var projects = await digitalOceanApi.Projects.GetAll();

            if (projects.Count > 0)
            {
                _sessionRepo.Update(message.From.Id, session =>
                {
                    session.Data = projects;
                    session.State = SessionState.ProjectsMenu;
                });

                var page = _pageFactory.GetInstance<ProjectPage>();
                var pageModel = page.GetPage(message.From.Id);

                var sendMessage = await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, pageModel.Message, ParseMode.Markdown, replyMarkup: pageModel.Keyboard);
                
                _handlerCallbackRepo.Update(message.From.Id, callback =>
                {
                    callback.MessageId = sendMessage.MessageId;
                    callback.UserId = message.From.Id;
                    callback.HandlerType = GetType().FullName;
                });
            }
            else
            {
                await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "You don't have a projects \U0001F914");
            }
        }

        #endregion
        
        #region Callbacks

        public async void Execute(CallbackQuery callback, Message message, SessionState sessionState)
        {
            try
            {
                await _telegramBotClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
                var callBackData = callback.Data.Split(';');

                switch (sessionState)
                {
                    case SessionState.ProjectsMenu when callBackData[0] == "NextProject" || callBackData[0] == "BackProject":
                        await NextOrBackProject(callback, message).ConfigureAwait(false);
                        break;
                    case SessionState.ProjectsMenu when callBackData[0] == "SelectProject":
                        await SelectProject(callback, message).ConfigureAwait(false);
                        break;
                }
            }
            catch (ApiException ex)
            {
                _logger.LogError($"UserId={message.From.Id.ToString()}, Error={ex.Message}");
                await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, $"DigitalOcean API Error: {ex.Message.Replace(".", "\\.")}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"UserId={message.From.Id.ToString()}, Error={ex.Message}, StackTrace={ex.StackTrace}");
                await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Sorry, аn error has occurred \U0001F628");
            }
        }

        private async Task SelectProject(CallbackQuery callback, Message message)
        {
            var projectId = callback.Data.Split(';')[1];
            var page = _pageFactory.GetInstance<ProjectPage>();
            var pageModel = page.SelectPage(callback.From.Id, projectId);

            _sessionRepo.Update(callback.From.Id, session =>
            {
                session.State = SessionState.SelectedProject;
                session.Data = projectId;
            });
            
            await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, $"Good! Now select one of the actions for this project", replyMarkup: Keyboards.GetSelectedProjectMenuKeyboard());
            await _telegramBotClient.EditMessageTextAsync(message.Chat.Id, message.MessageId, pageModel.Message, ParseMode.Markdown);
        }

        private async Task NextOrBackProject(CallbackQuery callback, Message message)
        {
            var pageCount = int.Parse(callback.Data.Split(';')[1]);
            var page = _pageFactory.GetInstance<ProjectPage>();
            var pageModel = page.GetPage(callback.From.Id, pageCount);

            await _telegramBotClient.EditMessageTextAsync(message.Chat.Id, message.MessageId, pageModel.Message, ParseMode.Markdown, replyMarkup: pageModel.Keyboard);
        }

        #endregion
    }
}
