using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace Blog.Models
{
    public class PostModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }

        [BsonElement("title")]
        [JsonPropertyName("title")]
        public string Title { get; set; } = null!;

        [BsonElement("description")]
        [JsonPropertyName("description")]
        public string Description { get; set; } = null!;

        [BsonElement("view")]
        [JsonPropertyName("view")]
        public int? View { get; set; } = null!;

        [BsonElement("like")]
        [JsonPropertyName("like")]
        public int? Like { get; set; } = null!;

        [BsonElement("image")]
        [JsonPropertyName("image")]
        public string? Image { get; set; } = null!;

        [BsonElement("userId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId UserId { get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; } = DateTime.Now;
    }
}