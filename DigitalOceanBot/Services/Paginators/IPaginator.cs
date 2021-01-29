using DigitalOceanBot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DigitalOceanBot.Services.Paginators
{
    public interface IPaginator
    {
        Paginator<InlineKeyboardMarkup> GetPage(int pageIndex);

        Paginator<ReplyKeyboardMarkup> Select(string id);
    }
}
