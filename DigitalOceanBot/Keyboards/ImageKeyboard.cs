using System.Collections.Generic;
using DigitalOceanBot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DigitalOceanBot.Keyboards
{
    internal static class ImageKeyboard
    {
        public static InlineKeyboardMarkup GetImagePaginatorKeyboard(int pageIndex, int count, string id)
        {
            var back = new InlineKeyboardButton
            {
                Text = "\U0001F448 Prev",
                CallbackData = pageIndex > 0 ? $"ImagePrevious;{(pageIndex - 1).ToString()}" : "None"
            };

            var next = new InlineKeyboardButton
            {
                Text = "Next \U0001F449",
                CallbackData = pageIndex < count - 1 ? $"ImageNext;{(pageIndex + 1).ToString()}" : "None"
            };

            var countLabel = new InlineKeyboardButton
            {
                Text = $"{(pageIndex + 1).ToString()}/{count.ToString()}",
                CallbackData = "None"
            };

            var select = new InlineKeyboardButton
            {
                Text = $"Select",
                CallbackData = $"ImageSelect;{id}"
            };
            
            var buttons = new List<List<InlineKeyboardButton>>
            {
                new()
                {
                    back,
                    countLabel,
                    next
                },
                new()
                {
                    select
                }
            };

            return new InlineKeyboardMarkup(buttons);
        }
    }
}