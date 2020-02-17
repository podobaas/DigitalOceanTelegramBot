using DigitalOceanBot.Models;

namespace DigitalOceanBot.Pages
{
    public interface IPage
    {
        PageModel GetPage(int userId, int page = 0);

        PageModel SelectPage(int userId, object id);
    }
}
