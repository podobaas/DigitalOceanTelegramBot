using System;
using System.Collections.Generic;
using DigitalOcean.API.Models.Responses;
using DigitalOceanBot;
using DigitalOceanBot.Commands.DropletCommands;
using DigitalOceanBot.Factory;
using DigitalOceanBot.Helpers;
using DigitalOceanBot.Models;
using DigitalOceanBot.MongoDb;
using DigitalOceanBot.MongoDb.Models;
using DigitalOceanBot.Pages;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Xunit;

namespace DigitalOceanBotTests
{
    public class RebootDropletCommandTest
    {
        ILogger<DigitalOceanWorker> logger;
        ITelegramBotClient tg;
        IRepository<DoUser> userRepo;
        IRepository<Session> sessionRepo;
        IDigitalOceanClientFactory digitalOceanClientFactory;
        Message message;

        public RebootDropletCommandTest()
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
        }

        [Fact]
        public void ConfirmMessageTest()
        {
            var command = Substitute.For<RebootDropletCommand>(logger, tg, userRepo, sessionRepo, digitalOceanClientFactory);
            command.Execute(message, SessionState.SelectedDroplet);

            command.Received().Execute(message, SessionState.SelectedDroplet);
            sessionRepo.Received().Update(Arg.Is<int>(i => i == 100), Arg.Invoke(new Session()));
            tg.Received().SendTextMessageAsync(Arg.Is<ChatId>(i => i.Identifier == 101), Arg.Any<string>(), replyMarkup: Keyboards.GetConfirmKeyboard());
        }

        [Fact]
        public void RebootDropletTest_AnswerYes()
        {
            message.Text = "Yes";
            var command = Substitute.For<RebootDropletCommand>(logger, tg, userRepo, sessionRepo, digitalOceanClientFactory);
            command.Execute(message, SessionState.WaitConfirmReboot);

            command.Received().Execute(message, SessionState.WaitConfirmReboot);
            var doApi = digitalOceanClientFactory.Received().GetInstance(Arg.Is<int>(i => i == 100));
            sessionRepo.Received().Get(Arg.Is<int>(i => i == 100));
            doApi.DropletActions.Received().Reboot(Arg.Is<int>(i => i == 1000));
            tg.Received(2).SendTextMessageAsync(Arg.Is<ChatId>(i => i.Identifier == 101), Arg.Any<string>());
            sessionRepo.Received(2).Update(Arg.Is<int>(i => i == 100), Arg.Invoke(new Session()));
        }
    }
}
