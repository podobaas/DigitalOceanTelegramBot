using System.Collections.Generic;
using DigitalOceanBot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DigitalOceanBot.Keyboards
{
    internal static class FirewallKeyboard
    {
        public static ReplyKeyboardMarkup GetFirewallKeyboard()
        {
            var inlineKeyboardButtons = new[]
            {
                new[]
                {
                    new KeyboardButton(CommandConst.FirewallCreateNew),
                },
                new[]
                {
                    new KeyboardButton(CommandConst.Back)
                }
            };

            return new ReplyKeyboardMarkup(inlineKeyboardButtons, true);
        }
        
        public static ReplyKeyboardMarkup GetFirewallOperationsKeyboard()
        {
            var inlineKeyboardButtons = new[]
            {
                new[]
                {
                    new KeyboardButton(CommandConst.FirewallAddInboundRule),
                    new KeyboardButton(CommandConst.FirewallAddOutboundRule),
                },
                new[]
                {
                    new KeyboardButton(CommandConst.FirewallAddDroplets),
                    new KeyboardButton(CommandConst.FirewallRemoveDroplets)
                },
                new[]
                {
                    new KeyboardButton(CommandConst.Back)
                }
            };

            return new ReplyKeyboardMarkup(inlineKeyboardButtons, true);
        }
        
        public static InlineKeyboardMarkup GetFirewallPaginatorKeyboard(int pageIndex, int count, string id)
        {
            var back = new InlineKeyboardButton
            {
                Text = "\U0001F448 Prev",
                CallbackData = pageIndex > 0 ? $"FirewallPrevious;{(pageIndex - 1).ToString()}" : "None"
            };

            var next = new InlineKeyboardButton
            {
                Text = "Next \U0001F449",
                CallbackData = pageIndex < count - 1 ? $"FirewallNext;{(pageIndex + 1).ToString()}" : "None"
            };

            var countLabel = new InlineKeyboardButton
            {
                Text = $"{(pageIndex + 1).ToString()}/{count.ToString()}",
                CallbackData = "None"
            };

            var select = new InlineKeyboardButton
            {
                Text = $"Select",
                CallbackData = $"FirewallSelect;{id}"
            };
            
            var createNew = new InlineKeyboardButton
            {
                Text = $"Create new",
                CallbackData = $"FirewallCreateNew"
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