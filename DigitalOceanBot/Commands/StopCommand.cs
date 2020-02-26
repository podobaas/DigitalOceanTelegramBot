using System;
using System.Threading.Tasks;
using DigitalOceanBot.Helpers;
using DigitalOceanBot.MongoDb;
using DigitalOceanBot.MongoDb.Models;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DigitalOceanBot.Commands
{
    public class StopCommand : IBotCommand
    {
        private readonly ILogger<DigitalOceanWorker> _logger;
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly IRepository<DoUser> _userRepo;
        private readonly IRepository<Session> _sessionRepo;
        private readonly IRepository<HandlerCallback> _handlerCallbackRepo;

        public StopCommand(
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
                await Stop(message).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError($"UserId={message.From.Id.ToString()}, Error={ex.Message}, StackTrace={ex.StackTrace}");
                await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Sorry, аn error has occurred \U0001F628");
            }
        }

        private async Task Stop(Message message)
        {
            var user = _userRepo.Get(message.From.Id);
            if (user != null && user.IsAuthorized)
            {
                var tokenManager = new TokenManager();
                var result = await tokenManager.RevokeToken(user.Token);

                if (result)
                {
                    _userRepo.Delete(message.From.Id);
                    _sessionRepo.Delete(message.From.Id);
                    _handlerCallbackRepo.Delete(message.From.Id);

                    await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Token has been revoked and deleted from database. For use this bot send the command /start", replyMarkup: null);
                }
                else
                {
                    await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Sorry, token revocation error. Please, try again.");
                }
            }
        }
    }
}