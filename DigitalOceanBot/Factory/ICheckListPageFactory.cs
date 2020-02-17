using DigitalOceanBot.CheckLists;
using DigitalOceanBot.Pages;

namespace DigitalOceanBot.Factory
{
    public interface ICheckListPageFactory
    {
        ICheckListPage<TCollectionType> GetInstance<TCollectionType, TPageType>();
    }
}