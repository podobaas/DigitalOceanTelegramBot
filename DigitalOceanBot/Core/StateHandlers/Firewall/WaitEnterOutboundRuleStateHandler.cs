using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DigitalOcean.API;
using DigitalOcean.API.Models.Requests;
using DigitalOceanBot.Messages;
using DigitalOceanBot.Services;
using DigitalOceanBot.Types;
using DigitalOceanBot.Types.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DigitalOceanBot.Core.StateHandlers.Firewall
{
    public class WaitEnterOutboundRuleStateHandler: IStateHandler
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly IDigitalOceanClient _digitalOceanClient;
        private readonly StorageService _storageService;

        public WaitEnterOutboundRuleStateHandler(
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
            var firewallId = _storageService.Get<string>(StorageKeys.SelectedFirewall);

            if (string.IsNullOrEmpty(firewallId))
            {
                return;
            }
            
            var outboundRules = new List<OutboundRule>();
            var invalidRules = new List<string>();
            var rules = message.Text.Split(";");
            var regExp = new Regex(RegExpPatterns.NetworkAddress);

            foreach (var rule in rules)
            {
                var resultMatch = regExp.Match(rule);
                
                if (resultMatch.Success)
                {
                    var inboundRule = new OutboundRule
                    {
                        Protocol = resultMatch.Groups[0].Value,
                        Ports = resultMatch.Groups[1].Value,
                        Destinations = new SourceLocation
                        {
                            Addresses = new List<string>
                            {
                                $"{resultMatch.Groups[2].Value}/{resultMatch.Groups[3].Value}"
                            }
                        }
                    };

                    outboundRules.Add(inboundRule);
                }
                else
                {
                    invalidRules.Add(rule);
                }
            }

            if (outboundRules.Count > 0)
            {
                await _digitalOceanClient.Firewalls.AddRules(firewallId, new FirewallRules
                {
                    OutboundRules = outboundRules
                });

                await _telegramBotClient.SendTextMessageAsync(
                    chatId:message.Chat.Id, 
                    text:FirewallMessage.GetCreatedBoundRulesMessage(outboundRules.Count), 
                    parseMode:ParseMode.Markdown);
            }

            if (invalidRules.Count > 0)
            {
                await _telegramBotClient.SendTextMessageAsync(
                    chatId:message.Chat.Id, 
                    text:FirewallMessage.GetInvalidBoundRulesMessage(invalidRules), 
                    parseMode:ParseMode.Html);
            }
            
            _storageService.AddOrUpdate(StorageKeys.BotCurrentState, StateType.None);
        }
    }
}