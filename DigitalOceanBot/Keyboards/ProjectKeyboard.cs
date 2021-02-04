using System.Collections.Generic;
using DigitalOceanBot.Core;
using DigitalOceanBot.Types;
using DigitalOceanBot.Types.Const;
using DigitalOceanBot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DigitalOceanBot.Keyboards
{
    internal static class ProjectKeyboard
    {
        public static ReplyKeyboardMarkup GetProjectOperationsKeyboard()
        {
            var inlineKeyboardButtons = new[]
            {
                new[]
                {
                    new KeyboardButton(BotCommands.SetAsDefaultProject)
                },
                new[]
                {
                    new KeyboardButton(BotCommands.RenameProject),
                    new KeyboardButton(BotCommands.ChangeDescriptionProject),
                },
                new[]
                {
                    new KeyboardButton(BotCommands.ChangePurposeProject),
                    new KeyboardButton(BotCommands.ChangeEnvironmentProject)
                },
                new[]
                {
                    new KeyboardButton(BotCommands.Back)
                }
            };

            return new ReplyKeyboardMarkup(inlineKeyboardButtons, true);
        }
        
        public static ReplyKeyboardMarkup GetProjectPurposeKeyboard()
        {
            var inlineKeyboardButtons = new[]
            {
                new[]
                {
                    new KeyboardButton("Just trying out DigitalOcean"),
                },
                new[]
                {
                    new KeyboardButton("Class project / Educational purposes")
                },
                new[]
                {
                    new KeyboardButton("Website or blog")
                },
                new[]
                {
                    new KeyboardButton("Web Application")
                },
                new[]
                {
                    new KeyboardButton("Service or API")
                },
                new[]
                {
                    new KeyboardButton("Mobile Application")
                },
                new[]
                {
                    new KeyboardButton("Machine learning / AI / Data processing")
                },
                new[]
                {
                    new KeyboardButton("IoT")
                },
                new[]
                {
                    new KeyboardButton("Operational / Developer tooling")
                },
                new[]
                {
                    new KeyboardButton("Other")
                }
            };

            return new ReplyKeyboardMarkup(inlineKeyboardButtons, true, true);
        }
        
        public static ReplyKeyboardMarkup GetProjectEnvironmentKeyboard()
        {
            var inlineKeyboardButtons = new[]
            {
                new[]
                {
                    new KeyboardButton("Development"),
                },
                new[]
                {
                    new KeyboardButton("Staging")
                },
                new[]
                {
                    new KeyboardButton("Production")
                }
            };

            return new ReplyKeyboardMarkup(inlineKeyboardButtons, true, true);
        }

        public static InlineKeyboardMarkup GetProjectPaginatorKeyboard(int pageIndex, int count, string id)
        {
            var back = new InlineKeyboardButton
            {
                Text = "\U0001F448 Prev",
                CallbackData = pageIndex > 0 ? 
                    $"{BotCallbackQueryType.ProjectPrevious};{(pageIndex - 1).ToString()}" : 
                    $"{BotCallbackQueryType.None}"
            };

            var next = new InlineKeyboardButton
            {
                Text = "Next \U0001F449",
                CallbackData = pageIndex < count - 1 ? 
                    $"{BotCallbackQueryType.ProjectNext};{(pageIndex + 1).ToString()}" : 
                    $"{BotCallbackQueryType.None}"
            };

            var countLabel = new InlineKeyboardButton
            {
                Text = $"{(pageIndex + 1).ToString()}/{count.ToString()}",
                CallbackData = $"{BotCallbackQueryType.None}"
            };

            var choose = new InlineKeyboardButton
            {
                Text = "Select",
                CallbackData = $"{BotCallbackQueryType.ProjectSelect};{id}"
            };
            
            var createNew = new InlineKeyboardButton
            {
                Text = $"Create new",
                CallbackData = $"{BotCallbackQueryType.ProjectCreateNew};{id}"
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
                    choose
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