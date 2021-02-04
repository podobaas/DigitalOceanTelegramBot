using System.Collections.Generic;
using DigitalOceanBot.Core;
using DigitalOceanBot.Types;
using DigitalOceanBot.Types.Const;
using DigitalOceanBot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DigitalOceanBot.Keyboards
{
    internal static class DropletKeyboard
    {
        public static ReplyKeyboardMarkup GetDropletOperationsKeyboard()
        {
            var inlineKeyboardButtons = new[]
            {
                new[]
                {
                    new KeyboardButton(BotCommands.DropletRename),
                },
                new[]
                {
                    new KeyboardButton(BotCommands.DropletReboot),
                    new KeyboardButton(BotCommands.DropletPowerCycle),
                },
                new[]
                {
                    new KeyboardButton(BotCommands.DropletShutdown),
                    new KeyboardButton(BotCommands.DropletPowerOn),
                },
                new[]
                {
                    new KeyboardButton(BotCommands.DropletCreateSnapshot),
                    new KeyboardButton(BotCommands.DropletResetPassword),
                },
                new[]
                {
                    new KeyboardButton(BotCommands.Back)
                }
            };

            return new ReplyKeyboardMarkup(inlineKeyboardButtons, true);
        }
        
        public static InlineKeyboardMarkup GetDropletPaginatorKeyboard(int pageIndex, int count, string id)
        {
            var back = new InlineKeyboardButton
            {
                Text = "\U0001F448 Prev",
                CallbackData = pageIndex > 0 ? 
                    $"{BotCallbackQueryType.DropletPrevious};{(pageIndex - 1).ToString()}" : 
                    $"{BotCallbackQueryType.None}"
            };

            var next = new InlineKeyboardButton
            {
                Text = "Next \U0001F449",
                CallbackData = pageIndex < count - 1 ? 
                    $"{BotCallbackQueryType.DropletNext};{(pageIndex + 1).ToString()}" : 
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
                CallbackData = $"{BotCallbackQueryType.DropletSelect};{id}"
            };
            
            var createNew = new InlineKeyboardButton
            {
                Text = $"Create new",
                CallbackData = $"{BotCallbackQueryType.DropletSelect};{id}"
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
                },
                new()
                {
                    createNew
                }
            };

            return new InlineKeyboardMarkup(buttons);
        }
    }
}