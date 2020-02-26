using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DigitalOceanBot.Helpers;
using DigitalOceanBot.MongoDb;
using DigitalOceanBot.MongoDb.Models;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DigitalOceanBot.Commands
{
    public class StartCommand : IBotCommand
    {
        private readonly ILogger<DigitalOceanWorker> _logger;
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly IRepository<DoUser> _userRepo;
        private readonly IRepository<Session> _sessionRepo;
        private readonly IRepository<HandlerCallback> _handlerCallbackRepo;

        public StartCommand(
            ITelegramBotClient telegramBotClient,
            IRepository<DoUser> userRepo,
            IRepository<Session> sessionRepo,
            IRepository<HandlerCallback> handlerCallbackRepo,
            ILogger<DigitalOceanWorker> logger)
        {
            _telegramBotClient = telegramBotClient;
            _userRepo = userRepo;
            _sessionRepo = sessionRepo;
            _logger = logger;
            _handlerCallbackRepo = handlerCallbackRepo;
        }

        public async Task Execute(Message message, SessionState sessionState)
        {
            try
            {
                await SendAuthUrl(message).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError($"UserId={message.From.Id.ToString()}, Error={ex.Message}, StackTrace={ex.StackTrace}");
                await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Sorry, аn error has occurred \U0001F628");
            }
        }

        private async Task SendAuthUrl(Message message)
        {
            var user = _userRepo.Get(message.From.Id);
            var state = $"{SecureStateString.Get(message.From.Id, message.Chat.Id)}";

            if (user == null)
            {
                _userRepo.Create(new DoUser
                {
                    UserId = message.From.Id,
                    State = state,
                    IsAuthorized = false
                });

                _sessionRepo.Create(new Session
                {
                    UserId = message.From.Id,
                    ChatId = message.Chat.Id,
                    State = SessionState.WaitAuth,
                });

                _handlerCallbackRepo.Create(new HandlerCallback
                {
                    UserId = message.From.Id,
                    MessageId = 0,
                    HandlerType = null
                });

                await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Hi! Sign In, please to DigitalOcean account", replyMarkup: GetAuthKeyboard(state));
            }
            else if(user.IsAuthorized)
            {
                await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "You are already logged in. If you want to exit the account and revoke the token, then run the command /stop");
            }
        }

        private static InlineKeyboardMarkup GetAuthKeyboard(string state)
        {
            var link = new InlineKeyboardButton
            {
                Text = "Sign In \U0001F40B",
                Url = $"{Environment.GetEnvironmentVariable("AUTH_URL")}&state={state}&scope=read write"
            };

            var buttons = new List<List<InlineKeyboardButton>>
            {
                new List<InlineKeyboardButton>
                {
                    link,
                }
            };

            return new InlineKeyboardMarkup(buttons);
        }
    }
}