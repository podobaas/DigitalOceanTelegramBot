using System.Collections.Generic;
using DigitalOceanBot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DigitalOceanBot.Keyboards
{
    internal static class SizeKeyboard
    {
        public static InlineKeyboardMarkup GetSizePaginatorKeyboard(int pageIndex, int count, string id)
        {
            var back = new InlineKeyboardButton
            {
                Text = "\U0001F448 Prev",
                CallbackData = pageIndex > 0 ? 
                    $"{BotCallbackQueryType.SizePrevious};{(pageIndex - 1).ToString()}" : 
                    $"{BotCallbackQueryType.None}"
            };

            var next = new InlineKeyboardButton
            {
                Text = "Next \U0001F449",
                CallbackData = pageIndex < count - 1 ? 
                    $"{BotCallbackQueryType.SizeNext};{(pageIndex + 1).ToString()}" : 
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
                CallbackData = $"{BotCallbackQueryType.SizeSelect};{id}"
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