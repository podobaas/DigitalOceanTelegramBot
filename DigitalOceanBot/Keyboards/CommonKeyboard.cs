using DigitalOceanBot.Core;
using DigitalOceanBot.Types;
using DigitalOceanBot.Types.Const;
using Telegram.Bot.Types.ReplyMarkups;

namespace DigitalOceanBot.Keyboards
{
    internal static class CommonKeyboard
    {
        public static ReplyKeyboardMarkup GetMainMenuKeyboard()
        {
            var inlineKeyboardButtons = new[]
            {
                new[]
                {
                    new KeyboardButton(BotCommands.Account),
                    new KeyboardButton(BotCommands.Projects),
                },
                new[]
                {
                    new KeyboardButton(BotCommands.Droplets),
                    new KeyboardButton(BotCommands.Firewalls),
                }
            };

            return new ReplyKeyboardMarkup(inlineKeyboardButtons, true);
        }
    }
}