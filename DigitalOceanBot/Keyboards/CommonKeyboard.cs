using DigitalOceanBot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DigitalOceanBot.Keyboards
{
    public static class CommonKeyboard
    {
        public static ReplyKeyboardMarkup GetMainMenuKeyboard()
        {
            var inlineKeyboardButtons = new[]
            {
                new[]
                {
                    new KeyboardButton(CommandConst.Account),
                    new KeyboardButton(CommandConst.Projects),
                },
                new[]
                {
                    new KeyboardButton(CommandConst.Droplets),
                    new KeyboardButton(CommandConst.Firewalls),
                }
            };

            return new ReplyKeyboardMarkup(inlineKeyboardButtons, true, true);
        }
    }
}