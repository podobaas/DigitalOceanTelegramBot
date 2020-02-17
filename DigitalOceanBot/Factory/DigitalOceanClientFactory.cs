using DigitalOcean.API;
using DigitalOceanBot.MongoDb;
using DigitalOceanBot.MongoDb.Models;

namespace DigitalOceanBot.Factory
{
    internal class DigitalOceanClientFactory : IDigitalOceanClientFactory
    {
        private readonly IRepository<DoUser> _userRepo;

        public DigitalOceanClientFactory(IRepository<DoUser> userRepo)
        {
            _userRepo = userRepo;
        }

        public IDigitalOceanClient GetInstance(int userId)
        {
            var user = _userRepo.Get(userId);
            return new DigitalOceanClient(user.UserInfo.access_token);
        }
    }
}
