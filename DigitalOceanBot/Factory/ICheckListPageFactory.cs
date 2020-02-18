using DigitalOceanBot.CheckLists;

namespace DigitalOceanBot.Factory
{
    public interface ICheckListPageFactory
    {
        ICheckListPage<TCollectionType> GetInstance<TCollectionType, TPageType>();
    }
}