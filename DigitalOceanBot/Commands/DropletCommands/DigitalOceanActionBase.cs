using System;
using System.Threading.Tasks;
using DigitalOcean.API;
using DigitalOcean.API.Exceptions;
using DigitalOceanBot.Factory;
using DigitalOceanBot.Helpers;
using DigitalOceanBot.MongoDb;
using DigitalOceanBot.MongoDb.Models;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DigitalOceanBot.Commands.DropletCommands
{
    public abstract class DigitalOceanActionBase
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly IRepository<DoUser> _userRepo;
        private readonly IRepository<Session> _sessionRepo;
        private readonly ILogger<DigitalOceanWorker> _logger;
        private readonly IDigitalOceanClientFactory _digitalOceanClientFactory;

        public DigitalOceanActionBase(
            ILogger<DigitalOceanWorker> logger,
            ITelegramBotClient telegramBotClient,
            IRepository<DoUser> userRepo,
            IRepository<Session> sessionRepo,
            IDigitalOceanClientFactory digitalOceanClientFactory)
        {
            _logger = logger;
            _telegramBotClient = telegramBotClient;
            _userRepo = userRepo;
            _sessionRepo = sessionRepo;
            _digitalOceanClientFactory = digitalOceanClientFactory;
        }


        protected async void StartActionWithConfirm(Message message, string actionName, Func<IDigitalOceanClient, int, Task<DigitalOcean.API.Models.Responses.Action>> func)
        {
            try
            {
                if (message.Text == "Yes")
                {
                    var digitalOceanApi = _digitalOceanClientFactory.GetInstance(message.From.Id);
                    var session = _sessionRepo.Get(message.From.Id);
                    var dropletId = session.Data.CastObject<int>();
                    var action = await func(digitalOceanApi, dropletId);
                    await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, $"\U0001F4C0 {actionName}...");

                    _sessionRepo.Update(message.From.Id, session =>
                    {
                        session.State = SessionState.WaitAction;
                    });

                    var resultStatus = await CheckActionStatus(dropletId, action.Id, digitalOceanApi);
                    if (resultStatus)
                    {
                        await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Done \U00002705", replyMarkup: Keyboards.GetSelectedDropletsMenuKeyboard());
                    }
                    else
                    {
                        await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Error \U0000274C", replyMarkup: Keyboards.GetSelectedDropletsMenuKeyboard());
                    }

                    _sessionRepo.Update(message.From.Id, session =>
                    {
                        session.State = SessionState.SelectedDroplet;
                    });
                }
                else if (message.Text == "No")
                {
                    _sessionRepo.Update(message.From.Id, session =>
                    {
                        session.State = SessionState.SelectedDroplet;
                    });

                    await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Сanceled \U0001F630", replyMarkup: Keyboards.GetSelectedDropletsMenuKeyboard());
                }
            }
            catch (ApiException ex)
            {
                _logger.LogError($"UserId={message.From.Id.ToString()}, Error={ex.Message}");
                await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, $"DigitalOcean API Error: {ex.Message.Replace(".", "\\.")}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"UserId={message.From.Id.ToString()}, ChatId={message.Chat.Id.ToString()}, Error={ex.Message}, StackTrace={ex.StackTrace}");
                await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Sorry, аn error has occurred \U0001F628", replyMarkup: Keyboards.GetSelectedDropletsMenuKeyboard());
            }
        }

        protected async void StartActionWithoutConfirm(Message message, string actionName, Func<IDigitalOceanClient, int, Task<DigitalOcean.API.Models.Responses.Action>> func)
        {
            try
            {
                var digitalOceanApi = _digitalOceanClientFactory.GetInstance(message.From.Id);
                var session = _sessionRepo.Get(message.From.Id);
                var dropletId = session.Data.CastObject<int>();
                var action = await func(digitalOceanApi, dropletId);
                await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, $"\U0001F4C0 {actionName}...");

                _sessionRepo.Update(message.From.Id, session =>
                {
                    session.State = SessionState.WaitAction;
                });

                var resultStatus = await CheckActionStatus(dropletId, action.Id, digitalOceanApi);
                if (resultStatus)
                {
                    await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Done \U00002705", replyMarkup: Keyboards.GetSelectedDropletsMenuKeyboard());
                }
                else
                {
                    await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Error \U0000274C", replyMarkup: Keyboards.GetSelectedDropletsMenuKeyboard());
                }

                _sessionRepo.Update(message.From.Id, session =>
                {
                    session.State = SessionState.SelectedDroplet;
                });
            }
            catch (ApiException ex)
            {
                _logger.LogError($"UserId={message.From.Id.ToString()}, Error={ex.Message}");
                await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, $"DigitalOcean API Error: {ex.Message.Replace(".", "\\.")}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"UserId={message.From.Id.ToString()}, ChatId={message.Chat.Id.ToString()}, Error={ex.Message}, StackTrace={ex.StackTrace}");
                await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Sorry, аn error has occurred \U0001F628", replyMarkup: Keyboards.GetSelectedDropletsMenuKeyboard());
            }
        }

        protected async void ConfirmMessage(Message message, SessionState sessionState)
        {
            _sessionRepo.Update(message.From.Id, (session) =>
            {
                session.State = sessionState;
            });

            await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, $"You sure? \U0001F914", replyMarkup: Keyboards.GetConfirmKeyboard());
        }

        private async Task<bool> CheckActionStatus(int dropletId, int actionId, IDigitalOceanClient digitalOceanClient)
        {
            while (true)
            {
                var action = await digitalOceanClient.DropletActions.GetDropletAction(dropletId, actionId);
                if (action.Status == "completed")
                {
                    return true;
                }
                else if (action.Status == "errored")
                {
                    return false;
                }

                await Task.Delay(3000);
            }
        }
    }
}