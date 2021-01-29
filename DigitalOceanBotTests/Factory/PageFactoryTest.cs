using DigitalOceanBot.MongoDb;
using DigitalOceanBot.MongoDb.Models;

namespace DigitalOceanBotTests.Factory
{
    public class PageFactoryTest
    {
        private IRepository<Session> _sessionRepo;

        public PageFactoryTest()
        {
            InitTest();
        }

        private void InitTest()
        {
            _sessionRepo = Substitute.For<IRepository<Session>>();
        }
        
        [Fact]
        public void GetInstanceTest()
        {
            var pageFactory = new PageFactory(_sessionRepo);
            var result = pageFactory.GetInstance<DropletPage>();
            
            Assert.NotNull(result);
            Assert.IsType<DropletPage>(result);
            Assert.IsAssignableFrom<IPage>(result);
        }
    }
}