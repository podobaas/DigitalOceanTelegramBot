using DigitalOcean.API;

namespace DigitalOceanBot.Factory
{
    public interface IDigitalOceanClientFactory
    {
        IDigitalOceanClient GetInstance(int userId);
    }
}
