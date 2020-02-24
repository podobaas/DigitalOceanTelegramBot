using DigitalOcean.API;
using DigitalOceanBot.Factory;
using DigitalOceanBot.MongoDb;
using DigitalOceanBot.MongoDb.Models;
using NSubstitute;
using Xunit;

namespace DigitalOceanBotTests.Factory
{
    public class DigitalOceanClientFactoryTest
    {
        private IRepository<DoUser> _userRepo;

        public DigitalOceanClientFactoryTest()
        {
            InitTest();
        }

        private void InitTest()
        {
            _userRepo = Substitute.For<IRepository<DoUser>>();

            _userRepo.Get(Arg.Any<int>()).Returns(new DoUser
            {
                UserId = 100,
                Token = "token"
            });
        }
        
        [Fact]
        public void GetInstanceTest()
        {
            var digitalOceanApiFactory = new DigitalOceanClientFactory(_userRepo);
            var result = digitalOceanApiFactory.GetInstance(100);

            _userRepo.Received().Get(Arg.Is<int>(i => i == 100));
            Assert.NotNull(result);
            Assert.IsType<DigitalOceanClient>(result);
            Assert.IsAssignableFrom<IDigitalOceanClient>(result);
        }
    }
}