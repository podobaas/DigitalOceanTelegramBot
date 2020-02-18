using System;
using System.IO;
using System.Reflection;
using DigitalOcean.API.Models.Requests;
using DigitalOceanBot.Factory;
using DigitalOceanBot.MongoDb;
using DigitalOceanBot.MongoDb.Models;
using EasyNetQ;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson.Serialization;
using Serilog;
using Telegram.Bot;

namespace DigitalOceanBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
             .UseSystemd()
             .ConfigureServices((hostContext, services) =>
             {
                 BsonRegisterClassMap();
                 
                 services.AddHostedService<DigitalOceanWorker>();
                 services.AddScoped<IRepository<DoUser>>(r => new UserRepository(Environment.GetEnvironmentVariable("MONGODB")));
                 services.AddScoped<IRepository<Session>>(r => new SessionRepository(Environment.GetEnvironmentVariable("MONGODB")));
                 services.AddScoped<IRepository<HandlerCallback>>(r => new HandlerCallbackRepository(Environment.GetEnvironmentVariable("MONGODB")));
                 services.AddScoped<IDigitalOceanClientFactory, DigitalOceanClientFactory>();
                 services.AddScoped<IPageFactory, PageFactory>();
                 services.AddScoped<ICheckListPageFactory, CheckListPageFactory>();
                 services.AddSingleton<ITelegramBotClient>(t => new TelegramBotClient(Environment.GetEnvironmentVariable("TELEGRAM")));
                 services.AddSingleton(b => RabbitHutch.CreateBus(Environment.GetEnvironmentVariable("RABBITMQ")).Advanced);
                 
                 services.AddLogging(logging =>
                 {
                     logging.AddSerilog(new LoggerConfiguration()
                                            .WriteTo.Console()
                                            .WriteTo.File($"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}//Logs//log.txt", shared: true, fileSizeLimitBytes: 50000000)
                                            .CreateLogger());
                 });
             });


            host.Start();
        }

        private static void BsonRegisterClassMap()
        {
            BsonClassMap.RegisterClassMap<Firewall>(cm =>
            {
                cm.AutoMap();
                cm.SetDiscriminator(typeof(Firewall).FullName);
            });
            
            BsonClassMap.RegisterClassMap<CreateDroplet>(cm =>
            {
                cm.AutoMap();
                cm.SetDiscriminator(typeof(CreateDroplet).FullName);
            });
        }
    }
}
