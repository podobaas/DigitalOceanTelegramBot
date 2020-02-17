using System;
using DigitalOceanBot.MongoDb;
using DigitalOceanBot.MongoDb.Models;
using EasyNetQ;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DigitalOceanAuth
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseKestrel();
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureServices(services =>
                {
                    services.AddControllersWithViews();
                    services.AddScoped(b => RabbitHutch.CreateBus(Environment.GetEnvironmentVariable("RABBITMQ")));
                    services.AddScoped<IRepository<DoUser>>(r => new UserRepository(Environment.GetEnvironmentVariable("MONGODB")));
                });

            host.Build().Run();
        }
    }
}
