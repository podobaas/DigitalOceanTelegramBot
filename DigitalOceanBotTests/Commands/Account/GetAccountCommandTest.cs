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

namespace DigitalOceanBotTests
{
    public class GetAccountCommandTest
    {
        ILogger<DigitalOceanWorker> logger;
        ITelegramBotClient tg;
        IRepository<DoUser> userRepo;
        IDigitalOceanClientFactory digitalOceanClientFactory;
        Message message;

        public GetAccountCommandTest()
        {
            InitTest();
        }

        private void InitTest()
        {
            logger = Substitute.For<ILogger<DigitalOceanWorker>>();
            tg = Substitute.For<ITelegramBotClient>();
            userRepo = Substitute.For<IRepository<DoUser>>();
            digitalOceanClientFactory = Substitute.For<IDigitalOceanClientFactory>();
            message = Substitute.For<Message>();

            message.From = new User { Id = 100 };
            message.Chat = new Chat { Id = 101 };

            digitalOceanClientFactory.GetInstance(Arg.Any<int>()).Account.Get()
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

            digitalOceanClientFactory.GetInstance(Arg.Any<int>()).BalanceClient.Get()
            .Returns(new Balance
            {
                AccountBalance = "10",
                MonthToDateBalance = "10",
                MonthToDateUsage = "10",
                GeneratedAt = DateTime.Now
            });

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
        }

        [Fact]
        public void GetAccountTest()
        {
            var command = Substitute.For<GetAccountCommand>(logger, tg, userRepo, digitalOceanClientFactory);
            command.Execute(message, SessionState.MainMenu);

            command.Received().Execute(message, SessionState.MainMenu);
            userRepo.Received().Get(Arg.Is<int>(i => i == 100));
            var doApi = digitalOceanClientFactory.Received().GetInstance(Arg.Is<int>(i => i == 100));
            doApi.Account.Received().Get();
            doApi.BalanceClient.Received().Get();
            tg.Received().SendTextMessageAsync(Arg.Is<ChatId>(i => i.Identifier == 101), Arg.Any<string>(), ParseMode.Markdown);
        }

        [Fact]
        public void GetAccountTest_InvalidSessionState()
        {
            var command = Substitute.For<GetAccountCommand>(logger, tg, userRepo, digitalOceanClientFactory);
            command.Execute(message, SessionState.WaitAction);

            command.Received().Execute(message, SessionState.WaitAction);
            userRepo.DidNotReceive().Get(Arg.Is<int>(i => i == 100));
            var doApi = digitalOceanClientFactory.DidNotReceive().GetInstance(Arg.Is<int>(i => i == 100));
            doApi.Account.DidNotReceive().Get();
            doApi.BalanceClient.DidNotReceive().Get();
            tg.DidNotReceive().SendTextMessageAsync(Arg.Is<ChatId>(i => i.Identifier == 101), Arg.Any<string>(), ParseMode.Markdown);
        }
    }
}
