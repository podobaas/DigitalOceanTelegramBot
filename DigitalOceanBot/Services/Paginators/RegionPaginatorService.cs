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
    public sealed class RegionPaginatorService : IPaginator
    {
        private readonly StorageService _storageService;
        
        public Action OnSelectCallback { get; set; }

        public RegionPaginatorService(StorageService storageService)
        {
            _storageService = storageService;
        }

        public Paginator<InlineKeyboardMarkup> GetPage(int pageIndex)
        {
            var regions = _storageService.Get<IReadOnlyList<Region>>(StorageKeys.Regions);

            if (regions is not (not null and {Count: > 0}))
            {
                throw new ArgumentNullException(nameof(regions));
            }

            var region = regions.Skip(pageIndex).Take(1).FirstOrDefault();
            
            return new Paginator<InlineKeyboardMarkup>
            {
                MessageText = RegionMessage.GetRegionInfoMessage(region),
                Keyboard = RegionKeyboard.GetRegionPaginatorKeyboard(pageIndex, regions.Count, region!.Slug)
            };

        }

        public Paginator<ReplyKeyboardMarkup> Select(string id)
        {
            var region = _storageService.Get<IReadOnlyList<Region>>(StorageKeys.Regions).FirstOrDefault(x => x.Slug == id);

            if (region is null)
            {
                throw new ArgumentNullException(nameof(region));
            }

            return new Paginator<ReplyKeyboardMarkup>
            {
                MessageText = RegionMessage.GetSelectedRegionMessage(region),
                Keyboard = null
            };

        }
    }
}
