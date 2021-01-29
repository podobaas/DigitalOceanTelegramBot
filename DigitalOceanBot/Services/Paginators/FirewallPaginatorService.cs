using System;
using System.Collections.Generic;
using System.Linq;
using DigitalOcean.API.Models.Responses;
using DigitalOceanBot.Keyboards;
using DigitalOceanBot.Messages;
using DigitalOceanBot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DigitalOceanBot.Services.Paginators
{
    public class FirewallPaginatorService : IPaginator
    {
        private readonly StorageService _storageService;

        public FirewallPaginatorService(StorageService storageService)
        {
            _storageService = storageService;
        }
        
        public Paginator<InlineKeyboardMarkup> GetPage(int pageIndex)
        {
            var firewalls = _storageService.Get<IReadOnlyCollection<Firewall>>(StorageKeys.MyFirewalls);
            var droplets = _storageService.Get<IReadOnlyCollection<Droplet>>(StorageKeys.MyDroplets);

            if (firewalls is not (not null and {Count: > 0}))
            {
                throw new ArgumentNullException(nameof(firewalls));
            }

            var firewall = firewalls.Skip(pageIndex).Take(1).FirstOrDefault();

            return new Paginator<InlineKeyboardMarkup>
            {
                MessageText = FirewallMessage.GetFirewallInfoMessage(firewall, droplets),
                Keyboard = FirewallKeyboard.GetFirewallPaginatorKeyboard(pageIndex, firewalls.Count, firewall!.Id)
            };

        }

        public Paginator<ReplyKeyboardMarkup> Select(string id)
        {
            var firewall = _storageService.Get<IReadOnlyList<Firewall>>(StorageKeys.MyFirewalls).FirstOrDefault(x => x.Id == id);

            if (firewall is null)
            {
                throw new ArgumentNullException(nameof(firewall));
            }
            
            return new Paginator<ReplyKeyboardMarkup>
            {
                MessageText = FirewallMessage.GetSelectedFirewallMessage(firewall),
                Keyboard = FirewallKeyboard.GetFirewallOperationsKeyboard()
            };
        }
    }
}
