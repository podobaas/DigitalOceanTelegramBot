using System;
using DigitalOceanBot.Extensions;
using Microsoft.Extensions.Hosting;

namespace DigitalOceanBot
{ 
    static class Program
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
