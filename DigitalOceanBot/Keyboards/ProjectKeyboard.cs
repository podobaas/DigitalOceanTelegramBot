using System.Collections.Generic;
using DigitalOceanBot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DigitalOceanBot.Keyboards
{
    internal static class ProjectKeyboard
    { 
        public static ReplyKeyboardMarkup GetProjectKeyboard()
        {
            var inlineKeyboardButtons = new[]
            {
                new[]
                {
                    new KeyboardButton(CommandConst.CreateNewProject),
                },
                new[]
                {
                    new KeyboardButton(CommandConst.Back)
                }
            };

            return new ReplyKeyboardMarkup(inlineKeyboardButtons, true);
        }
        
        public static ReplyKeyboardMarkup GetProjectOperationsKeyboard()
        {
            var inlineKeyboardButtons = new[]
            {
                new[]
                {
                    new KeyboardButton(CommandConst.SetAsDefaultProject)
                },
                new[]
                {
                    new KeyboardButton(CommandConst.RenameProject),
                    new KeyboardButton(CommandConst.ChangeDescriptionProject),
                },
                new[]
                {
                    new KeyboardButton(CommandConst.ChangePurposeProject),
                    new KeyboardButton(CommandConst.ChangeEnvironmentProject)
                },
                new[]
                {
                    new KeyboardButton(CommandConst.Back)
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
                CallbackData = pageIndex > 0 ? $"ProjectPrevious;{(pageIndex - 1).ToString()}" : "None"
            };

            var next = new InlineKeyboardButton
            {
                Text = "Next \U0001F449",
                CallbackData = pageIndex < count - 1 ? $"ProjectNext;{(pageIndex + 1).ToString()}" : "None"
            };

            var countLabel = new InlineKeyboardButton
            {
                Text = $"{(pageIndex + 1).ToString()}/{count.ToString()}",
                CallbackData = "None"
            };

            var choose = new InlineKeyboardButton
            {
                Text = "Select",
                CallbackData = $"ProjectSelect;{id}"
            };
            
            var createNew = new InlineKeyboardButton
            {
                Text = $"Create new",
                CallbackData = $"ProjectCreateNew;{id}"
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