using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DigitalOceanBot.MongoDb.Models
{
    public class HandlerCallback
    {
        [BsonId]
        public int UserId { get; set; }
        
        [BsonRepresentation(BsonType.Int32)]
        public int MessageId { get; set; }

        [BsonRepresentation(BsonType.String)]
        public string HandlerType { get; set; }
    }
}
