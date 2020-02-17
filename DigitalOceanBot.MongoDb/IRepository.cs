using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

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
