using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Requests = DigitalOcean.API.Models.Requests;
using Responses = DigitalOcean.API.Models.Responses;

namespace DigitalOceanBot.MongoDb.Models
{
    public class CreateDroplet
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        public Requests.Droplet Droplet { get; set; }

        public IReadOnlyList<Responses.Image> Images { get; set; }

        public IReadOnlyList<Responses.Region> Regions { get; set; }

        public IReadOnlyList<Responses.Size> Sizes { get; set; }
    }
}
