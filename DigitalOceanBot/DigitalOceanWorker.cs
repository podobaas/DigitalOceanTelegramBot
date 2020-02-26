using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DigitalOceanBot.BusMessage;
using DigitalOceanBot.Commands;
using DigitalOceanBot.Helpers;
using DigitalOceanBot.MongoDb;
using DigitalOceanBot.MongoDb.Models;
using EasyNetQ;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

namespace DigitalOceanBot
{
    public class DigitalOceanWorker : IHostedService
    {
        private readonly ILogger<DigitalOceanWorker> _logger;
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly IAdvancedBus _bus;
        private readonly IRepository<Session> _sessionRepo;
        private readonly IRepository<HandlerCallback> _handlerCallbackRepo;
        private readonly IServiceProvider _serviceProvider;
        private Router _botCommand;


        public DigitalOceanWorker(
            IServiceProvider serviceProvider,
            ITelegramBotClient telegramBotClient,
            IAdvancedBus bus,
            ILogger<DigitalOceanWorker> logger,
            IRepository<Session> sessionRepo,
            IRepository<HandlerCallback> handlerCallbackRepo)
        {
            _logger = logger;
            _telegramBotClient = telegramBotClient;
            _bus = bus;
            _sessionRepo = sessionRepo;
            _handlerCallbackRepo = handlerCallbackRepo;
            _serviceProvider = serviceProvider;

            LoadCommands();
            RunListeningBus();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting...");

            _telegramBotClient.OnMessage += OnMessage;
            _telegramBotClient.OnUpdate += OnUpdate;
            _telegramBotClient.StartReceiving(new[]
            {
                UpdateType.CallbackQuery,
                UpdateType.Message
            });

            _logger.LogInformation("Started");

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping...");

            _telegramBotClient.StopReceiving();
            _bus?.Dispose();

            _logger.LogInformation("Stopped");

            return Task.CompletedTask;
        }

        private async void OnMessage(object sender, MessageEventArgs e)
        {
            try
            {
                if (e.Message.Type == MessageType.Text && !e.Message.From.IsBot)
                {
                    var session = _sessionRepo.Get(e.Message.From.Id);
                    if (session != null)
                    {
                        var command = _botCommand.Commands.FirstOrDefault(c => c.Name == e.Message.Text);
                        if (command != null)
                        {
                            var controller = GetCommandOrCallback<IBotCommand>(command.Type);
                            controller?.Execute(e.Message, session.State);
                            
                        }
                        else
                        {
                            var className = _botCommand.States.FirstOrDefault(c => c.SessionStates.Contains(session.State))?.Type;
                            if (!string.IsNullOrEmpty(className))
                            {
                                var controller = GetCommandOrCallback<IBotCommand>(className);
                                await controller.Execute(e.Message, session.State);
                            }
                        }
                    }
                    else
                    {
                        var controller = GetCommandOrCallback<IBotCommand>(typeof(StartCommand).FullName);
                        await controller.Execute(e.Message, SessionState.Unknown);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"ChatId={e.Message.Chat.Id.ToString()}, ErrorMessage={ex.Message}, StackTrace={ex.StackTrace}");
            }
        }

        private async void OnUpdate(object sender, UpdateEventArgs e)
        {
            try
            {
                if (e.Update.CallbackQuery != null)
                {
                    var callBackData = e.Update.CallbackQuery.Data.Split(';');
                    if (callBackData.Length > 0 && callBackData[0] != "Empty")
                    {
                        var handlerCallback = _handlerCallbackRepo.Get(e.Update.CallbackQuery.From.Id);
                        if (handlerCallback != null && handlerCallback.MessageId == e.Update.CallbackQuery.Message.MessageId)
                        {
                            var callback = GetCommandOrCallback<IBotCallback>(handlerCallback.HandlerType);
                            var session = _sessionRepo.Get(e.Update.CallbackQuery.From.Id);
                            
                            if (callback != null && session != null)
                            {
                                await _telegramBotClient.AnswerCallbackQueryAsync(e.Update.CallbackQuery.Id);
                                await callback.Execute(e.Update.CallbackQuery, e.Update.CallbackQuery.Message, session.State);
                            }
                        }
                    }
                    else
                    {
                        await _telegramBotClient.AnswerCallbackQueryAsync(e.Update.CallbackQuery.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"UserId={e.Update.Message.From.Id.ToString()},ErrorMessage={ex.Message}, StackTrace={ex.StackTrace}");
            }
        }

        private void RunListeningBus()
        {
            _bus.Consume(_bus.QueueDeclare("auth-queue"), register =>
            {
                register.Add<AuthMessage>(async (message, info) =>
                {
                    try
                    {
                        if (message.Body.IsSuccess)
                        {
                            _sessionRepo.Update(message.Body.UserId, (session) =>
                            {
                                session.State = SessionState.MainMenu;
                            });

                            await _telegramBotClient.SendTextMessageAsync(message.Body.ChatId, "Authentication completed \U0001F973", replyMarkup: Keyboards.GetMainMenuKeyboard());
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"UserId={message.Body.UserId.ToString()}, ErrorMessage={ex.Message}, StackTrace={ex.StackTrace}");
                    }
                });
            });
        }

        private T GetCommandOrCallback<T>(string className) where T : class
        {
            var type = Type.GetType(className);
            return (T)ActivatorUtilities.CreateInstance(_serviceProvider, type);
        }

        private void LoadCommands()
        {
            var json = File.ReadAllText($"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}//route.json");
            _botCommand = JsonConvert.DeserializeObject<Router>(json);
        }
    }
}
