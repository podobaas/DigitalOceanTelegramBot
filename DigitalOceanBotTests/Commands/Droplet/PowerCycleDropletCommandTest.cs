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
    public class PowerCycleDropletCommandTest
    {
        ILogger<DigitalOceanWorker> logger;
        ITelegramBotClient tg;
        IRepository<DoUser> userRepo;
        IRepository<Session> sessionRepo;
        IDigitalOceanClientFactory digitalOceanClientFactory;
        Message message;

        public PowerCycleDropletCommandTest()
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
                .PowerCycle(Arg.Any<int>())
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
        public void ConfirmMessageTest()
        {
            var command = Substitute.For<PowerCycleDropletCommand>(logger, tg, userRepo, sessionRepo, digitalOceanClientFactory);
            command.Execute(message, SessionState.SelectedDroplet);

            command.Received().Execute(message, SessionState.SelectedDroplet);
            sessionRepo.Received().Update(Arg.Is<int>(i => i == 100), Arg.Invoke(new Session()));
            tg.Received().SendTextMessageAsync(Arg.Is<ChatId>(i => i.Identifier == 101), Arg.Any<string>(), replyMarkup:Arg.Any<ReplyKeyboardMarkup>());
        }

        [Fact]
        public void PowerCycleDropletTest_AnswerYes()
        {
            message.Text = "Yes";
            var command = Substitute.For<PowerCycleDropletCommand>(logger, tg, userRepo, sessionRepo, digitalOceanClientFactory);
            command.Execute(message, SessionState.WaitConfirmPowerCycle);

            command.Received().Execute(message, SessionState.WaitConfirmPowerCycle);
            var doApi = digitalOceanClientFactory.Received().GetInstance(Arg.Is<int>(i => i == 100));
            sessionRepo.Received().Get(Arg.Is<int>(i => i == 100));
            doApi.DropletActions.Received().PowerCycle(Arg.Is<int>(i => i == 1000));
            doApi.DropletActions.Received().GetDropletAction(Arg.Is<int>(i => i == 1000), Arg.Is<int>(i => i == 200));
            tg.Received().SendTextMessageAsync(Arg.Is<ChatId>(i => i.Identifier == 101), Arg.Any<string>());
            sessionRepo.Received().Update(Arg.Is<int>(i => i == 100), Arg.Invoke(new Session()));
        }
        
        [Fact]
        public void PowerCycleDropletTest_AnswerNo()
        {
            message.Text = "No";
            var command = Substitute.For<PowerCycleDropletCommand>(logger, tg, userRepo, sessionRepo, digitalOceanClientFactory);
            command.Execute(message, SessionState.WaitConfirmPowerCycle);

            command.Received().Execute(message, SessionState.WaitConfirmPowerCycle);
            sessionRepo.Received().Update(Arg.Is<int>(i => i == 100), Arg.Invoke(new Session()));
            tg.Received().SendTextMessageAsync(Arg.Is<ChatId>(i => i.Identifier == 101), Arg.Any<string>(), replyMarkup:Arg.Any<ReplyKeyboardMarkup>());

            var doApi = digitalOceanClientFactory.DidNotReceive().GetInstance(Arg.Is<int>(i => i == 100));
            doApi.DropletActions.DidNotReceive().PowerCycle(Arg.Is<int>(i => i == 1000));
        }
    }
}
