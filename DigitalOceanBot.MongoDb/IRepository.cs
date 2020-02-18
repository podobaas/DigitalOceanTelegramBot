using System;

namespace DigitalOceanBot.MongoDb
{
    public interface IRepository<T> where T : class
    {
        T Get(int id);

        void Create(T entity);

        void Update(int id, Action<T> func);
        
        void Delete(int id);
    }
}
