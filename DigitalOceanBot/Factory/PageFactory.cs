using System;
using DigitalOceanBot.MongoDb;
using DigitalOceanBot.MongoDb.Models;
using DigitalOceanBot.Pages;

namespace DigitalOceanBot.Factory
{
    public class PageFactory : IPageFactory
    {
        private readonly IRepository<Session> _sessionRepo;

        public PageFactory(IRepository<Session> sessionRepo)
        {
            _sessionRepo = sessionRepo;
        }

        public IPage GetInstance<T>() where T : IPage
        {
            return (IPage)Activator.CreateInstance(typeof(T), _sessionRepo);
        }

        public IPage GetInstance<T>(object param1) where T : IPage
        {
            return (IPage)Activator.CreateInstance(typeof(T), _sessionRepo, param1);
        }
    }
}
