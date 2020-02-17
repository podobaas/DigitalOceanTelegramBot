using DigitalOceanBot.MongoDb.Models;
using MongoDB.Driver;
using System;

namespace DigitalOceanBot.MongoDb
{
    public class UserRepository : IRepository<DoUser>
    {
        private readonly MongoClient _mongoClient;
        private readonly IMongoDatabase _database;

        public UserRepository(string connectionString)
        {
            _mongoClient = new MongoClient(connectionString);
            _database = _mongoClient.GetDatabase("DigitalOceanBot");
        }

        public void Create(DoUser entity)
        {
            _database.GetCollection<DoUser>("Users").InsertOne(entity);
        }

        public DoUser Get(int userId)
        {
            return _database.GetCollection<DoUser>("Users")
                 .Find(u => u.UserId == userId)
                 .FirstOrDefault();
        }

        public void Update(int userId, Action<DoUser> action)
        {
            var user = _database.GetCollection<DoUser>("Users")
                        .Find(u => u.UserId == userId)
                        .FirstOrDefault();

            action(user);

            _database.GetCollection<DoUser>("Users").ReplaceOne(u => u.UserId == user.UserId, user);
        }

        public void Delete(int userId)
        {
            _database.GetCollection<HandlerCallback>("Users").DeleteOne(h => h.UserId == userId);
        }
    }
}
