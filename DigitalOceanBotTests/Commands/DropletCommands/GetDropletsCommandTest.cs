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

namespace DigitalOceanBotTests.Commands.DropletCommands
{
    public class GetDropletsCommandTest
    {
        ILogger<DigitalOceanWorker> _logger;
        ITelegramBotClient _tg;
        IRepository<DoUser> _userRepo;
        IRepository<Session> _sessionRepo;
        IRepository<HandlerCallback> _handlerCallbackRepo;
        IPageFactory _pageFactory;
        IDigitalOceanClientFactory _digitalOceanClientFactory;
        Message _message;

        public GetDropletsCommandTest()
        {
            InitTest();
        }

        private void InitTest()
        {
            _logger = Substitute.For<ILogger<DigitalOceanWorker>>();
            _tg = Substitute.For<ITelegramBotClient>();
            _userRepo = Substitute.For<IRepository<DoUser>>();
            _sessionRepo = Substitute.For<IRepository<Session>>();
            _handlerCallbackRepo = Substitute.For<IRepository<HandlerCallback>>();
            _pageFactory = Substitute.For<IPageFactory>();
            _digitalOceanClientFactory = Substitute.For<IDigitalOceanClientFactory>();
            _message = Substitute.For<Message>();

            _message.From = new User { Id = 100 };
            _message.Chat = new Chat { Id = 101 };

            _digitalOceanClientFactory.GetInstance(Arg.Any<int>())
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

            _userRepo.Get(Arg.Any<int>()).Returns(new DoUser
            {
                UserId = 100,
                Token = "token"
            });

            _pageFactory.GetInstance<IPage>()
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
            var command = Substitute.For<GetDropletsCommand>(_logger, _tg, _sessionRepo, _handlerCallbackRepo, _pageFactory, _digitalOceanClientFactory);
            command.Execute(_message, SessionState.MainMenu);
            
            command.Received().Execute(_message, SessionState.MainMenu);
            var doApi = _digitalOceanClientFactory.Received().GetInstance(Arg.Is<int>(i => i == 100));
            doApi.Droplets.Received().GetAll();
            _pageFactory
                .Received().GetInstance<DropletPage>()
                .Received().GetPage(Arg.Is<int>(i => i == 100));
            _sessionRepo.Received().Update(Arg.Is<int>(i => i == 100), Arg.Invoke(new Session()));
            _handlerCallbackRepo.Received().Update(Arg.Is<int>(i => i == 100), Arg.Invoke(new HandlerCallback()));
            _tg.Received().SendTextMessageAsync(Arg.Is<ChatId>(i => i.Identifier == 101), Arg.Any<string>(), ParseMode.Markdown, replyMarkup: Arg.Any<InlineKeyboardMarkup>());
        }

        [Fact]
        public void GetDropletsTest_InvalidSession()
        {
            InitTest();
            var command = Substitute.For<GetDropletsCommand>(_logger, _tg, _sessionRepo, _handlerCallbackRepo, _pageFactory, _digitalOceanClientFactory);
            command.Execute(_message, SessionState.WaitAction);

            command.Received().Execute(_message, SessionState.WaitAction);
            var doApi = _digitalOceanClientFactory.DidNotReceive().GetInstance(Arg.Is<int>(i => i == 100));
            doApi.Droplets.DidNotReceive().GetAll();
            _sessionRepo.DidNotReceive().Update(Arg.Is<int>(i => i == 100), Arg.Invoke(new Session()));
            _handlerCallbackRepo.DidNotReceive().Update(Arg.Is<int>(i => i == 100), Arg.Invoke(new HandlerCallback()));
            _tg.DidNotReceive().SendTextMessageAsync(Arg.Is<ChatId>(i => i.Identifier == 101), Arg.Any<string>(), replyMarkup: Arg.Any<InlineKeyboardMarkup>());
        }

        [Fact]
        public void GetDropletsTest_ZeroCountDroplets()
        {
            _digitalOceanClientFactory.GetInstance(Arg.Any<int>())
                .Droplets
                .GetAll()
                .Returns(new List<Droplet>());

            var command = Substitute.For<GetDropletsCommand>(_logger, _tg, _sessionRepo, _handlerCallbackRepo, _pageFactory, _digitalOceanClientFactory);
            command.Execute(_message, SessionState.MainMenu);

            command.Received().Execute(_message, SessionState.MainMenu);
            var doApi = _digitalOceanClientFactory.Received().GetInstance(Arg.Is<int>(i => i == 100));
            doApi.Droplets.Received().GetAll();
            _sessionRepo.DidNotReceive().Update(Arg.Is<int>(i => i == 100), Arg.Invoke(new Session()));
            _handlerCallbackRepo.DidNotReceive().Update(Arg.Is<int>(i => i == 100), Arg.Invoke(new HandlerCallback()));
            _tg.Received().SendTextMessageAsync(Arg.Is<ChatId>(i => i.Identifier == 101), Arg.Any<string>());
        }
    }
}
