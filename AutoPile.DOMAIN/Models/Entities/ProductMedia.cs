using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPile.DOMAIN.Models.Entities
{
    public class ProductMedia
    {
        [BsonElement("url")]
        [BsonRequired]
        public string Url { get; set; }

        [BsonElement("fullUrl")]
        [BsonRequired]
        public string FullUrl { get; set; }

        [BsonElement("mediaType")]
        [BsonRequired]
        public string MediaType { get; set; }

        [BsonElement("altText")]
        [BsonRequired]
        public string AltText { get; set; }

        [BsonElement("title")]
        [BsonRequired]
        public string Title { get; set; }

        [BsonElement("width")]
        [BsonRequired]
        public int Width { get; set; }

        [BsonElement("height")]
        [BsonRequired]
        public int Height { get; set; }
    }
}