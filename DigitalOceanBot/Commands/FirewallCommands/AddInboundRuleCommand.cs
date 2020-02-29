using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DigitalOcean.API.Exceptions;
using DigitalOcean.API.Models.Requests;
using DigitalOceanBot.Factory;
using DigitalOceanBot.Helpers;
using DigitalOceanBot.MongoDb;
using DigitalOceanBot.MongoDb.Models;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DigitalOceanBot.Commands.FirewallCommands
{
    public class AddInboundRuleCommand : IBotCommand
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly IRepository<Session> _sessionRepo;
        private readonly ILogger<DigitalOceanWorker> _logger;
        private readonly IDigitalOceanClientFactory _digitalOceanClientFactory;

        public AddInboundRuleCommand(
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
        
        public async Task Execute(Message message, SessionState sessionState)
        {
            try
            {
                if (sessionState == SessionState.SelectedFirewall)
                {
                    await InputInboundRule(message).ConfigureAwait(false);
                }
                else if (sessionState == SessionState.WaitInputAddInboundRuleFirewall)
                {
                    await AddInboundRule(message).ConfigureAwait(false);
                }
            }
            catch (ApiException ex)
            {
                _logger.LogError($"UserId={message.From.Id.ToString()}, Error={ex.Message}");
                await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, $"DigitalOcean API Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"UserId={message.From.Id.ToString()}, Error={ex.Message}, StackTrace={ex.StackTrace}");
                await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Sorry, аn error has occurred \U0001F628");
            }
        }

        private async Task InputInboundRule(Message message)
        {
            _sessionRepo.Update(message.From.Id, session =>
            {
                session.State = SessionState.WaitInputAddInboundRuleFirewall;
            });

            await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Input an inbound rule in format:\n*tcp or udp or icmp:port or range:addresses*\n\nFor example:\n*tcp:80:0.0.0.0/0;icmp:8000-9000:0.0.0.0/0;udp:421:1.1.1.1,0.0.0.0/0*", ParseMode.Markdown);
        }

        private async Task AddInboundRule(Message message)
        {
            await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "\U0001F4C0 Adding inbound rule...", replyMarkup: Keyboards.GetFirewallMenuKeyboard());

            var session = _sessionRepo.Get(message.From.Id);
            var inboundRules = new List<InboundRule>();
            var userRules = message.Text.Split(';');
            foreach (var rule in userRules)
            {
                var ruleData = rule.Split(':');
                if (ruleData.Length == 3)
                {
                    var inboundRule = new InboundRule
                    {
                        Protocol = ruleData[0],
                        Ports = ruleData[1],
                        Sources = new SourceLocation
                        {
                            Addresses = ruleData[2].Split(',').ToList()
                        }
                    };

                    inboundRules.Add(inboundRule);
                }
                else
                {
                    await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, $"Invalid rule: {rule.Replace(".", "\\.")}\nPlease, try again");
                    return;
                }
            }

            var digitalOceanApi = _digitalOceanClientFactory.GetInstance(message.From.Id);
            await digitalOceanApi.Firewalls.AddRules((string)session.Data, new FirewallRules
            {
                InboundRules = inboundRules
            });

            await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, $"Done \U00002705");
        }
    }
}
