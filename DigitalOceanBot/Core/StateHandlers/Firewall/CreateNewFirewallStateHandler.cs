using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DigitalOcean.API;
using DigitalOcean.API.Models.Requests;
using DigitalOceanBot.Core.Attributes;
using DigitalOceanBot.Messages;
using DigitalOceanBot.Services;
using DigitalOceanBot.Types;
using DigitalOceanBot.Types.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using FirewallRequest = DigitalOcean.API.Models.Requests.Firewall;
using DropletResponse = DigitalOcean.API.Models.Responses.Droplet;

namespace DigitalOceanBot.Core.StateHandlers.Firewall
{
    [BotStateHandler(BotStateType.FirewallCreateWaitingEnterName)]
    [BotStateHandler(BotStateType.FirewallCreateWaitingEnterInboundRule)]
    [BotStateHandler(BotStateType.FirewallCreateWaitingEnterAddDroplet)]
    public sealed class CreateNewFirewallStateHandler: IBotStateHandler
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly IDigitalOceanClient _digitalOceanClient;
        private readonly StorageService _storageService;

        public CreateNewFirewallStateHandler(
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
            var state = _storageService.Get<BotStateType>(StorageKeys.BotCurrentState);
            
            switch (state)
            {
                case BotStateType.FirewallCreateWaitingEnterName:
                    await EnterName(message);
                    break;
                case BotStateType.FirewallCreateWaitingEnterInboundRule:
                    await EnterInboundRule(message);
                    break;
                case BotStateType.FirewallCreateWaitingEnterAddDroplet:
                    await EnterDroplets(message);
                    _storageService.Remove(StorageKeys.Droplets);
                    _storageService.Remove(StorageKeys.NewFirewall);
                    _storageService.AddOrUpdate(StorageKeys.BotCurrentState, BotStateType.None);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state));
            }
        }

        private async Task EnterName(Message message)
        {
            var newFirewall = new FirewallRequest
            {
                Name = message.Text
            };

            _storageService.AddOrUpdate(StorageKeys.NewFirewall, newFirewall);
            _storageService.AddOrUpdate(StorageKeys.BotCurrentState, BotStateType.FirewallCreateWaitingEnterInboundRule);
            
            await _telegramBotClient.SendTextMessageAsync(
                chatId:message.Chat.Id, 
                text:FirewallMessage.GetEnterBoundRuleMessage("inbound"), 
                parseMode:ParseMode.Html);
        }

        private async Task EnterInboundRule(Message message)
        {
            var firewall = _storageService.Get<FirewallRequest>(StorageKeys.NewFirewall);
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

            if (inboundRules.Count > 0 && invalidRules.Count == 0)
            {
                var droplets = await _digitalOceanClient.Droplets.GetAll();

                if (droplets is not null and {Count: > 0})
                {
                    firewall.InboundRules = inboundRules;
                    
                    _storageService.AddOrUpdate(StorageKeys.Droplets, droplets.OrderBy(x => x.CreatedAt));
                    _storageService.AddOrUpdate(StorageKeys.BotCurrentState, BotStateType.FirewallCreateWaitingEnterAddDroplet);

                    await _telegramBotClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: FirewallMessage.GetDropletsListMessage(droplets),
                        parseMode: ParseMode.Markdown);
                }
                else
                {
                    await _telegramBotClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: CommonMessage.GetDoneMessage());
                }
            }
            else
            {
                await _telegramBotClient.SendTextMessageAsync(
                    chatId:message.Chat.Id, 
                    text:FirewallMessage.GetInvalidBoundRulesMessage(invalidRules), 
                    parseMode:ParseMode.Html);
            }
        }

        private async Task EnterDroplets(Message message)
        {
            var firewall = _storageService.Get<FirewallRequest>(StorageKeys.NewFirewall);
            firewall.DropletIds = new List<long>();
            
            var isNumber = int.TryParse(message.Text, out var id);

            if (isNumber)
            {
                if (id <= 0)
                {
                    await _telegramBotClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: CommonMessage.GetInvalidIndexMessage());
                    
                    return;
                }
                
                var droplet = _storageService.Get<IEnumerable<DropletResponse>>(StorageKeys.Droplets)
                    .OrderBy(x => x.CreatedAt)
                    .ElementAt(id - 1);

                firewall.DropletIds.Add(droplet.Id);

                await _digitalOceanClient.Firewalls.Create(firewall);
                
                await _telegramBotClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: CommonMessage.GetDoneMessage());
                
                return;
            }
            
            var regExp = new Regex(RegExpPatterns.NumbersSeparatedByCommas);
            var result = regExp.Match(message.Text);

            if (result.Success)
            {
                var indexes = message.Text.Split(",");
                var droplets = _storageService.Get<IEnumerable<DropletResponse>>(StorageKeys.Droplets)
                    .OrderBy(x => x.CreatedAt);

                foreach (var index in indexes)
                {
                    if (int.Parse(index) <= 0)
                    {
                        continue;
                    }
                    
                    var droplet = droplets.ElementAt(int.Parse(index) - 1);
                    firewall.DropletIds.Add(droplet.Id);
                }

                await _digitalOceanClient.Firewalls.Create(firewall);

                await _telegramBotClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: CommonMessage.GetDoneMessage());
                
                return;
            }
            
            regExp = new Regex(RegExpPatterns.NumbersSeparatedByDash);
            result = regExp.Match(message.Text);
             
            if (result.Success)
            {
                var indexes = message.Text.Split("-");
                var firstIndex = int.Parse(indexes[0]);
                var lastIndex = int.Parse(indexes[1]);

                if (firstIndex <= 0 || lastIndex <= 0)
                {
                    await _telegramBotClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: CommonMessage.GetInvalidIndexMessage());
                    
                    return;
                }
                
                var droplets = _storageService.Get<IEnumerable<DropletResponse>>(StorageKeys.Droplets)
                    .OrderBy(x => x.CreatedAt);

                for (var i = firstIndex - 1; i <= lastIndex - 1; i++)
                {
                    var droplet = droplets.ElementAt(i);
                    firewall.DropletIds.Add(droplet.Id);
                }

                await _digitalOceanClient.Firewalls.Create(firewall);

                await _telegramBotClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: CommonMessage.GetDoneMessage());
            }
        }
    }
}