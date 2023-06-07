using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace Blog.Models
{
    public class UserModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }

        [BsonElement("username")]
        [JsonPropertyName("username")]
        public string username { get; set; } = null!;

        [BsonElement("email")]
        [JsonPropertyName("email")]
        public string email { get; set; } = null!;

        [BsonElement("password")]
        [JsonPropertyName("password")]
        public string password { get; set; } = null!;

        [BsonElement("image")]
        [JsonPropertyName("image")]
        public string image { get; set; } = null!;

        [BsonElement("verify_token")]
        [JsonPropertyName("verify_token")]
        public string? verify_token { get; set; } = "";

        [BsonElement("verify_code")]
        [JsonPropertyName("verify_code")]
        public int? verify_code { get; set; } = 0!;

        [BsonElement("limitLogin")]
        [JsonPropertyName("limitLogin")]
        public int? limitLogin { get; set; } = 0!;

        [BsonElement("countLogin")]
        [JsonPropertyName("countLogin")]
        public int? countLogin { get; set; } = 0!;

        public DateTime? createdAt { get; set; } = DateTime.Now;
        public DateTime? updatedAt { get; set; } = DateTime.Now;
    }
}
