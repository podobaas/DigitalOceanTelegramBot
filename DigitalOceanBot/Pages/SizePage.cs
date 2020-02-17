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
    public class SizePage : IPage
    {
        private readonly IRepository<Session> _sessionRepo;

        public SizePage(IRepository<Session> sessionRepo)
        {
            _sessionRepo = sessionRepo;
        }

        public PageModel GetPage(int userId, int page = 0)
        {
            var stringBuilder = new StringBuilder(string.Empty);
            var session = _sessionRepo.Get(userId);
            var createDropletModel = session.Data.CastObject<CreateDroplet>();
            var sizes = createDropletModel.Sizes.Where(s => s.Available && s.Regions.Contains(createDropletModel.Droplet.Region));

            if (sizes.Any())
            {
                var size = sizes.Skip(page).Take(1).FirstOrDefault();
                stringBuilder.Append("\U0001F4BB *Choose size for droplet* \n\n");
                stringBuilder.Append($"Name: *{size.Slug}* \n");
                stringBuilder.Append($"Price per month: *{size.PriceMonthly}$\\mo* \n");
                stringBuilder.Append($"Price per hour: *{size.PriceHourly}$\\hour* \n");
                stringBuilder.Append($"Size: *{size.Memory} MB\\{size.Vcpus} vCPUs*");

                return new PageModel
                {
                    Message = stringBuilder.ToString(),
                    Keyboard = GetInlineKeyboard(size.Slug, sizes.Count(), page)
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
            var size = session.Data.CastObject<CreateDroplet>().Sizes.FirstOrDefault(r => r.Slug == (string)id);

            if (size != null)
            {
                stringBuilder.Append($"Selected size: \U0001F4BB *{size.Slug}*");
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

        private InlineKeyboardMarkup GetInlineKeyboard(string slug, int regionCount, int page)
        {
            var back = new InlineKeyboardButton()
            {
                Text = "\U0001F448 Back",
                CallbackData = page > 0 ? $"BackSize;{(page - 1).ToString()}" : "Empty"
            };

            var next = new InlineKeyboardButton
            {
                Text = "Next \U0001F449",
                CallbackData = page < regionCount - 1 ? $"NextSize;{(page + 1).ToString()}" : "Empty"
            };

            var count = new InlineKeyboardButton
            {
                Text = $"{(page + 1).ToString()}/{regionCount.ToString()}",
                CallbackData = "Empty"
            };

            var choose = new InlineKeyboardButton
            {
                Text = $"Select this size",
                CallbackData = $"SelectSize;{slug.ToString()}"
            };

            var buttons = new List<List<InlineKeyboardButton>>()
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
    }
}

