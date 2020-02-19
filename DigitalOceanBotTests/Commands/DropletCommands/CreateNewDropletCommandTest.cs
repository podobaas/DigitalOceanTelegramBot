using System;
using System.Collections.Generic;
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
using Requests = DigitalOcean.API.Models.Requests;
using Responses = DigitalOcean.API.Models.Responses;

namespace DigitalOceanBotTests.Commands.DropletCommands
{
    public class CreateNewDropletCommandTest
    {
        ILogger<DigitalOceanWorker> _logger;
        ITelegramBotClient _tg;
        IRepository<DoUser> _userRepo;
        IRepository<Session> _sessionRepo;
        IRepository<HandlerCallback> _handlerCallbackRepo;
        IPageFactory _pageFactory;
        IDigitalOceanClientFactory _digitalOceanClientFactory;
        Message _message;
        CallbackQuery _callbackQuery;

        public CreateNewDropletCommandTest()
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
            _callbackQuery = Substitute.For<CallbackQuery>();

            _message.From = new User { Id = 100 };
            _message.Chat = new Chat { Id = 101 };

            _callbackQuery.From = _message.From;
            _callbackQuery.Message = _message;

            _digitalOceanClientFactory.GetInstance(Arg.Any<int>())
                .Images
                .GetAll(Requests.ImageType.Distribution)
                .Returns(new List<Responses.Image>
                {
                    new Responses.Image
                    {
                        CreatedAt = DateTime.Now,
                        Id = 1,
                        MinDiskSize = 1,
                        SizeGigabytes = 20,
                        Name = "Test OS",
                        Distribution = "test-dist",
                        Public = true,
                        Status = "active",
                        Slug = "slug",
                        Regions = new List<string>
                        {
                           "reg1",
                           "reg2"
                        },
                        Type = "type"
                    }
                });

            _digitalOceanClientFactory.GetInstance(Arg.Any<int>())
                .Regions
                .GetAll()
                .Returns(new List<Responses.Region>
                {
                    new Responses.Region
                    {
                        Available = true,
                        Name = "reg1",
                        Slug = "slug",
                        Sizes = new List<string>
                        {
                            "size-1",
                            "size-2"
                        }
                    }
                });

            _digitalOceanClientFactory.GetInstance(Arg.Any<int>())
                .Sizes
                .GetAll()
                .Returns(new List<Responses.Size>
                {
                    new Responses.Size
                    {
                        Available = true,
                        Memory = 1024,
                        Disk = 25,
                        Slug = "slug",
                        PriceHourly = decimal.Zero,
                        PriceMonthly = decimal.One,
                        Vcpus = 1,
                        Regions = new List<string>
                        {
                            "reg1",
                            "reg2"
                        }
                    }
                });

            _pageFactory.GetInstance<IPage>()
                .GetPage(Arg.Any<int>(), Arg.Any<int>())
                .Returns(new PageModel
                {
                    Message = "Test message",
                    Keyboard = new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>())
                });

            _pageFactory.GetInstance<IPage>()
                .SelectPage(Arg.Any<int>(), Arg.Any<object>())
                .Returns(new PageModel
                {
                    Message = "Test message",
                    Keyboard = new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>())
                });

            _userRepo.Get(Arg.Any<int>())
                .Returns(new DoUser
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
        public void InputNameDropletTest()
        {
            var command = Substitute.For<CreateNewDropletCommand>(_logger, _tg, _sessionRepo, _handlerCallbackRepo, _pageFactory, _digitalOceanClientFactory);
            command.Execute(_message, SessionState.DropletsMenu);
            
            command.Received().Execute(_message, SessionState.DropletsMenu);
            _sessionRepo.Received().Update(Arg.Is<int>(i => i == 100), Arg.Invoke(new Session()));
            _tg.SendTextMessageAsync(Arg.Is<ChatId>(i => i.Identifier == 101), Arg.Any<string>());
        }

        [Fact]
        public void ChooseImageTest()
        {
            var command = Substitute.For<CreateNewDropletCommand>(_logger, _tg, _sessionRepo, _handlerCallbackRepo, _pageFactory, _digitalOceanClientFactory);
            command.Execute(_message, SessionState.WaitInputNameDroplet);

            command.Received().Execute(_message, SessionState.WaitInputNameDroplet);
            var doApi = _digitalOceanClientFactory.Received().GetInstance(Arg.Is<int>(i => i == 100));
            doApi.Images.Received().GetAll(Requests.ImageType.Distribution);
            _sessionRepo.Received().Update(Arg.Is<int>(i => i == 100), Arg.Invoke(new Session()));
            _pageFactory.Received().GetInstance<ImagePage>().Received().GetPage(Arg.Is<int>(i => i == 100));
            _tg.SendTextMessageAsync(Arg.Is<ChatId>(i => i.Identifier == 101), Arg.Any<string>(), ParseMode.Markdown, replyMarkup: Arg.Any<InlineKeyboardMarkup>());
            _handlerCallbackRepo.Received().Update(Arg.Is<int>(i => i == 100), Arg.Invoke(new HandlerCallback()));
        }

        [Fact]
        public void NextOrBackImageTest()
        {
            _callbackQuery.Data = "NextImage;1";
            var command = Substitute.For<CreateNewDropletCommand>(_logger, _tg, _sessionRepo, _handlerCallbackRepo, _pageFactory, _digitalOceanClientFactory);
            command.Execute(_callbackQuery, _message, SessionState.WaitChooseImageDroplet);

            command.Received().Execute(_callbackQuery, _message, SessionState.WaitChooseImageDroplet);
            _pageFactory.Received().GetInstance<ImagePage>().Received().GetPage(Arg.Is<int>(i => i == 100), 1);
            _tg.SendTextMessageAsync(Arg.Is<ChatId>(i => i.Identifier == 101), Arg.Any<string>(), ParseMode.Markdown, replyMarkup: Arg.Any<InlineKeyboardMarkup>());
        }

        [Fact]
        public void SelectImageTest()
        {
            _callbackQuery.Data = "SelectImage;1";
            var command = Substitute.For<CreateNewDropletCommand>(_logger, _tg, _sessionRepo, _handlerCallbackRepo, _pageFactory, _digitalOceanClientFactory);
            command.Execute(_callbackQuery, _message, SessionState.WaitChooseImageDroplet);

            command.Received().Execute(_callbackQuery, _message, SessionState.WaitChooseImageDroplet);
            _pageFactory.Received().GetInstance<ImagePage>().Received().SelectPage(Arg.Is<int>(i => i == 100), Arg.Any<object>());
            _sessionRepo.Received().Update(Arg.Is<int>(i => i == 100), Arg.Invoke(new Session()));

            var doApi = _digitalOceanClientFactory.Received().GetInstance(Arg.Is<int>(i => i == 100));
            doApi.Regions.Received().GetAll();
            _sessionRepo.Received().Update(Arg.Is<int>(i => i == 100), Arg.Invoke(new Session()));
            _pageFactory.Received().GetInstance<RegionPage>().Received().GetPage(Arg.Is<int>(i => i == 100));
            _tg.SendTextMessageAsync(Arg.Is<ChatId>(i => i.Identifier == 101), Arg.Any<string>(), ParseMode.Markdown, replyMarkup: Arg.Any<InlineKeyboardMarkup>());
            _handlerCallbackRepo.Received().Update(Arg.Is<int>(i => i == 100), Arg.Invoke(new HandlerCallback()));
        }

        [Fact]
        public void NextOrBackRegionTest()
        {
            _callbackQuery.Data = "NextRegion;1";
            var command = Substitute.For<CreateNewDropletCommand>(_logger, _tg, _sessionRepo, _handlerCallbackRepo, _pageFactory, _digitalOceanClientFactory);
            command.Execute(_callbackQuery, _message, SessionState.WaitChooseRegionDroplet);

            command.Received().Execute(_callbackQuery, _message, SessionState.WaitChooseRegionDroplet);
            _pageFactory.Received().GetInstance<RegionPage>().Received().GetPage(Arg.Is<int>(i => i == 100), 1);
            _tg.SendTextMessageAsync(Arg.Is<ChatId>(i => i.Identifier == 101), Arg.Any<string>(), ParseMode.Markdown, replyMarkup: Arg.Any<InlineKeyboardMarkup>());
        }

        [Fact]
        public void SelectRegionTest()
        {
            _callbackQuery.Data = "SelectRegion;1";
            var command = Substitute.For<CreateNewDropletCommand>(_logger, _tg, _sessionRepo, _handlerCallbackRepo, _pageFactory, _digitalOceanClientFactory);
            command.Execute(_callbackQuery, _message, SessionState.WaitChooseRegionDroplet);

            command.Received().Execute(_callbackQuery, _message, SessionState.WaitChooseRegionDroplet);
            _pageFactory.Received().GetInstance<RegionPage>().Received().SelectPage(Arg.Is<int>(i => i == 100), Arg.Any<object>());
            _sessionRepo.Received().Update(Arg.Is<int>(i => i == 100), Arg.Invoke(new Session()));

            var doApi = _digitalOceanClientFactory.Received().GetInstance(Arg.Is<int>(i => i == 100));
            doApi.Sizes.Received().GetAll();
            _sessionRepo.Received().Update(Arg.Is<int>(i => i == 100), Arg.Invoke(new Session()));
            _pageFactory.Received().GetInstance<SizePage>().Received().GetPage(Arg.Is<int>(i => i == 100));
            _tg.SendTextMessageAsync(Arg.Is<ChatId>(i => i.Identifier == 101), Arg.Any<string>(), ParseMode.Markdown, replyMarkup: Arg.Any<InlineKeyboardMarkup>());
            _handlerCallbackRepo.Received().Update(Arg.Is<int>(i => i == 100), Arg.Invoke(new HandlerCallback()));
        }

        [Fact]
        public void NextOrBackSizeTest()
        {
            _callbackQuery.Data = "NextSize;1";
            var command = Substitute.For<CreateNewDropletCommand>(_logger, _tg, _sessionRepo, _handlerCallbackRepo, _pageFactory, _digitalOceanClientFactory);
            command.Execute(_callbackQuery, _message, SessionState.WaitChooseSizeDroplet);

            command.Received().Execute(_callbackQuery, _message, SessionState.WaitChooseSizeDroplet);
            _pageFactory.Received().GetInstance<SizePage>().Received().GetPage(Arg.Is<int>(i => i == 100), 1);
            _tg.SendTextMessageAsync(Arg.Is<ChatId>(i => i.Identifier == 101), Arg.Any<string>(), ParseMode.Markdown, replyMarkup: Arg.Any<InlineKeyboardMarkup>());
        }

        [Fact]
        public void SelectSizeTest()
        {
            _sessionRepo.Get(Arg.Any<int>()).Returns(new Session
            {
                Data = new CreateDroplet
                {
                    Droplet = new Requests.Droplet()
                }
            });

            _callbackQuery.Data = "SelectSize;1";
            var command = Substitute.For<CreateNewDropletCommand>(_logger, _tg, _sessionRepo, _handlerCallbackRepo, _pageFactory, _digitalOceanClientFactory);
            command.Execute(_callbackQuery, _message, SessionState.WaitChooseSizeDroplet);

            command.Received().Execute(_callbackQuery, _message, SessionState.WaitChooseSizeDroplet);
            _pageFactory.Received().GetInstance<SizePage>().Received().SelectPage(Arg.Is<int>(i => i == 100), Arg.Any<object>());
            _sessionRepo.Received().Update(Arg.Is<int>(i => i == 100), Arg.Invoke(new Session()));

            var doApi = _digitalOceanClientFactory.Received().GetInstance(Arg.Is<int>(i => i == 100));
            doApi.Droplets.Received().Create(Arg.Any<Requests.Droplet>());
            _tg.SendTextMessageAsync(Arg.Is<ChatId>(i => i.Identifier == 101), Arg.Any<string>(), ParseMode.Markdown, replyMarkup: Arg.Any<InlineKeyboardMarkup>());
        }

    }
}
