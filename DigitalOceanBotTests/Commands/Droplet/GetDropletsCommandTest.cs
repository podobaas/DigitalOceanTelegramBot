using System;
using System.Collections.Generic;
using DigitalOcean.API.Models.Responses;
using DigitalOceanBot;
using DigitalOceanBot.Commands.DropletCommands;
using DigitalOceanBot.Factory;
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
    public class GetDropletsCommandTest
    {
        ILogger<DigitalOceanWorker> logger;
        ITelegramBotClient tg;
        IRepository<DoUser> userRepo;
        IRepository<Session> sessionRepo;
        IRepository<HandlerCallback> handlerCallbakRepo;
        IPageFactory pageFactory;
        IDigitalOceanClientFactory digitalOceanClientFactory;
        Message message;

        public GetDropletsCommandTest()
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
            pageFactory = Substitute.For<IPageFactory>();
            digitalOceanClientFactory = Substitute.For<IDigitalOceanClientFactory>();
            message = Substitute.For<Message>();

            message.From = new User { Id = 100 };
            message.Chat = new Chat { Id = 101 };

            digitalOceanClientFactory.GetInstance(Arg.Any<int>())
                .Droplets
                .GetAll()
                .Returns(new List<Droplet>
            {
                new Droplet
                {
                    Id = 1,
                    CreatedAt = DateTime.Now,
                    Disk = 256,
                    Memory = 1024,
                    Name = "Test droplet",
                    Status = "active",
                    Vcpus = 1,
                    Region = new Region { Name = "Test" },
                    Image = new Image { Name = "test", Distribution = "test" },
                    Tags = new List<string> { "test" },
                    Networks = new Network
                    { 
                        V4 = new List<InterfaceV4>
                        {
                            new InterfaceV4 { IpAddress = "0.0.0.0.0" }
                        },
                        V6 = new List<InterfaceV6>
                        {
                            new InterfaceV6 { IpAddress = "000:0000:000" }
                        }
                    }
                }
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

            pageFactory.GetInstance<IPage>()
                .GetPage(Arg.Any<int>(), Arg.Any<int>())
                .Returns(new PageModel
                {
                    Message = "Test message",
                    Keyboard = new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>())
                });
        }

        [Fact]
        public void GetDropletsTest()
        {
            var command = Substitute.For<GetDropletsCommand>(logger, tg, sessionRepo, handlerCallbakRepo, pageFactory, digitalOceanClientFactory);
            command.Execute(message, SessionState.MainMenu);
            
            command.Received().Execute(message, SessionState.MainMenu);
            var doApi = digitalOceanClientFactory.Received().GetInstance(Arg.Is<int>(i => i == 100));
            doApi.Droplets.Received().GetAll();
            pageFactory
                .Received().GetInstance<DropletPage>()
                .Received().GetPage(Arg.Is<int>(i => i == 100));
            sessionRepo.Received().Update(Arg.Is<int>(i => i == 100), Arg.Invoke(new Session()));
            handlerCallbakRepo.Received().Update(Arg.Is<int>(i => i == 100), Arg.Invoke(new HandlerCallback()));
            tg.Received().SendTextMessageAsync(Arg.Is<ChatId>(i => i.Identifier == 101), Arg.Any<string>(), ParseMode.Markdown, replyMarkup: Arg.Any<InlineKeyboardMarkup>());
        }

        [Fact]
        public void GetDropletsTest_InvalidSession()
        {
            InitTest();
            var command = Substitute.For<GetDropletsCommand>(logger, tg, sessionRepo, handlerCallbakRepo, pageFactory, digitalOceanClientFactory);
            command.Execute(message, SessionState.WaitAction);

            command.Received().Execute(message, SessionState.WaitAction);
            var doApi = digitalOceanClientFactory.DidNotReceive().GetInstance(Arg.Is<int>(i => i == 100));
            doApi.Droplets.DidNotReceive().GetAll();
            sessionRepo.DidNotReceive().Update(Arg.Is<int>(i => i == 100), Arg.Invoke(new Session()));
            handlerCallbakRepo.DidNotReceive().Update(Arg.Is<int>(i => i == 100), Arg.Invoke(new HandlerCallback()));
            tg.DidNotReceive().SendTextMessageAsync(Arg.Is<ChatId>(i => i.Identifier == 101), Arg.Any<string>(), replyMarkup: Arg.Any<InlineKeyboardMarkup>());
        }

        [Fact]
        public void GetDropletsTest_ZeroCountDroplets()
        {
            digitalOceanClientFactory.GetInstance(Arg.Any<int>())
                .Droplets
                .GetAll()
                .Returns(new List<Droplet>());

            var command = Substitute.For<GetDropletsCommand>(logger, tg, sessionRepo, handlerCallbakRepo, pageFactory, digitalOceanClientFactory);
            command.Execute(message, SessionState.MainMenu);

            command.Received().Execute(message, SessionState.MainMenu);
            var doApi = digitalOceanClientFactory.Received().GetInstance(Arg.Is<int>(i => i == 100));
            doApi.Droplets.Received().GetAll();
            sessionRepo.DidNotReceive().Update(Arg.Is<int>(i => i == 100), Arg.Invoke(new Session()));
            handlerCallbakRepo.DidNotReceive().Update(Arg.Is<int>(i => i == 100), Arg.Invoke(new HandlerCallback()));
            tg.Received().SendTextMessageAsync(Arg.Is<ChatId>(i => i.Identifier == 101), Arg.Any<string>());
        }
    }
}
