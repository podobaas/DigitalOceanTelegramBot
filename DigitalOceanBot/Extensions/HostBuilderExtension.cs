using System;
using DigitalOcean.API;
using DigitalOceanBot.Core;
using DigitalOceanBot.Services;
using DigitalOceanBot.Services.Paginators;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Telegram.Bot;

namespace DigitalOceanBot.Extensions
{
    public static class HostBuilderExtension
    {
        public static IHostBuilder ConfigureLogging(this IHostBuilder hostBuilder)
        {
            return hostBuilder.UseSerilog((_, configuration) =>
            {
                configuration.Enrich.FromLogContext()
                    .MinimumLevel.Information()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .WriteTo.Console();
            });
        }
        
        public static IHostBuilder ConfigureTelegram(this IHostBuilder hostBuilder)
        {
            return hostBuilder.ConfigureServices((_, collection) =>
            {
                collection.AddSingleton<ITelegramBotClient>(x => new TelegramBotClient(Environment.GetEnvironmentVariable("TELEGRAM_TOKEN")));
            });
        }
        
        public static IHostBuilder ConfigureServices(this IHostBuilder hostBuilder)
        {
            return hostBuilder.ConfigureServices((_, collection) =>
            {
                collection.Scan(scan =>
                {
                    scan.FromAssemblyOf<IPaginator>()
                        .AddClasses()
                        .AsSelf()
                        .WithSingletonLifetime();
                });
                
                collection.AddSingleton<IDigitalOceanClient>(x => new DigitalOceanClient(Environment.GetEnvironmentVariable("DIGITALOCEAN_TOKEN")));
                collection.AddSingleton<StorageService>();
                collection.AddSingleton<BotCommandManager>();
            });
        }
        
        
        public static IHostBuilder ConfigureHostService(this IHostBuilder hostBuilder)
        {
            return hostBuilder.ConfigureServices((_, collection) =>
            {
                collection.AddHostedService<DigitalOceanWorker>();
            });
        }
    }
}