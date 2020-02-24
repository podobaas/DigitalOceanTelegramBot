using DigitalOceanBot.MongoDb.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;

namespace DigitalOceanBot.MongoDb
{
    public class HandlerCallbackRepository : IRepository<HandlerCallback>
    {
        private readonly IMongoDatabase _database;

        public HandlerCallbackRepository(string connectionString)
        {
            _database = new MongoClient(connectionString).GetDatabase("DigitalOceanBot");
        }

        public IEnumerable<HandlerCallback> GetAll()
        {
            return _database.GetCollection<HandlerCallback>("HandlersCallback").FindSync(Builders<HandlerCallback>.Filter.Empty).ToList();
        }

        public void Create(HandlerCallback entity)
        {
            _database.GetCollection<HandlerCallback>("HandlersCallback").InsertOne(entity);
        }

        public HandlerCallback Get(int userId)
        {
            return _database.GetCollection<HandlerCallback>("HandlersCallback")
                 .Find(u => u.UserId == userId)
                 .FirstOrDefault();
        }

        public void Update(int userId, Action<HandlerCallback> action)
        {
            var handlerCallback = _database.GetCollection<HandlerCallback>("HandlersCallback")
                        .Find(u => u.UserId == userId)
                        .FirstOrDefault();

            action(handlerCallback);

            _database.GetCollection<HandlerCallback>("HandlersCallback").ReplaceOne(u => u.UserId == userId, handlerCallback);
        }

        public void Delete(int userId)
        {
            _database.GetCollection<HandlerCallback>("HandlersCallback").DeleteOne(h => h.UserId == userId);
        }
    }
}
