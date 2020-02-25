using System.Collections.Generic;
using System.Linq;
using System.Text;
using DigitalOceanBot.Helpers;
using DigitalOceanBot.Models;
using DigitalOceanBot.MongoDb;
using DigitalOceanBot.MongoDb.Models;
using Telegram.Bot.Types.ReplyMarkups;

namespace DigitalOceanBot.Pages
{
    public class RegionPage : IPage
    {
        private readonly IRepository<Session> _sessionRepo;

        public RegionPage(IRepository<Session> sessionRepo)
        {
            _sessionRepo = sessionRepo;
        }

        public PageModel GetPage(int userId, int page = 0)
        {
            var stringBuilder = new StringBuilder(string.Empty);
            var session = _sessionRepo.Get(userId);
            var regions = session.Data.CastObject<CreateDroplet>()
                                        .Regions
                                        .Where(r => r.Available)
                                        .OrderByDescending(r => r.Name);

            if (regions.Any())
            {
                var region = regions.Skip(page).Take(1).FirstOrDefault();
                stringBuilder.Append("\U0001F3F3 *Choose region for droplet* \n\n");
                stringBuilder.Append($"{GetFlag(region.Slug)} Name: *{region.Name}*\n");

                return new PageModel
                {
                    Message = stringBuilder.ToString(),
                    Keyboard = GetInlineKeyboard(region.Slug, regions.Count(), page)
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
            var region = session.Data.CastObject<CreateDroplet>().Regions.FirstOrDefault(r => r.Slug == (string)id);

            if (region != null)
            {
                stringBuilder.Append($"Selected region: *{GetFlag(region.Slug)} {region.Name}* \n\n");
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

        private static InlineKeyboardMarkup GetInlineKeyboard(string slug, int regionCount, int page)
        {
            var back = new InlineKeyboardButton
            {
                Text = "\U0001F448 Back",
                CallbackData = page > 0 ? $"BackRegion;{(page - 1).ToString()}" : "Empty"
            };

            var next = new InlineKeyboardButton
            {
                Text = "Next \U0001F449",
                CallbackData = page < regionCount - 1 ? $"NextRegion;{(page + 1).ToString()}" : "Empty"
            };

            var count = new InlineKeyboardButton
            {
                Text = $"{(page + 1).ToString()}/{regionCount.ToString()}",
                CallbackData = "Empty"
            };

            var choose = new InlineKeyboardButton
            {
                Text = $"Select this region",
                CallbackData = $"SelectRegion;{slug}"
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

        private static string GetFlag(string slug)
        {
            switch (slug)
            {
                case "nyc1":
                case "nyc2":
                case "nyc3":
                case "sfo1":
                case "sfo2":
                case "sfo3":
                    return "\U0001F1FA\U0001F1F8";
                case "ams1":
                case "ams2":
                case "ams3":
                    return "\U0001F1F3\U0001F1F1";
                case "sgp1":
                case "sgp2":
                case "sgp3":
                    return "\U0001F1F8\U0001F1EC";
                case "lon1":
                case "lon2":
                case "lon3":
                    return "\U0001F1EC\U0001F1E7";
                case "fra1":
                case "fra2":
                case "fra3":
                    return "\U0001F1E9\U0001F1EA";
                case "tor1":
                case "tor2":
                case "tor3":
                    return "\U0001F1E8\U0001F1E6";
                case "blr1":
                case "blr2":
                case "blr3":
                    return "\U0001F1EE\U0001F1F3";
                default:
                    return "\U0001F6A9";
            }
        }
    }
}

