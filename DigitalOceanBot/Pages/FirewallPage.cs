using System.Collections.Generic;
using System.Linq;
using System.Text;
using DigitalOcean.API.Models.Responses;
using DigitalOceanBot.Helpers;
using DigitalOceanBot.Models;
using DigitalOceanBot.MongoDb;
using DigitalOceanBot.MongoDb.Models;
using Telegram.Bot.Types.ReplyMarkups;

namespace DigitalOceanBot.Pages
{
    public class FirewallPage : IPage
    {
        private readonly IRepository<Session> _sessionRepo;
        private readonly IReadOnlyList<Droplet> _droplets;

        public FirewallPage(IRepository<Session> sessionRepo)
        {
            _sessionRepo = sessionRepo;
        }

        public FirewallPage(IRepository<Session> sessionRepo, IReadOnlyList<Droplet> droplets)
        {
            _sessionRepo = sessionRepo;
            _droplets = droplets;
        }

        public PageModel GetPage(int userId, int page = 0)
        {
            var stringBuilder = new StringBuilder(string.Empty);
            var session = _sessionRepo.Get(userId);
            var firewalls = session.Data.CastObject<IReadOnlyList<Firewall>>();

            if (firewalls.Count > 0)
            {
                var firewall = firewalls.Skip(page).Take(1).FirstOrDefault();

                if (firewall != null)
                {
                    stringBuilder.Append($"\U0001F3F0 *{firewall.Name} (Created at UTC {firewall.CreatedAt.ToString("dd/MM/yyyy HH:mm:ss")})*\n\n");
                    stringBuilder.Append($"Status: *{GetFirewallStatus(firewall.Status)} ({firewall.Status})*\n");

                    if (firewall?.DropletIds?.Count > 0)
                    {
                        stringBuilder.Append($"Related droplets: *{string.Join(',', _droplets.Where(d => firewall.DropletIds.Contains(d.Id)).Select(d => d.Name))}*\n");
                    }

                    if (firewall.Tags?.Count > 0)
                    {
                        stringBuilder.Append($"Tags: *{string.Join(',', firewall.Tags)}*\n");
                    }

                    stringBuilder.Append($"\n\U00002B07 *Inbound rules:* \n");

                    foreach (var rule in firewall.InboundRules)
                    {
                        stringBuilder.Append($"---------------------------\n");
                        stringBuilder.Append($"Protocol: *{rule.Protocol}*\n");
                        stringBuilder.Append($"Ports: *{rule.Ports}*\n");

                        if (rule.Sources?.Addresses?.Count > 0)
                        {
                            stringBuilder.Append($"Addresses: *{string.Join(',', rule.Sources.Addresses)}*\n");
                        }
                    }

                    stringBuilder.Append($"---------------------------\n");
                    stringBuilder.Append($"\n\U00002B06	*Outbound rules:* \n");

                    foreach (var rule in firewall.OutboundRules)
                    {
                        stringBuilder.Append($"---------------------------\n");
                        stringBuilder.Append($"Protocol: *{rule.Protocol}*\n");
                        stringBuilder.Append($"Ports: *{rule.Ports}*\n");

                        if (rule?.Destinations?.Addresses?.Count > 0)
                        {
                            stringBuilder.Append($"Destinations: *{string.Join(',', rule.Destinations.Addresses)}*\n");
                        }
                    }
                    stringBuilder.Append($"---------------------------");

                    return new PageModel
                    {
                        Message = stringBuilder.ToString(),
                        Keyboard = GetInlineKeyboard(firewall.Id, firewalls.Count, page)
                    };
                }
            }

            return new PageModel();
        }

        public PageModel SelectPage(int userId, object id)
        {
            var stringBuilder = new StringBuilder(string.Empty);
            var session = _sessionRepo.Get(userId);
            var firewall = session.Data
                .CastObject<IReadOnlyCollection<Firewall>>()
                .FirstOrDefault(f => f.Id == (string)id);

            if (firewall != null)
            {
                stringBuilder.Append($"Selected firewall: \U0001F3F0 *{firewall.Name}*");

                return new PageModel
                {
                    Message = stringBuilder.ToString()
                };
            }
            else
            {
                return new PageModel();
            }
        }

        private InlineKeyboardMarkup GetInlineKeyboard(string imageId, int imageCount, int page)
        {
            var back = new InlineKeyboardButton
            {
                Text = "\U0001F448 Back",
                CallbackData = page > 0 ? $"BackFirewall;{(page - 1).ToString()}" : "Empty"
            };

            var next = new InlineKeyboardButton
            {
                Text = "Next \U0001F449",
                CallbackData = page < imageCount - 1 ? $"NextFirewall;{(page + 1).ToString()}" : "Empty"
            };

            var count = new InlineKeyboardButton
            {
                Text = $"{(page + 1).ToString()}/{imageCount.ToString()}",
                CallbackData = "Empty"
            };

            var choose = new InlineKeyboardButton
            {
                Text = $"Select this firewall",
                CallbackData = $"SelectFirewall;{imageId}"
            };

            var buttons = new List<List<InlineKeyboardButton>>
            {
                new List<InlineKeyboardButton>
                {
                    back,
                    count,
                    next
                },
                new List<InlineKeyboardButton>
                {
                    choose
                }
            };

            return new InlineKeyboardMarkup(buttons);
        }

        private static string GetFirewallStatus(string status)
        {
            switch (status)
            {
                case "failed":
                    return "\U0001F534";
                case "succeeded":
                    return "\U0001F7E2";
                case "waiting":
                    return "\U0001F7E4";
                default:
                    return "\U000026AB";
            }
        }
    }
}
