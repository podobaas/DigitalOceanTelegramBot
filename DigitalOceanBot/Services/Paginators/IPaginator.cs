using System;
using DigitalOceanBot.Types;
using DigitalOceanBot.Types.Classes;
using Telegram.Bot.Types.ReplyMarkups;

namespace DigitalOceanBot.Services.Paginators
{
    public interface IPaginator
    { 
        Action OnSelectCallback { get; set; }
        
        Paginator<InlineKeyboardMarkup> GetPage(int pageIndex);

        Paginator<ReplyKeyboardMarkup> Select(string id);
    }
}
