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
    public class ImagePage : IPage
    {
        private readonly IRepository<Session> _sessionRepo;

        public ImagePage(IRepository<Session> sessionRepo)
        {
            _sessionRepo = sessionRepo;
        }

        public PageModel GetPage(int userId, int page = 0)
        {
            var stringBuilder = new StringBuilder(string.Empty);
            var session = _sessionRepo.Get(userId);
            var images = session.Data.CastObject<CreateDroplet>();

            if (images.Images.Count > 0)
            {
                var image = images.Images.Skip(page).Take(1).FirstOrDefault();

                stringBuilder.Append("\U0001F4BF *Choose image for droplet* \n \n");
                stringBuilder.Append($"Name: *{image.Distribution} ({image.Slug})*\n");
                stringBuilder.Append($"Minimum disk size: *{image.MinDiskSize}*\n");
                stringBuilder.Append($"Size image: *{image.SizeGigabytes}GB*\n");

                return new PageModel
                {
                    Message = stringBuilder.ToString(),
                    Keyboard = GetInlineKeyboard(image.Id, images.Images.Count, page)
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
            var image = session.Data.CastObject<CreateDroplet>()
                                    .Images
                                    .Where(d => d.Id == (int)id)
                                    ?.FirstOrDefault();

            if (image != null)
            {
                stringBuilder.Append($"Selected image: \U0001F4BF *{image.Distribution} ({image.Slug})* \n \n");

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

        private InlineKeyboardMarkup GetInlineKeyboard(int imageId, int imageCount, int page)
        {
            var back = new InlineKeyboardButton
            {
                Text = "\U0001F448 Back",
                CallbackData = page > 0 ? $"BackImage;{(page - 1).ToString()}" : "Empty"
            };

            var next = new InlineKeyboardButton
            {
                Text = "Next \U0001F449",
                CallbackData = page < imageCount - 1 ? $"NextImage;{(page + 1).ToString()}" : "Empty"
            };

            var count = new InlineKeyboardButton
            {
                Text = $"{(page + 1).ToString()}/{imageCount.ToString()}",
                CallbackData = "Empty"
            };

            var choose = new InlineKeyboardButton
            {
                Text = $"Select this image",
                CallbackData = $"SelectImage;{imageId.ToString()}"
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
    }
}
