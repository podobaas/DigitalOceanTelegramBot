using Telegram.Bot.Types.ReplyMarkups;

namespace DigitalOceanBot.Models
{
    public class PageModel
    {
        public string Message { get; set; }

        public InlineKeyboardMarkup Keyboard { get; set; }
    }
}
