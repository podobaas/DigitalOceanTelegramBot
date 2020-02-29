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
    internal sealed class BackCommand : IBotCommand
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly IRepository<Session> _sessionRepo;
        private readonly ILogger<DigitalOceanWorker> _logger;

        public BackCommand(
            ILogger<DigitalOceanWorker> logger,
            ITelegramBotClient telegramBotClient,
            IRepository<Session> sessionRepo)
        {
            _logger = logger;
            _telegramBotClient = telegramBotClient;
            _sessionRepo = sessionRepo;
        }


        public async Task Execute(Message message, SessionState sessionState)
        {
            try
            {
                if (sessionState != SessionState.WaitAuth)
                {
                    await BackToMainMenu(message).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"UserId={message.From.Id.ToString()}, Error={ex.Message}, StackTrace={ex.StackTrace}");
                await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Sorry, аn error has occurred \U0001F628");
            }
        }

        private async Task BackToMainMenu(Message message)
        {
            _sessionRepo.Update(message.From.Id, session =>
            {
                session.State = SessionState.MainMenu;
            });

            await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "You in main menu", replyMarkup: Keyboards.GetMainMenuKeyboard());
        }
        
    }
}
