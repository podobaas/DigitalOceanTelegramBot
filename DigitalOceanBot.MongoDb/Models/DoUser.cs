using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DigitalOceanBot.MongoDb.Models
{ 
    public class DoUser
    {
        [BsonId]
        public int UserId { get; set; }

        [BsonRepresentation(BsonType.String)]
        public string State { get; set; }

        [BsonRepresentation(BsonType.DateTime)]
        public DateTime TokenExpires { get; set; }

        [BsonRepresentation(BsonType.String)]
        public string Token { get; set; }
        
        [BsonRepresentation(BsonType.Boolean)]
        public bool IsAuthorized { get; set; }
    }
}
