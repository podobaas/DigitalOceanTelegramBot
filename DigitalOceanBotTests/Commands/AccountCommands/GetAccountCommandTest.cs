using System;
using DigitalOcean.API.Models.Responses;
using DigitalOceanBot;
using DigitalOceanBot.Commands.AccountCommands;
using DigitalOceanBot.Factory;
using DigitalOceanBot.MongoDb;
using DigitalOceanBot.MongoDb.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Xunit;

namespace DigitalOceanBotTests.Commands.AccountCommands
{
    public class GetAccountCommandTest
    {
        ILogger<DigitalOceanWorker> _logger;
        ITelegramBotClient _tg;
        IRepository<DoUser> _userRepo;
        IDigitalOceanClientFactory _digitalOceanClientFactory;
        Message _message;

        public GetAccountCommandTest()
        {
            InitTest();
        }

        private void InitTest()
        {
            _logger = Substitute.For<ILogger<DigitalOceanWorker>>();
            _tg = Substitute.For<ITelegramBotClient>();
            _userRepo = Substitute.For<IRepository<DoUser>>();
            _digitalOceanClientFactory = Substitute.For<IDigitalOceanClientFactory>();
            _message = Substitute.For<Message>();

            _message.From = new User { Id = 100 };
            _message.Chat = new Chat { Id = 101 };

            _digitalOceanClientFactory.GetInstance(Arg.Any<int>()).Account.Get()
            .Returns(new Account
            {
                DropletLimit = 10,
                FloatingIpLimit = 1,
                Email = "test@test.com",
                EmailVerified = true,
                Status = "active",
                StatusMessage = string.Empty,
                Uuid = "uuid"
            });

            _digitalOceanClientFactory.GetInstance(Arg.Any<int>()).BalanceClient.Get()
            .Returns(new Balance
            {
                AccountBalance = "10",
                MonthToDateBalance = "10",
                MonthToDateUsage = "10",
                GeneratedAt = DateTime.Now
            });

            _userRepo.Get(Arg.Any<int>()).Returns(new DoUser
            {
                UserId = 100,
                Token = "token"
            });
        }

        [Fact]
        public void GetAccountTest()
        {
            var command = Substitute.For<GetAccountCommand>(_logger, _tg, _digitalOceanClientFactory);
            command.Execute(_message, SessionState.MainMenu);

            command.Received().Execute(_message, SessionState.MainMenu);
            var doApi = _digitalOceanClientFactory.Received().GetInstance(Arg.Is<int>(i => i == 100));
            doApi.Account.Received().Get();
            doApi.BalanceClient.Received().Get();
            _tg.Received().SendTextMessageAsync(Arg.Is<ChatId>(i => i.Identifier == 101), Arg.Any<string>(), ParseMode.Markdown);
        }

        [Fact]
        public void GetAccountTest_InvalidSessionState()
        {
            var command = Substitute.For<GetAccountCommand>(_logger, _tg, _digitalOceanClientFactory);
            command.Execute(_message, SessionState.WaitAction);

            command.Received().Execute(_message, SessionState.WaitAction);
            var doApi = _digitalOceanClientFactory.DidNotReceive().GetInstance(Arg.Is<int>(i => i == 100));
            doApi.Account.DidNotReceive().Get();
            doApi.BalanceClient.DidNotReceive().Get();
            _tg.DidNotReceive().SendTextMessageAsync(Arg.Is<ChatId>(i => i.Identifier == 101), Arg.Any<string>(), ParseMode.Markdown);
        }
    }
}
