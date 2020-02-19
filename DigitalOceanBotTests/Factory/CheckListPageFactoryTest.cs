using DigitalOcean.API.Models.Responses;
using DigitalOceanBot.CheckLists;
using DigitalOceanBot.Factory;
using Xunit;

namespace DigitalOceanBotTests.Factory
{
    public class CheckListPageFactoryTest
    {
        [Fact]
        public void GetInstanceTest()
        {
            var pageFactory = new CheckListPageFactory();
            var result = pageFactory.GetInstance<Droplet, DropletCheckList>();
            
            Assert.NotNull(result);
            Assert.IsType<DropletCheckList>(result);
            Assert.IsAssignableFrom<ICheckListPage<Droplet>>(result);
        }
    }
}