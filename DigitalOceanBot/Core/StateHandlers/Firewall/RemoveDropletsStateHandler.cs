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
using DropletResponse = DigitalOcean.API.Models.Responses.Droplet;

namespace DigitalOceanBot.Core.StateHandlers.Firewall
{
    [BotStateHandler(BotStateType.FirewallUpdateWaitingEnterRemoveDroplet)]
    public sealed class RemoveDropletsStateHandler: IBotStateHandler
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly IDigitalOceanClient _digitalOceanClient;
        private readonly StorageService _storageService;

        public RemoveDropletsStateHandler(
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

                await _digitalOceanClient.Firewalls.RemoveDroplets(firewallId, new FirewallDroplets
                {
                    DropletIds = new List<long> {droplet.Id}
                });

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
                var dropletIds = new List<long>();
                var droplets = _storageService.Get<IEnumerable<DropletResponse>>(StorageKeys.Droplets)
                    .OrderBy(x => x.CreatedAt);

                foreach (var index in indexes)
                {
                    if (int.Parse(index) <= 0)
                    {
                        continue;
                    }
                    
                    var droplet = droplets.ElementAt(int.Parse(index) - 1);
                    dropletIds.Add(droplet.Id);
                }
                
                await _digitalOceanClient.Firewalls.RemoveDroplets(firewallId, new FirewallDroplets
                {
                    DropletIds = dropletIds
                });

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
                
                var dropletIds = new List<long>();
                var droplets = _storageService.Get<IEnumerable<DropletResponse>>(StorageKeys.Droplets)
                    .OrderBy(x => x.CreatedAt);

                for (var i = firstIndex - 1; i <= lastIndex - 1; i++)
                {
                    var droplet = droplets.ElementAt(i);
                    dropletIds.Add(droplet.Id);
                }

                await _digitalOceanClient.Firewalls.RemoveDroplets(firewallId, new FirewallDroplets
                {
                    DropletIds = dropletIds
                });

                await _telegramBotClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: CommonMessage.GetDoneMessage());
            }
        }
    }
}