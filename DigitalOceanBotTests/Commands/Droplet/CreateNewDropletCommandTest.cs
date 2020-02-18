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

namespace DigitalOceanBotTests
{
    public class CreateNewDropletCommandTest
    {
        ILogger<DigitalOceanWorker> logger;
        ITelegramBotClient tg;
        IRepository<DoUser> userRepo;
        IRepository<Session> sessionRepo;
        IRepository<HandlerCallback> handlerCallbakRepo;
        IPageFactory pageFactory;
        IDigitalOceanClientFactory digitalOceanClientFactory;
        Message message;
        CallbackQuery callbackQuery;

        public CreateNewDropletCommandTest()
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
            callbackQuery = Substitute.For<CallbackQuery>();

            message.From = new User { Id = 100 };
            message.Chat = new Chat { Id = 101 };

            callbackQuery.From = message.From;
            callbackQuery.Message = message;

            digitalOceanClientFactory.GetInstance(Arg.Any<int>())
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

            digitalOceanClientFactory.GetInstance(Arg.Any<int>())
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

            digitalOceanClientFactory.GetInstance(Arg.Any<int>())
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

            pageFactory.GetInstance<IPage>()
                .GetPage(Arg.Any<int>(), Arg.Any<int>())
                .Returns(new PageModel
                {
                    Message = "Test message",
                    Keyboard = new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>())
                });

            pageFactory.GetInstance<IPage>()
                .SelectPage(Arg.Any<int>(), Arg.Any<object>())
                .Returns(new PageModel
                {
                    Message = "Test message",
                    Keyboard = new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>())
                });

            userRepo.Get(Arg.Any<int>())
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
            var command = Substitute.For<CreateNewDropletCommand>(logger, tg, sessionRepo, handlerCallbakRepo, pageFactory, digitalOceanClientFactory);
            command.Execute(message, SessionState.DropletsMenu);
            
            command.Received().Execute(message, SessionState.DropletsMenu);
            sessionRepo.Received().Update(Arg.Is<int>(i => i == 100), Arg.Invoke(new Session()));
            tg.SendTextMessageAsync(Arg.Is<ChatId>(i => i.Identifier == 101), Arg.Any<string>());
        }

        [Fact]
        public void ChooseImageTest()
        {
            var command = Substitute.For<CreateNewDropletCommand>(logger, tg, sessionRepo, handlerCallbakRepo, pageFactory, digitalOceanClientFactory);
            command.Execute(message, SessionState.WaitInputNameDroplet);

            command.Received().Execute(message, SessionState.WaitInputNameDroplet);
            var doApi = digitalOceanClientFactory.Received().GetInstance(Arg.Is<int>(i => i == 100));
            doApi.Images.Received().GetAll(Requests.ImageType.Distribution);
            sessionRepo.Received().Update(Arg.Is<int>(i => i == 100), Arg.Invoke(new Session()));
            pageFactory.Received().GetInstance<ImagePage>().Received().GetPage(Arg.Is<int>(i => i == 100));
            tg.SendTextMessageAsync(Arg.Is<ChatId>(i => i.Identifier == 101), Arg.Any<string>(), ParseMode.Markdown, replyMarkup: Arg.Any<InlineKeyboardMarkup>());
            handlerCallbakRepo.Received().Update(Arg.Is<int>(i => i == 100), Arg.Invoke(new HandlerCallback()));
        }

        [Fact]
        public void NextOrBackImageTest()
        {
            callbackQuery.Data = "NextImage;1";
            var command = Substitute.For<CreateNewDropletCommand>(logger, tg, sessionRepo, handlerCallbakRepo, pageFactory, digitalOceanClientFactory);
            command.Execute(callbackQuery, message, SessionState.WaitChooseImageDroplet);

            command.Received().Execute(callbackQuery, message, SessionState.WaitChooseImageDroplet);
            pageFactory.Received().GetInstance<ImagePage>().Received().GetPage(Arg.Is<int>(i => i == 100), 1);
            tg.SendTextMessageAsync(Arg.Is<ChatId>(i => i.Identifier == 101), Arg.Any<string>(), ParseMode.Markdown, replyMarkup: Arg.Any<InlineKeyboardMarkup>());
        }

        [Fact]
        public void SelectImageTest()
        {
            callbackQuery.Data = "SelectImage;1";
            var command = Substitute.For<CreateNewDropletCommand>(logger, tg, sessionRepo, handlerCallbakRepo, pageFactory, digitalOceanClientFactory);
            command.Execute(callbackQuery, message, SessionState.WaitChooseImageDroplet);

            command.Received().Execute(callbackQuery, message, SessionState.WaitChooseImageDroplet);
            pageFactory.Received().GetInstance<ImagePage>().Received().SelectPage(Arg.Is<int>(i => i == 100), Arg.Any<object>());
            sessionRepo.Received().Update(Arg.Is<int>(i => i == 100), Arg.Invoke(new Session()));

            var doApi = digitalOceanClientFactory.Received().GetInstance(Arg.Is<int>(i => i == 100));
            doApi.Regions.Received().GetAll();
            sessionRepo.Received().Update(Arg.Is<int>(i => i == 100), Arg.Invoke(new Session()));
            pageFactory.Received().GetInstance<RegionPage>().Received().GetPage(Arg.Is<int>(i => i == 100));
            tg.SendTextMessageAsync(Arg.Is<ChatId>(i => i.Identifier == 101), Arg.Any<string>(), ParseMode.Markdown, replyMarkup: Arg.Any<InlineKeyboardMarkup>());
            handlerCallbakRepo.Received().Update(Arg.Is<int>(i => i == 100), Arg.Invoke(new HandlerCallback()));
        }

        [Fact]
        public void NextOrBackRegionTest()
        {
            callbackQuery.Data = "NextRegion;1";
            var command = Substitute.For<CreateNewDropletCommand>(logger, tg, sessionRepo, handlerCallbakRepo, pageFactory, digitalOceanClientFactory);
            command.Execute(callbackQuery, message, SessionState.WaitChooseRegionDroplet);

            command.Received().Execute(callbackQuery, message, SessionState.WaitChooseRegionDroplet);
            pageFactory.Received().GetInstance<RegionPage>().Received().GetPage(Arg.Is<int>(i => i == 100), 1);
            tg.SendTextMessageAsync(Arg.Is<ChatId>(i => i.Identifier == 101), Arg.Any<string>(), ParseMode.Markdown, replyMarkup: Arg.Any<InlineKeyboardMarkup>());
        }

        [Fact]
        public void SelectRegionTest()
        {
            callbackQuery.Data = "SelectRegion;1";
            var command = Substitute.For<CreateNewDropletCommand>(logger, tg, sessionRepo, handlerCallbakRepo, pageFactory, digitalOceanClientFactory);
            command.Execute(callbackQuery, message, SessionState.WaitChooseRegionDroplet);

            command.Received().Execute(callbackQuery, message, SessionState.WaitChooseRegionDroplet);
            pageFactory.Received().GetInstance<RegionPage>().Received().SelectPage(Arg.Is<int>(i => i == 100), Arg.Any<object>());
            sessionRepo.Received().Update(Arg.Is<int>(i => i == 100), Arg.Invoke(new Session()));

            var doApi = digitalOceanClientFactory.Received().GetInstance(Arg.Is<int>(i => i == 100));
            doApi.Sizes.Received().GetAll();
            sessionRepo.Received().Update(Arg.Is<int>(i => i == 100), Arg.Invoke(new Session()));
            pageFactory.Received().GetInstance<SizePage>().Received().GetPage(Arg.Is<int>(i => i == 100));
            tg.SendTextMessageAsync(Arg.Is<ChatId>(i => i.Identifier == 101), Arg.Any<string>(), ParseMode.Markdown, replyMarkup: Arg.Any<InlineKeyboardMarkup>());
            handlerCallbakRepo.Received().Update(Arg.Is<int>(i => i == 100), Arg.Invoke(new HandlerCallback()));
        }

        [Fact]
        public void NextOrBackSizeTest()
        {
            callbackQuery.Data = "NextSize;1";
            var command = Substitute.For<CreateNewDropletCommand>(logger, tg, sessionRepo, handlerCallbakRepo, pageFactory, digitalOceanClientFactory);
            command.Execute(callbackQuery, message, SessionState.WaitChooseSizeDroplet);

            command.Received().Execute(callbackQuery, message, SessionState.WaitChooseSizeDroplet);
            pageFactory.Received().GetInstance<SizePage>().Received().GetPage(Arg.Is<int>(i => i == 100), 1);
            tg.SendTextMessageAsync(Arg.Is<ChatId>(i => i.Identifier == 101), Arg.Any<string>(), ParseMode.Markdown, replyMarkup: Arg.Any<InlineKeyboardMarkup>());
        }

        [Fact]
        public void SelectSizeTest()
        {
            sessionRepo.Get(Arg.Any<int>()).Returns(new Session
            {
                Data = new CreateDroplet
                {
                    Droplet = new Requests.Droplet()
                }
            });

            callbackQuery.Data = "SelectSize;1";
            var command = Substitute.For<CreateNewDropletCommand>(logger, tg, sessionRepo, handlerCallbakRepo, pageFactory, digitalOceanClientFactory);
            command.Execute(callbackQuery, message, SessionState.WaitChooseSizeDroplet);

            command.Received().Execute(callbackQuery, message, SessionState.WaitChooseSizeDroplet);
            pageFactory.Received().GetInstance<SizePage>().Received().SelectPage(Arg.Is<int>(i => i == 100), Arg.Any<object>());
            sessionRepo.Received().Update(Arg.Is<int>(i => i == 100), Arg.Invoke(new Session()));

            var doApi = digitalOceanClientFactory.Received().GetInstance(Arg.Is<int>(i => i == 100));
            doApi.Droplets.Received().Create(Arg.Any<Requests.Droplet>());
            tg.SendTextMessageAsync(Arg.Is<ChatId>(i => i.Identifier == 101), Arg.Any<string>(), ParseMode.Markdown, replyMarkup: Arg.Any<InlineKeyboardMarkup>());
        }

    }
}
