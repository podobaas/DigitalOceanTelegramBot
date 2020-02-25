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
    public class DropletPage : IPage
    {
        private readonly IRepository<Session> _sessionRepo;

        public DropletPage(IRepository<Session> sessionRepo)
        {
            _sessionRepo = sessionRepo;
        }

        public PageModel GetPage(int userId, int page = 0)
        {
            var stringBuilder = new StringBuilder(string.Empty);
            var session = _sessionRepo.Get(userId);
            var droplets = session.Data.CastObject<IReadOnlyList<Droplet>>();

            if (droplets.Count > 0)
            {
                var droplet = droplets.Skip(page).Take(1).FirstOrDefault();

                stringBuilder.Append($"\U0001F4A7 *{droplet.Name} (Created at UTC {droplet.CreatedAt.ToString("dd/MM/yyyy HH:mm:ss")})*\n\n");

                stringBuilder.Append($"Status: {GetStatusDroplet(droplet.Status)} *({droplet.Status})*\n");
                stringBuilder.Append($"Image: *{droplet.Image.Distribution} {droplet.Image.Name}*\n");
                stringBuilder.Append($"vCPUs: *{droplet.Vcpus.ToString()}*\n");
                stringBuilder.Append($"Memory: *{droplet.Memory.ToString()}MB*\n");
                stringBuilder.Append($"Disk: *{droplet.Disk.ToString()}GB*\n");
                stringBuilder.Append($"Region: *{droplet.Region.Name}*\n");
                stringBuilder.Append($"IPv4: *{string.Join(',', droplet.Networks.V4.Select(ip => ip.IpAddress))}*\n");
                stringBuilder.Append($"IPv6: *{string.Join(',', droplet.Networks.V6.Select(ip => ip.IpAddress))}*\n");
                stringBuilder.Append($"Tags: *{string.Join(',', droplet.Tags)}*\n");

                return new PageModel
                {
                    Message = stringBuilder.ToString(),
                    Keyboard = GetInlineKeyboard(droplet.Id, droplets.Count(), page)
                };
            }
            else
            {
                return new PageModel();
            }
        }

        public PageModel SelectPage(int userId, object id)
        {
            var stringBuilder = new StringBuilder(string.Empty);
            var session = _sessionRepo.Get(userId);
            var droplet = session.Data.CastObject<IEnumerable<Droplet>>().Where(d => d.Id == (int)id)?.FirstOrDefault();

            if (droplet != null)
            {
                stringBuilder.Append($"Selected droplet: \U0001F4A7 *{droplet.Name}*\n");
                
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

        private InlineKeyboardMarkup GetInlineKeyboard(int dropletId, int dropletsCount, int page)
        {
            var back = new InlineKeyboardButton
            {
                Text = "\U0001F448 Back",
                CallbackData = page > 0 ? $"BackDroplet;{(page - 1).ToString()}" : "Empty"
            };

            var next = new InlineKeyboardButton
            {
                Text = "Next \U0001F449",
                CallbackData = page < dropletsCount - 1 ? $"NextDroplet;{(page + 1).ToString()}" : "Empty"
            };

            var count = new InlineKeyboardButton
            {
                Text = $"{(page + 1).ToString()}/{dropletsCount.ToString()}",
                CallbackData = "Empty"
            };

            var choose = new InlineKeyboardButton
            {
                Text = $"Select this droplet",
                CallbackData = $"SelectDroplet;{dropletId.ToString()}"
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

        private static string GetStatusDroplet(string status)
        {
            switch (status)
            {
                case "off":
                    return "\U0001F534";
                case "active":
                    return "\U0001F7E2";
                case "archive":
                    return "\U0001F7E4";
                case "new":
                    return "\U000026AA";
                default:
                    return "\U000026AB";
            }
        }
    }
}
