using DigitalOceanBot.MongoDb.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;

namespace DigitalOceanBot.MongoDb
{
    public class UserRepository : IRepository<DoUser>
    {
        private readonly IMongoDatabase _database;

        public UserRepository(string connectionString)
        {
            _database = new MongoClient(connectionString).GetDatabase("DigitalOceanBot");
        }

        public IEnumerable<DoUser> GetAll()
        {
            return _database.GetCollection<DoUser>("Users").FindSync(Builders<DoUser>.Filter.Empty).ToList();
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
            _database.GetCollection<DoUser>("Users").DeleteOne(h => h.UserId == userId);
        }
    }
}
