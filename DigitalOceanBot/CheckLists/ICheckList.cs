using System.Collections.Generic;
using DigitalOceanBot.Models;
using Telegram.Bot.Types.ReplyMarkups;

namespace DigitalOceanBot.CheckLists
{
    public interface ICheckListPage<in T>
    {
        PageModel GetCheckListPage(IEnumerable<T> collection, bool hasSkip = false);

        InlineKeyboardMarkup SelectItem(InlineKeyboardMarkup inlineKeyboardMarkup, string id);

        IEnumerable<string> GetSelectedItems(InlineKeyboardMarkup inlineKeyboardMarkup);

    }
}
