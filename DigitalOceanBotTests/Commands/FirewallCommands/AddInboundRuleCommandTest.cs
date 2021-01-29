using DigitalOceanBot.MongoDb;
using DigitalOceanBot.MongoDb.Models;

namespace DigitalOceanBotTests.Commands.FirewallCommands
{
    public class AddInboundRuleCommandTest
    {
        ILogger<DigitalOceanWorker> _logger;
        ITelegramBotClient _tg;
        IRepository<DoUser> _userRepo;
        IRepository<Session> _sessionRepo;
        IDigitalOceanClientFactory _digitalOceanClientFactory;
        Message _message;

        public AddInboundRuleCommandTest()
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
                Data = "1"
            });
        }

        [Fact]
        public void InputInboundRuleTest()
        {
            var command = Substitute.For<AddInboundRuleCommand>(_logger, _tg, _sessionRepo, _digitalOceanClientFactory);
            command.Execute(_message, SessionState.SelectedFirewall);
            
            command.Received().Execute(_message, SessionState.SelectedFirewall);
            _sessionRepo.Received().Update(Arg.Is<int>(i => i == 100), Arg.Invoke(new Session()));
            _tg.Received().SendTextMessageAsync(Arg.Is<ChatId>(i => i.Identifier == 101), Arg.Any<string>(), ParseMode.Markdown);
        }
        
        [Fact]
        public void AddInboundRuleTest()
        {
            _message.Text = "tcp:80:0.0.0.0/0";
            var command = Substitute.For<AddInboundRuleCommand>(_logger, _tg, _sessionRepo, _digitalOceanClientFactory);
            command.Execute(_message, SessionState.WaitInputAddInboundRuleFirewall);

            command.Received().Execute(_message, SessionState.WaitInputAddInboundRuleFirewall);
            _sessionRepo.Received().Get(Arg.Is<int>(i => i == 100));
            _digitalOceanClientFactory.Received().GetInstance(Arg.Is<int>(i => i == 100));
            _tg.Received().SendTextMessageAsync(Arg.Is<ChatId>(i => i.Identifier == 101), Arg.Any<string>());
        }
    }
}
