using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using DigitalOceanBot.Helpers;
using DigitalOceanBot.MongoDb;
using DigitalOceanBot.MongoDb.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using Timer = System.Timers.Timer;

namespace DigitalOceanBot
{
    public class AutoRevokeTokenWorker : IHostedService
    {
        private readonly ILogger<AutoRevokeTokenWorker> _logger;
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly IRepository<Session> _sessionRepo;
        private readonly IRepository<DoUser> _userRepo;
        private readonly IRepository<HandlerCallback> _handlerCallbackRepo;
        private readonly Timer _timer;

        public AutoRevokeTokenWorker(
            ILogger<AutoRevokeTokenWorker> logger,
            ITelegramBotClient telegramBotClient,
            IRepository<Session> sessionRepo,
            IRepository<DoUser> userRepo,
            IRepository<HandlerCallback> handlerCallbackRepo)
        {
            _logger = logger;
            _telegramBotClient = telegramBotClient;
            _sessionRepo = sessionRepo;
            _userRepo = userRepo;
            _handlerCallbackRepo = handlerCallbackRepo;
            
            _timer = new Timer
            {
                Interval = TimeSpan.FromMinutes(1).TotalMilliseconds,
                Enabled = true,
                AutoReset = true
            };
            
            _timer.Elapsed += RevokeToken;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer.Start();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping...");
            
            _timer.Stop();
            _timer?.Dispose();

            _logger.LogInformation("Stopped");

            return Task.CompletedTask;
        }

        private async void RevokeToken(object sender, ElapsedEventArgs args)
        {
            try
            {
                var tokenManager = new TokenManager();
                var users = _userRepo.GetAll();

                foreach (var user in users)
                {
                    if ((user.TokenExpires.Ticks < DateTime.UtcNow.Ticks) && user.IsAuthorized)
                    {
                        var result = await TokenManager.RevokeToken(user.Token);
                        if (result)
                        {
                            _userRepo.Delete(user.UserId);
                            _sessionRepo.Delete(user.UserId);
                            _handlerCallbackRepo.Delete(user.UserId);

                            await _telegramBotClient.SendTextMessageAsync(user.UserId, "Your token has been auto revoked after 15 minutes. For use this bot send the command /start", replyMarkup: new ReplyKeyboardRemove());
                            _logger.LogInformation($"UserId={user.UserId.ToString()}, Message=Token has been revoked");
                        }
                        else
                        {
                            _logger.LogError($"UserId={user.UserId.ToString()} Error=Error revoke token");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error={ex.Message}, StackTrace={ex.StackTrace}");
            }
        }
    }
}
