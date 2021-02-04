using System.Collections.Generic;
using DigitalOceanBot.Core;
using DigitalOceanBot.Types;
using DigitalOceanBot.Types.Const;
using DigitalOceanBot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DigitalOceanBot.Keyboards
{
    internal static class FirewallKeyboard
    {
        public static ReplyKeyboardMarkup GetFirewallOperationsKeyboard()
        {
            var inlineKeyboardButtons = new[]
            {
                new[]
                {
                    new KeyboardButton(BotCommands.FirewallAddInboundRule),
                    new KeyboardButton(BotCommands.FirewallAddOutboundRule),
                },
                new[]
                {
                    new KeyboardButton(BotCommands.FirewallAddDroplets),
                    new KeyboardButton(BotCommands.FirewallRemoveDroplets)
                },
                new[]
                {
                    new KeyboardButton(BotCommands.Back)
                }
            };

            return new ReplyKeyboardMarkup(inlineKeyboardButtons, true);
        }
        
        public static InlineKeyboardMarkup GetFirewallPaginatorKeyboard(int pageIndex, int count, string id)
        {
            var back = new InlineKeyboardButton
            {
                Text = "\U0001F448 Prev",
                CallbackData = pageIndex > 0 ? 
                    $"{BotCallbackQueryType.FirewallPrevious};{(pageIndex - 1).ToString()}" : 
                    $"{BotCallbackQueryType.None}"
            };

            var next = new InlineKeyboardButton
            {
                Text = "Next \U0001F449",
                CallbackData = pageIndex < count - 1 ? 
                    $"{BotCallbackQueryType.FirewallNext};{(pageIndex + 1).ToString()}" : 
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
                CallbackData = $"{BotCallbackQueryType.FirewallSelect};{id}"
            };
            
            var createNew = new InlineKeyboardButton
            {
                Text = $"Create new",
                CallbackData = $"{BotCallbackQueryType.FirewallCreateNew}"
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