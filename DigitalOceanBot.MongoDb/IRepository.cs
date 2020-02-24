using System;
using System.Collections.Generic;

namespace DigitalOceanBot.MongoDb
{
    public interface IRepository<T> where T : class
    {
        T Get(int id);
        
        IEnumerable<T> GetAll();

        void Create(T entity);

        void Update(int id, Action<T> func);
        
        void Delete(int id);
    }
}
