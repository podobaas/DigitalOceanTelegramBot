using System;
using DigitalOceanBot.CheckLists;

namespace DigitalOceanBot.Factory
{
    public class CheckListPageFactory : ICheckListPageFactory
    {
        public ICheckListPage<TCollectionType> GetInstance<TCollectionType, TPageType>()
        {
            return (ICheckListPage<TCollectionType>)Activator.CreateInstance(typeof(TPageType));
        }
    }
}