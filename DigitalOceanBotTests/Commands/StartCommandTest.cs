using DigitalOceanBot;
using DigitalOceanBot.Commands;
using DigitalOceanBot.MongoDb;
using DigitalOceanBot.MongoDb.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Xunit;

namespace DigitalOceanBotTests.Commands
{
    public class StartCommandTest
    {
        ITelegramBotClient _tg;
        IRepository<DoUser> _userRepo;
        IRepository<Session> _sessionRepo;
        IRepository<HandlerCallback> _handlerCallbakRepo;
        ILogger<DigitalOceanWorker> _logger;
        Message _message;

        public StartCommandTest()
        {
            InitTest();
        }

        private void InitTest()
        {
            _logger = Substitute.For<ILogger<DigitalOceanWorker>>();
            _tg = Substitute.For<ITelegramBotClient>();
            _userRepo = Substitute.For<IRepository<DoUser>>();
            _sessionRepo = Substitute.For<IRepository<Session>>();
            _handlerCallbakRepo = Substitute.For<IRepository<HandlerCallback>>();
            _message = Substitute.For<Message>();

            _message.From = new User { Id = 100 };
            _message.Chat = new Chat { Id = 101 };
        }

        [Fact]
        public void SendAuthUrlTest()
        {
            var command = Substitute.For<StartCommand>(_tg, _userRepo, _sessionRepo, _handlerCallbakRepo, _logger);
            command.Execute(_message, SessionState.Unknown);

            command.Received().Execute(_message, SessionState.Unknown);
            _userRepo.Received().Get(Arg.Is<int>(i => i == 100));
            _userRepo.Received().Create(Arg.Any<DoUser>());
            _sessionRepo.Received().Create(Arg.Any<Session>());
            _handlerCallbakRepo.Received().Create(Arg.Any<HandlerCallback>());
            _tg.Received().SendTextMessageAsync(Arg.Is<ChatId>(i => i.Identifier == 101), Arg.Any<string>(), replyMarkup:Arg.Any<InlineKeyboardMarkup>());
        }

        [Fact]
        public void SendAuthUrlTest_ExistsUser()
        {
            var command = Substitute.For<StartCommand>(_tg, _userRepo, _sessionRepo, _handlerCallbakRepo, _logger);
            command.Execute(_message, SessionState.Unknown);

            _userRepo.Get(Arg.Any<int>()).Returns(new DoUser { UserId = 100 });
            command.Received().Execute(_message, SessionState.Unknown);
            _userRepo.Received().Get(Arg.Is<int>(i => i == 100));
            _userRepo.DidNotReceive().Update(Arg.Is<int>(i => i == 100), Arg.Invoke(new DoUser()));
            _sessionRepo.DidNotReceive().Update(Arg.Is<int>(i => i == 100), Arg.Invoke(new Session()));
            _tg.Received().SendTextMessageAsync(Arg.Is<ChatId>(i => i.Identifier == 101), Arg.Any<string>());
        }
    }
}
