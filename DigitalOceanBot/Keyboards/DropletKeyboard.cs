using System.Collections.Generic;
using DigitalOceanBot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DigitalOceanBot.Keyboards
{
    public static class DropletKeyboard
    {
        public static ReplyKeyboardMarkup GetDropletKeyboard()
        {
            var inlineKeyboardButtons = new[]
            {
                new[]
                {
                    new KeyboardButton(CommandConst.DropletCreateNew),
                },
                new[]
                {
                    new KeyboardButton(CommandConst.Back)
                }
            };

            return new ReplyKeyboardMarkup(inlineKeyboardButtons, true, true);
        }
        
        public static ReplyKeyboardMarkup GetDropletOperationsKeyboard()
        {
            var inlineKeyboardButtons = new[]
            {
                new[]
                {
                    new KeyboardButton(CommandConst.DropletRename),
                },
                new[]
                {
                    new KeyboardButton(CommandConst.DropletReboot),
                    new KeyboardButton(CommandConst.DropletPowerCycle),
                },
                new[]
                {
                    new KeyboardButton(CommandConst.DropletShutdown),
                    new KeyboardButton(CommandConst.DropletPowerOn),
                },
                new[]
                {
                    new KeyboardButton(CommandConst.DropletCreateSnapshot),
                    new KeyboardButton(CommandConst.DropletResetPassword),
                },
                new[]
                {
                    new KeyboardButton(CommandConst.Back)
                }
            };

            return new ReplyKeyboardMarkup(inlineKeyboardButtons, true, true);
        }
        
        public static InlineKeyboardMarkup GetDropletPaginatorKeyboard(int pageIndex, int count, string id)
        {
            var back = new InlineKeyboardButton
            {
                Text = "\U0001F448 Prev",
                CallbackData = pageIndex > 0 ? $"DropletPrevious;{(pageIndex - 1).ToString()}" : "None"
            };

            var next = new InlineKeyboardButton
            {
                Text = "Next \U0001F449",
                CallbackData = pageIndex < count - 1 ? $"DropletNext;{(pageIndex + 1).ToString()}" : "None"
            };

            var countLabel = new InlineKeyboardButton
            {
                Text = $"{(pageIndex + 1).ToString()}/{count.ToString()}",
                CallbackData = "None"
            };

            var select = new InlineKeyboardButton
            {
                Text = $"Select",
                CallbackData = $"DropletSelect;{id}"
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