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
    public sealed class DropletPaginatorService : IPaginator
    {
        private readonly StorageService _storageService;

        public Action OnSelectCallback { get; set; }
        
        public DropletPaginatorService(StorageService storageService)
        {
            _storageService = storageService;
        }

        public Paginator<InlineKeyboardMarkup> GetPage(int pageIndex)
        {
            var droplets = _storageService.Get<IReadOnlyList<Droplet>>(StorageKeys.Droplets);

            if (droplets is not (not null and {Count: > 0}))
            {
                throw new ArgumentNullException(nameof(droplets));
            }

            var droplet = droplets.Skip(pageIndex).Take(1).FirstOrDefault();
            
            return new Paginator<InlineKeyboardMarkup>
            {
                MessageText = DropletMessage.GetDropletInfoMessage(droplet),
                Keyboard = DropletKeyboard.GetDropletPaginatorKeyboard(pageIndex, droplets.Count, droplet!.Id.ToString())
            };

        }

        public Paginator<ReplyKeyboardMarkup> Select(string id)
        {
            var droplet = _storageService.Get<IReadOnlyList<Droplet>>(StorageKeys.Droplets).FirstOrDefault(x => x.Id == long.Parse(id));

            if (droplet is null)
            {
                throw new ArgumentNullException(nameof(droplet));
            }
            
            return new Paginator<ReplyKeyboardMarkup>
            {
                MessageText = DropletMessage.GetSelectedDropletMessage(droplet),
                Keyboard = DropletKeyboard.GetDropletOperationsKeyboard()
            };

        }
    }
}
