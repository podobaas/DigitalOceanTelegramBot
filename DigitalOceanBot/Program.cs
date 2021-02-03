using DigitalOceanBot.Extensions;
using Microsoft.Extensions.Hosting;

namespace DigitalOceanBot
{
    internal static class Program
    {
        private static void Main(string[] args)
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
