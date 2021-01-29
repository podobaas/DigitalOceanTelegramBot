using System;
using System.Threading;
using System.Threading.Tasks;
using DigitalOceanBot.Core;
using DigitalOceanBot.Core.CallbackQueries.Droplet;
using DigitalOceanBot.Core.CallbackQueries.Firewall;
using DigitalOceanBot.Core.Commands.Account;
using DigitalOceanBot.Core.Commands.Droplet;
using DigitalOceanBot.Core.Commands.Firewall;
using DigitalOceanBot.Core.Commands.Main;
using DigitalOceanBot.Core.StateHandlers.Droplet;
using DigitalOceanBot.Core.StateHandlers.Firewall;
using DigitalOceanBot.Types.Enums;
using Microsoft.Extensions.Hosting;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

namespace DigitalOceanBot
{
    public class DigitalOceanWorker : IHostedService
    {
        private readonly BotCommandManager _botCommandManager;
        private readonly ILogger _logger;
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly int _userId;


        public DigitalOceanWorker(BotCommandManager botCommandManager, ITelegramBotClient telegramBotClient, ILogger logger)
        {
            _botCommandManager = botCommandManager;
            _logger = logger;
            _telegramBotClient = telegramBotClient;
            _userId = int.Parse(Environment.GetEnvironmentVariable("USER_ID"));
            
            RegisterCommands();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.Information("Starting bot...");
            
            _telegramBotClient.OnMessage += OnMessage;
            _telegramBotClient.OnCallbackQuery += OnCallbackQuery;
            _telegramBotClient.StartReceiving(new[] {UpdateType.CallbackQuery, UpdateType.Message}, cancellationToken);

            // await _telegramBotClient.SendTextMessageAsync(_userId, 
            //     MessageHelper.GetMainMenuMessage(), 
            //     replyMarkup: KeyboardHelper.GetMainMenuKeyboard(), 
            //     cancellationToken: cancellationToken);
            
            _logger.Information("Bot started successfully!");
        }
        
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.Information("Stopping bot...");

            _telegramBotClient?.StopReceiving();

            _logger.Information("Bot stopped!");

            return Task.CompletedTask;
        }

        private async void OnMessage(object sender, MessageEventArgs e)
        {
            if (e.Message.Type == MessageType.Text && e.Message.From.Id == _userId)
            {
                await _botCommandManager.StartCommandAsync(e.Message);
            }
        }

        private async void OnCallbackQuery(object sender, CallbackQueryEventArgs e)
        {
            if (e.CallbackQuery.From.Id == _userId)
            {
                await _botCommandManager.StartCallbackQueryAsync(e.CallbackQuery.Message.Chat.Id, e.CallbackQuery.Message.MessageId, e.CallbackQuery.Id, e.CallbackQuery.Data);
            }
        }
        
        private void RegisterCommands()
        {
            #region Main

            _botCommandManager
                .OnCommand(CommandType.Start, typeof(StartCommand))
                .OnCommand(CommandType.Back, typeof(BackCommand));

            #endregion
            
            #region Account

            _botCommandManager.OnCommand(CommandType.Account, typeof(GetAccountCommand));

            #endregion

            #region Droplet

            _botCommandManager
                .OnCommand(CommandType.Droplets, typeof(GetDropletsCommand))
                .OnCommand(CommandType.DropletReboot, typeof(RebootCommand))
                .OnCommand(CommandType.DropletRename, typeof(RenameCommand))
                .OnCommand(CommandType.DropletShutdown, typeof(ShutdownCommand))
                .OnCommand(CommandType.DropletPowerCycle, typeof(PowerCycleCommand))
                .OnCommand(CommandType.DropletPowerOn, typeof(PowerOnCommand))
                .OnCommand(CommandType.DropletResetPassword, typeof(ResetPasswordCommand))
                .OnCommand(CommandType.DropletCreateSnapshot, typeof(SnapshotCommand))
                .OnState(StateType.DropletWaitEnterNewName, typeof(WaitEnterNewNameDropletStateHandler))
                .OnState(StateType.DropletWaitEnterSnapshotName, typeof(WaitEnterSnapshotNameStateHandler))
                .OnCallbackQuery(CallbackQueryType.DropletNext, typeof(PreviousAndNextDropletCallbackQuery))
                .OnCallbackQuery(CallbackQueryType.DropletPrevious, typeof(PreviousAndNextDropletCallbackQuery))
                .OnCallbackQuery(CallbackQueryType.DropletSelect, typeof(SelectDropletCallbackQuery));

            #endregion

            #region Firewall

            _botCommandManager
                .OnCommand(CommandType.Firewalls, typeof(GetFirewallsCommand))
                .OnCommand(CommandType.FirewallAddInboundRule, typeof(AddInboundRuleCommand))
                .OnCommand(CommandType.FirewallAddOutboundRule, typeof(AddOutboundRuleCommand))
                .OnCommand(CommandType.FirewallCreateNew, typeof(CreateFirewallCommand))
                .OnState(StateType.FirewallWaitEnterInboundRule, typeof(WaitEnterInboundRuleStateHandler))
                .OnState(StateType.FirewallWaitEnterOutboundRule, typeof(WaitEnterOutboundRuleStateHandler))
                .OnState(StateType.FirewallWaitEnterCreationData, typeof(WaitEnterCreationDataFirewallStateHandler))
                .OnCallbackQuery(CallbackQueryType.FirewallNext, typeof(PreviousAndNextFirewallCallbackQuery))
                .OnCallbackQuery(CallbackQueryType.FirewallPrevious, typeof(PreviousAndNextFirewallCallbackQuery))
                .OnCallbackQuery(CallbackQueryType.FirewallSelect, typeof(SelectFirewallCallbackQuery));

            #endregion
        }
    }
}
