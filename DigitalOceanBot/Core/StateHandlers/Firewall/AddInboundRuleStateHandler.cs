using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DigitalOcean.API;
using DigitalOcean.API.Models.Requests;
using DigitalOceanBot.Messages;
using DigitalOceanBot.Services;
using DigitalOceanBot.Types;
using DigitalOceanBot.Types.Attributes;
using DigitalOceanBot.Types.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DigitalOceanBot.Core.StateHandlers.Firewall
{
    [BotStateHandler(BotStateType.FirewallUpdateWaitingEnterInboundRule)]
    public sealed class AddInboundRuleStateHandler: IBotStateHandler
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly IDigitalOceanClient _digitalOceanClient;
        private readonly StorageService _storageService;

        public AddInboundRuleStateHandler(
            ITelegramBotClient telegramBotClient,
            IDigitalOceanClient digitalOceanClient,
            StorageService storageService)
        {
            _telegramBotClient = telegramBotClient;
            _digitalOceanClient = digitalOceanClient;
            _storageService = storageService;
        }
        

        public async Task ExecuteHandlerAsync(Message message)
        {
            var firewallId = _storageService.Get<string>(StorageKeys.FirewallId);

            if (string.IsNullOrEmpty(firewallId))
            {
                return;
            }
            
            var inboundRules = new List<InboundRule>();
            var invalidRules = new List<string>();
            var rules = message.Text.Split(";");
            var regExp = new Regex(RegExpPatterns.NetworkAddress);

            foreach (var rule in rules)
            {
                var resultMatch = regExp.Match(rule);
                
                if (resultMatch.Success)
                {
                    var inboundRule = new InboundRule
                    {
                        Protocol = resultMatch.Groups[1].Value,
                        Ports = resultMatch.Groups[2].Value,
                        Sources = new SourceLocation
                        {
                            Addresses = new List<string>
                            {
                                $"{resultMatch.Groups[3].Value}/{resultMatch.Groups[4].Value}"
                            }
                        }
                    };

                    inboundRules.Add(inboundRule);
                }
                else
                {
                    invalidRules.Add(rule);
                }
            }

            if (inboundRules.Count > 0)
            {
                await _digitalOceanClient.Firewalls.AddRules(firewallId, new FirewallRules
                {
                    InboundRules = inboundRules
                });

                await _telegramBotClient.SendTextMessageAsync(
                    chatId:message.Chat.Id, 
                    text:FirewallMessage.GetCreatedBoundRulesMessage(inboundRules.Count), 
                    parseMode:ParseMode.Markdown);
            }

            if (invalidRules.Count > 0)
            {
                await _telegramBotClient.SendTextMessageAsync(
                    chatId:message.Chat.Id, 
                    text:FirewallMessage.GetInvalidBoundRulesMessage(invalidRules), 
                    parseMode:ParseMode.Html);
            }
            
            _storageService.AddOrUpdate(StorageKeys.BotCurrentState, BotStateType.None);
        }
    }
}