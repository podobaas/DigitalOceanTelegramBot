using DigitalOceanBot.Extensions;
using Microsoft.Extensions.Hosting;

namespace DigitalOceanBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureLogging()
                .ConfigureTelegram()
                .ConfigureServices()
                .ConfigureHostService();


            host.Start();
        }
    }
}
