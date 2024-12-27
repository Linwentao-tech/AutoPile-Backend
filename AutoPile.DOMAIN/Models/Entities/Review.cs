using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPile.DOMAIN.Models.Entities
{
    public class Review
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }

        public string UserId { get; set; }
        public ObjectId ProductId { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Content { get; set; }
        public int Rating { get; set; }
        public byte[] Image { get; set; }
        public string ImageContentType { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}