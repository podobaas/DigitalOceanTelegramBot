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

namespace DigitalOceanBotTests
{
    public class StartCommandTest
    {
        ITelegramBotClient tg;
        IRepository<DoUser> userRepo;
        IRepository<Session> sessionRepo;
        IRepository<HandlerCallback> handlerCallbakRepo;
        ILogger<DigitalOceanWorker> logger;
        Message message;

        public StartCommandTest()
        {
            InitTest();
        }

        private void InitTest()
        {
            logger = Substitute.For<ILogger<DigitalOceanWorker>>();
            tg = Substitute.For<ITelegramBotClient>();
            userRepo = Substitute.For<IRepository<DoUser>>();
            sessionRepo = Substitute.For<IRepository<Session>>();
            handlerCallbakRepo = Substitute.For<IRepository<HandlerCallback>>();
            message = Substitute.For<Message>();

            message.From = new User { Id = 100 };
            message.Chat = new Chat { Id = 101 };
        }

        [Fact]
        public void SendAuthUrlTest()
        {
            var command = Substitute.For<StartCommand>(tg, userRepo, sessionRepo, handlerCallbakRepo, logger);
            command.Execute(message, SessionState.Unknow);

            command.Received().Execute(message, SessionState.Unknow);
            userRepo.Received().Get(Arg.Is<int>(i => i == 100));
            userRepo.Received().Create(Arg.Any<DoUser>());
            sessionRepo.Received().Create(Arg.Any<Session>());
            handlerCallbakRepo.Received().Create(Arg.Any<HandlerCallback>());
            tg.Received().SendTextMessageAsync(Arg.Is<ChatId>(i => i.Identifier == 101), Arg.Any<string>(), replyMarkup:Arg.Any<InlineKeyboardMarkup>());
        }

        [Fact]
        public void SendAuthUrlTest_ExistsUser()
        {
            var command = Substitute.For<StartCommand>(tg, userRepo, sessionRepo, handlerCallbakRepo, logger);
            command.Execute(message, SessionState.Unknow);

            userRepo.Get(Arg.Any<int>()).Returns(new DoUser { UserId = 100 });
            command.Received().Execute(message, SessionState.Unknow);
            userRepo.Received().Get(Arg.Is<int>(i => i == 100));
            userRepo.DidNotReceive().Update(Arg.Is<int>(i => i == 100), Arg.Invoke(new DoUser()));
            sessionRepo.DidNotReceive().Update(Arg.Is<int>(i => i == 100), Arg.Invoke(new Session()));
            tg.Received().SendTextMessageAsync(Arg.Is<ChatId>(i => i.Identifier == 101), Arg.Any<string>());
        }
    }
}
