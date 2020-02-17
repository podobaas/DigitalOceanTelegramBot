using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DigitalOceanBot.MongoDb.Models
{
    public class Session
    {
        [BsonId]
        public int UserId { get; set; }

        [BsonRepresentation(BsonType.Int64)]
        public long ChatId { get; set; }

        public SessionState State { get; set; }
        
        public object Data { get; set; }
    }
}
