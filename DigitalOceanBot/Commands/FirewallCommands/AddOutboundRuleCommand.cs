using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DigitalOcean.API.Exceptions;
using DigitalOcean.API.Models.Requests;
using DigitalOceanBot.Factory;
using DigitalOceanBot.MongoDb;
using DigitalOceanBot.MongoDb.Models;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DigitalOceanBot.Commands.FirewallCommands
{
    public class AddOutboundRuleCommand : IBotCommand
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly IRepository<Session> _sessionRepo;
        private readonly ILogger<DigitalOceanWorker> _logger;
        private readonly IDigitalOceanClientFactory _digitalOceanClientFactory;

        public AddOutboundRuleCommand(
            ILogger<DigitalOceanWorker> logger,
            ITelegramBotClient telegramBotClient,
            IRepository<Session> sessionRepo,
            IDigitalOceanClientFactory digitalOceanClientFactory)
        {
            _logger = logger;
            _telegramBotClient = telegramBotClient;
            _sessionRepo = sessionRepo;
            _digitalOceanClientFactory = digitalOceanClientFactory;
        }


        #region Commands

        public async Task Execute(Message message, SessionState sessionState)
        {
            try
            {
                await _telegramBotClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                switch (sessionState)
                {
                    case SessionState.SelectedFirewall:
                        await InputOutboundRule(message).ConfigureAwait(false);
                        break;
                    case SessionState.WaitInputAddInboundRuleFirewall:
                        await AddOutboundRule(message).ConfigureAwait(false);
                        break;

                }
            }
            catch (ApiException ex)
            {
                _logger.LogError($"UserId={message.From.Id.ToString()}, Error={ex.Message}");
                await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, $"DigitalOcean API Error: {ex.Message.Replace(".", "\\.")}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"UserId={message.From.Id.ToString()}, Error={ex.Message}, StackTrace={ex.StackTrace}");
                await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Sorry, аn error has occurred \U0001F628");
            }
        }

        private async Task InputOutboundRule(Message message)
        {
            _sessionRepo.Update(message.From.Id, session =>
            {
                session.State = SessionState.WaitInputAddOutboundRuleFirewall;
            });

            await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Input an outbound rule in format:\n*tcp or udp or icmp:port or range:addresses*\n\nFor example:\n*tcp:80:0.0.0.0/0;icmp:8000-9000:0.0.0.0/0;udp:421:1.1.1.1,0.0.0.0/0", ParseMode.Markdown);
        }

        private async Task AddOutboundRule(Message message)
        {
            await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "\U0001F4C0 Adding outbound rule...");

            var session = _sessionRepo.Get(message.From.Id);
            var outboundRules = new List<OutboundRule>();
            var userRules = message.Text.Split(';');
            foreach (var rule in userRules)
            {
                var ruleData = rule.Split(':');
                if (ruleData.Length == 3)
                {
                    var inboundRule = new OutboundRule
                    {
                        Protocol = ruleData[0],
                        Ports = ruleData[1],
                        Destinations = new SourceLocation
                        {
                            Addresses = ruleData[2].Split(',').ToList()
                        }
                    };

                    outboundRules.Add(inboundRule);
                }
                else
                {
                    await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, $"Invalid rule: {rule.Replace(".", "\\.")}\nPlease, try again", ParseMode.MarkdownV2);
                    return;
                }
            }

            var digitalOceanApi = _digitalOceanClientFactory.GetInstance(message.From.Id);
            await digitalOceanApi.Firewalls.AddRules((string)session.Data, new FirewallRules
            {
                OutboundRules = outboundRules
            });

            await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, $"Done \U00002705", ParseMode.MarkdownV2);
        }

        #endregion
    }
}
