using System;
using DigitalOcean.API;
using DigitalOceanBot.Core;
using DigitalOceanBot.Core.CallbackQueries;
using DigitalOceanBot.Core.Commands;
using DigitalOceanBot.Core.StateHandlers;
using DigitalOceanBot.Services;
using DigitalOceanBot.Services.Paginators;
using DigitalOceanBot.Workers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;

namespace DigitalOceanBot.Extensions
{
    internal static class HostBuilderExtension
    {
        public static IHostBuilder ConfigureLogging(this IHostBuilder hostBuilder)
        {
            return hostBuilder.ConfigureLogging((_, configuration) =>
            {
                configuration.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Warning);
                configuration.AddFilter(x => x == LogLevel.Information);
                configuration.AddConsole();
            });
        }
        
        public static IHostBuilder ConfigureTelegram(this IHostBuilder hostBuilder)
        {
            return hostBuilder.ConfigureServices((_, collection) =>
            {
                collection.AddSingleton<ITelegramBotClient>(_ => new TelegramBotClient(EnvironmentVars.GetTelegramToken()));
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
                    
                    scan.FromAssemblyOf<IBotCommand>()
                        .AddClasses(x => x.AssignableTo<IBotCommand>())
                        .As<IBotCommand>()
                        .WithSingletonLifetime();
                    
                    scan.FromAssemblyOf<IBotStateHandler>()
                        .AddClasses(x => x.AssignableTo<IBotStateHandler>())
                        .As<IBotStateHandler>()
                        .WithSingletonLifetime();
                    
                    scan.FromAssemblyOf<IBotCallbackQuery>()
                        .AddClasses(x => x.AssignableTo<IBotCallbackQuery>())
                        .As<IBotCallbackQuery>()
                        .WithSingletonLifetime();
                    
                });

                collection.AddSingleton<IDigitalOceanClient>(_ => new DigitalOceanClient(EnvironmentVars.GetDigitalOceanToken()));
                collection.AddSingleton<StorageService>();
                collection.AddSingleton<BotCommandResolver>();
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