using Responses = DigitalOcean.API.Models.Responses;
using DigitalOceanBot.MongoDb;
using DigitalOceanBot.MongoDb.Models;

namespace DigitalOceanBotTests.Commands.DropletCommands
{
    public class RenameDropletCommandTest
    {
        ILogger<DigitalOceanWorker> _logger;
        ITelegramBotClient _tg;
        IRepository<DoUser> _userRepo;
        IRepository<Session> _sessionRepo;
        IDigitalOceanClientFactory _digitalOceanClientFactory;
        Message _message;

        public RenameDropletCommandTest()
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
                .Rename(Arg.Any<int>(), Arg.Any<string>())
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
        public void InputNewNameTest()
        {
            var command = Substitute.For<RenameDropletCommand>(_logger, _tg, _sessionRepo, _digitalOceanClientFactory);
            command.Execute(_message, SessionState.SelectedDroplet);

            command.Received().Execute(_message, SessionState.SelectedDroplet);
            _sessionRepo.Received().Update(Arg.Is<int>(i => i == 100), Arg.Invoke(new Session()));
            _tg.Received().SendTextMessageAsync(Arg.Is<ChatId>(i => i.Identifier == 101), Arg.Any<string>());
        }

        [Fact]
        public void SetNewNameDroplet()
        {
            var command = Substitute.For<RenameDropletCommand>(_logger, _tg, _sessionRepo, _digitalOceanClientFactory);
            command.Execute(_message, SessionState.WaitInputNameForNewDroplet);

            command.Received().Execute(_message, SessionState.WaitInputNameForNewDroplet);
            var doApi = _digitalOceanClientFactory.Received().GetInstance(Arg.Is<int>(i => i == 100));
            _sessionRepo.Received().Get(Arg.Is<int>(i => i == 100));
            doApi.DropletActions.Received().Rename(Arg.Is<int>(i => i == 1000), Arg.Any<string>());
            doApi.DropletActions.Received().GetDropletAction(Arg.Is<int>(i => i == 1000), Arg.Is<int>(i => i == 200));
            _tg.Received().SendTextMessageAsync(Arg.Is<ChatId>(i => i.Identifier == 101), Arg.Any<string>());
            _sessionRepo.Received().Update(Arg.Is<int>(i => i == 100), Arg.Invoke(new Session()));
        }
    }
}
