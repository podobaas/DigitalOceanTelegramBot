using System;
using System.Collections.Generic;
using System.Linq;
using DigitalOcean.API.Models.Responses;
using DigitalOceanBot.Keyboards;
using DigitalOceanBot.Messages;
using DigitalOceanBot.Types.Classes;
using Telegram.Bot.Types.ReplyMarkups;
using Action = System.Action;

namespace DigitalOceanBot.Services.Paginators
{
    public sealed class SizePaginatorService : IPaginator
    {
        private readonly StorageService _storageService;
        
        public Action OnSelectCallback { get; set; }

        public SizePaginatorService(StorageService storageService)
        {
            _storageService = storageService;
        }

        public Paginator<InlineKeyboardMarkup> GetPage(int pageIndex)
        {
            var sizes = _storageService.Get<IReadOnlyList<Size>>(StorageKeys.Sizes);

            if (sizes is not (not null and {Count: > 0}))
            {
                throw new ArgumentNullException(nameof(sizes));
            }

            var size = sizes.Skip(pageIndex).Take(1).FirstOrDefault();
            
            return new Paginator<InlineKeyboardMarkup>
            {
                MessageText = SizeMessage.GetSizeInfoMessage(size),
                Keyboard = SizeKeyboard.GetSizePaginatorKeyboard(pageIndex, sizes.Count, size!.Slug)
            };

        }

        public Paginator<ReplyKeyboardMarkup> Select(string id)
        {
            var size = _storageService.Get<IReadOnlyList<Size>>(StorageKeys.Sizes).FirstOrDefault(x => x.Slug == id);

            if (size is null)
            {
                throw new ArgumentNullException(nameof(size));
            }

            return new Paginator<ReplyKeyboardMarkup>
            {
                MessageText = SizeMessage.GetSelectedSizeMessage(size),
                Keyboard = null
            };

        }
    }
}
