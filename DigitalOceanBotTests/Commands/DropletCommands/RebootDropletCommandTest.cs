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

namespace DigitalOceanBotTests.Commands.DropletCommands
{
    public class RebootDropletCommandTest
    {
        ILogger<DigitalOceanWorker> _logger;
        ITelegramBotClient _tg;
        IRepository<DoUser> _userRepo;
        IRepository<Session> _sessionRepo;
        IDigitalOceanClientFactory _digitalOceanClientFactory;
        Message _message;

        public RebootDropletCommandTest()
        {
            InitTest();
        }

        private void InitTest()
        {
            _logger = Substitute.For<ILogger<DigitalOceanWorker>>();
            _tg = Substitute.For<ITelegramBotClient>();
            _userRepo = Substitute.For<IRepository<DoUser>>();
            _sessionRepo = Substitute.For<IRepository<Session>>();
            _digitalOceanClientFactory = Substitute.For<IDigitalOceanClientFactory>();
            _message = Substitute.For<Message>();

            _message.From = new User { Id = 100 };
            _message.Chat = new Chat { Id = 101 };

            _userRepo.Get(Arg.Any<int>()).Returns(new DoUser
            {
                UserId = 100,
                Token = "token"
            });
            
            _sessionRepo.Get(Arg.Any<int>()).Returns(new Session
            {
                Data = 1000
            });

            _digitalOceanClientFactory.GetInstance(Arg.Any<int>())
                .DropletActions
                .Reboot(Arg.Any<int>())
                .Returns(
                    new Responses.Action
                    {
                        Id = 200
                    });
            
            _digitalOceanClientFactory.GetInstance(Arg.Any<int>())
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
            var command = Substitute.For<RebootDropletCommand>(_logger, _tg, _userRepo, _sessionRepo, _digitalOceanClientFactory);
            command.Execute(_message, SessionState.SelectedDroplet);

            command.Received().Execute(_message, SessionState.SelectedDroplet);
            _sessionRepo.Received().Update(Arg.Is<int>(i => i == 100), Arg.Invoke(new Session()));
            _tg.Received().SendTextMessageAsync(Arg.Is<ChatId>(i => i.Identifier == 101), Arg.Any<string>(), replyMarkup:Arg.Any<ReplyKeyboardMarkup>());
        }

        [Fact]
        public void RebootDropletTest_AnswerYes()
        {
            _message.Text = "Yes";
            var command = Substitute.For<RebootDropletCommand>(_logger, _tg, _userRepo, _sessionRepo, _digitalOceanClientFactory);
            command.Execute(_message, SessionState.WaitConfirmReboot);

            command.Received().Execute(_message, SessionState.WaitConfirmReboot);
            var doApi = _digitalOceanClientFactory.Received().GetInstance(Arg.Is<int>(i => i == 100));
            _sessionRepo.Received().Get(Arg.Is<int>(i => i == 100));
            doApi.DropletActions.Received().Reboot(Arg.Is<int>(i => i == 1000));
            doApi.DropletActions.Received().GetDropletAction(Arg.Is<int>(i => i == 1000), Arg.Is<int>(i => i == 200));
            _tg.Received().SendTextMessageAsync(Arg.Is<ChatId>(i => i.Identifier == 101), Arg.Any<string>());
            _sessionRepo.Received().Update(Arg.Is<int>(i => i == 100), Arg.Invoke(new Session()));
        }
        
        [Fact]
        public void RebootDropletTest_AnswerNo()
        {
            _message.Text = "No";
            var command = Substitute.For<RebootDropletCommand>(_logger, _tg, _userRepo, _sessionRepo, _digitalOceanClientFactory);
            command.Execute(_message, SessionState.WaitConfirmReboot);

            command.Received().Execute(_message, SessionState.WaitConfirmReboot);
            _sessionRepo.Received().Update(Arg.Is<int>(i => i == 100), Arg.Invoke(new Session()));
            _tg.Received().SendTextMessageAsync(Arg.Is<ChatId>(i => i.Identifier == 101), Arg.Any<string>(), replyMarkup:Arg.Any<ReplyKeyboardMarkup>());

            var doApi = _digitalOceanClientFactory.DidNotReceive().GetInstance(Arg.Is<int>(i => i == 100));
            doApi.DropletActions.DidNotReceive().Reboot(Arg.Is<int>(i => i == 1000));
        }
    }
}
