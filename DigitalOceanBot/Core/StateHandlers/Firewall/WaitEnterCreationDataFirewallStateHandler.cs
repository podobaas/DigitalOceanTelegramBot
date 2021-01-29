using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DigitalOcean.API;
using DigitalOcean.API.Models.Requests;
using DigitalOceanBot.Keyboards;
using DigitalOceanBot.Messages;
using DigitalOceanBot.Services;
using DigitalOceanBot.Types;
using DigitalOceanBot.Types.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using DoFirewall = DigitalOcean.API.Models.Requests.Firewall;

namespace DigitalOceanBot.Core.StateHandlers.Firewall
{
    public class WaitEnterCreationDataFirewallStateHandler: IStateHandler
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly IDigitalOceanClient _digitalOceanClient;
        private readonly StorageService _storageService;

        public WaitEnterCreationDataFirewallStateHandler(
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
            var data = message.Text.Split(";");

            if (data.Length != 2)
            {
                await _telegramBotClient.SendTextMessageAsync(
                    chatId:message.Chat.Id, 
                    text:FirewallMessage.GetInvalidCreationDataFirewallMessage(), 
                    parseMode:ParseMode.Html);
                
                return;
            }
            
            var regExp = new Regex(RegExpPatterns.NetworkAddress);
            var result = regExp.Match(data[1]);

            if (!result.Success && result.Groups.Count < 5)
            {
                await _telegramBotClient.SendTextMessageAsync(
                    chatId:message.Chat.Id, 
                    text:FirewallMessage.GetInvalidBoundRulesMessage(new []{data[1]}), 
                    parseMode:ParseMode.Html);
                
                return;
            }
            
            var firewall = await _digitalOceanClient.Firewalls.Create(new DoFirewall
            {
                Name = data[0],
                InboundRules = new List<InboundRule>
                {
                    new()
                    {
                        Protocol = result.Groups[1].Value,
                        Ports = result.Groups[2].Value,
                        Sources = new SourceLocation
                        {
                            Addresses = new List<string>
                            {
                                $"{result.Groups[3].Value}/{result.Groups[4].Value}"
                            }
                        }
                    }
                }
            });
            
            _storageService.AddOrUpdate(StorageKeys.SelectedFirewall, firewall.Id);
            _storageService.AddOrUpdate(StorageKeys.BotCurrentState, StateType.None);

            await _telegramBotClient.SendTextMessageAsync(
                chatId:message.Chat.Id, 
                text:FirewallMessage.GetSelectedFirewallMessage(firewall), 
                parseMode:ParseMode.Markdown, 
                replyMarkup: FirewallKeyboard.GetFirewallOperationsKeyboard());
        }
    }
}