using DigitalOceanBot.Pages;

namespace DigitalOceanBot.Factory
{
    public interface IPageFactory
    {
        IPage GetInstance<T>() where T : IPage;

        IPage GetInstance<T>(object param1) where T : IPage;
    }
}
