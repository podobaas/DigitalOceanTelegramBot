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
    public sealed class ImagePaginatorService : IPaginator
    {
        private readonly StorageService _storageService;

        public ImagePaginatorService(StorageService storageService)
        {
            _storageService = storageService;
        }

        public Paginator<InlineKeyboardMarkup> GetPage(int pageIndex)
        {
            var images = _storageService.Get<IReadOnlyList<Image>>(StorageKeys.Images);

            if (images is not (not null and {Count: > 0}))
            {
                throw new ArgumentNullException(nameof(images));
            }

            var image = images.Skip(pageIndex).Take(1).FirstOrDefault();
            
            return new Paginator<InlineKeyboardMarkup>
            {
                MessageText = ImageMessage.GetImageInfoMessage(image),
                Keyboard = ImageKeyboard.GetImagePaginatorKeyboard(pageIndex, images.Count, image!.Id.ToString())
            };

        }

        public Paginator<ReplyKeyboardMarkup> Select(string id)
        {
            var image = _storageService.Get<IReadOnlyList<Image>>(StorageKeys.Images).FirstOrDefault(x => x.Id == long.Parse(id));

            if (image is null)
            {
                throw new ArgumentNullException(nameof(image));
            }
            
            return new Paginator<ReplyKeyboardMarkup>
            {
                MessageText = ImageMessage.GetSelectedImageMessage(image),
                Keyboard = null
            };

        }
    }
}
