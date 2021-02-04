using System.Collections.Generic;
using DigitalOceanBot.Types;
using DigitalOceanBot.Types.Enums;
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
                CallbackData = pageIndex > 0 ? 
                    $"{BotCallbackQueryType.ImagePrevious};{(pageIndex - 1).ToString()}" : 
                    $"{BotCallbackQueryType.None}"
            };

            var next = new InlineKeyboardButton
            {
                Text = "Next \U0001F449",
                CallbackData = pageIndex < count - 1 ? 
                    $"{BotCallbackQueryType.ImageNext};{(pageIndex + 1).ToString()}" : 
                    $"{BotCallbackQueryType.None}"
            };

            var countLabel = new InlineKeyboardButton
            {
                Text = $"{(pageIndex + 1).ToString()}/{count.ToString()}",
                CallbackData = $"{BotCallbackQueryType.None}"
            };

            var select = new InlineKeyboardButton
            {
                Text = $"Select",
                CallbackData = $"{BotCallbackQueryType.ImageSelect};{id}"
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