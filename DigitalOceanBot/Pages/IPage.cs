using DigitalOceanBot.Models;

namespace DigitalOceanBot.Pages
{
    public interface IPage
    {
        PageModel GetPage(int userId, int page);

        PageModel SelectPage(int userId, object id);
    }
}
