using Responses = DigitalOcean.API.Models.Responses;
using DigitalOceanBot;
using DigitalOceanBot.Commands.DropletCommands;
using DigitalOceanBot.Factory;
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
    public class RenameDropletCommandTest
    {
        ILogger<DigitalOceanWorker> logger;
        ITelegramBotClient tg;
        IRepository<DoUser> userRepo;
        IRepository<Session> sessionRepo;
        IDigitalOceanClientFactory digitalOceanClientFactory;
        Message message;

        public RenameDropletCommandTest()
        {
            InitTest();
        }

        private void InitTest()
        {
            logger = Substitute.For<ILogger<DigitalOceanWorker>>();
            tg = Substitute.For<ITelegramBotClient>();
            userRepo = Substitute.For<IRepository<DoUser>>();
            sessionRepo = Substitute.For<IRepository<Session>>();
            digitalOceanClientFactory = Substitute.For<IDigitalOceanClientFactory>();
            message = Substitute.For<Message>();

            message.From = new User { Id = 100 };
            message.Chat = new Chat { Id = 101 };

            userRepo.Get(Arg.Any<int>()).Returns(new DoUser
            {
                UserId = 100,
                UserInfo = new UserInfo
                {
                    access_token = "token",
                    info = new Info
                    {
                        email = "test@test.com",
                        name = "test",
                        uuid = "uuid"
                    },
                }
            });
            
            sessionRepo.Get(Arg.Any<int>()).Returns(new Session
            {
                Data = 1000
            });

            digitalOceanClientFactory.GetInstance(Arg.Any<int>())
                .DropletActions
                .Rename(Arg.Any<int>(), Arg.Any<string>())
                .Returns(
                    new Responses.Action
                    {
                        Id = 200
                    });
            
            digitalOceanClientFactory.GetInstance(Arg.Any<int>())
                .DropletActions
                .GetDropletAction(Arg.Any<int>(), Arg.Any<int>())
                .Returns(
                    new Responses.Action
                    {
                        Id = 200,
                        Status = "completed"
                    });
        }

        [Fact]
        public void InputNewNameTest()
        {
            var command = Substitute.For<RenameDropletCommand>(logger, tg, userRepo, sessionRepo, digitalOceanClientFactory);
            command.Execute(message, SessionState.SelectedDroplet);

            command.Received().Execute(message, SessionState.SelectedDroplet);
            sessionRepo.Received().Update(Arg.Is<int>(i => i == 100), Arg.Invoke(new Session()));
            tg.Received().SendTextMessageAsync(Arg.Is<ChatId>(i => i.Identifier == 101), Arg.Any<string>());
        }

        [Fact]
        public void SetNewNameDroplet()
        {
            var command = Substitute.For<RenameDropletCommand>(logger, tg, userRepo, sessionRepo, digitalOceanClientFactory);
            command.Execute(message, SessionState.WaitInputNewNameDroplet);

            command.Received().Execute(message, SessionState.WaitInputNewNameDroplet);
            var doApi = digitalOceanClientFactory.Received().GetInstance(Arg.Is<int>(i => i == 100));
            sessionRepo.Received().Get(Arg.Is<int>(i => i == 100));
            doApi.DropletActions.Received().Rename(Arg.Is<int>(i => i == 1000), Arg.Any<string>());
            doApi.DropletActions.Received().GetDropletAction(Arg.Is<int>(i => i == 1000), Arg.Is<int>(i => i == 200));
            tg.Received().SendTextMessageAsync(Arg.Is<ChatId>(i => i.Identifier == 101), Arg.Any<string>());
            sessionRepo.Received().Update(Arg.Is<int>(i => i == 100), Arg.Invoke(new Session()));
        }
    }
}
