using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace Blog.Models
{
    public class BookingModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }

        [JsonPropertyName("title")]
        public string title { get; set; }

        [JsonPropertyName("description")]
        public string description { get; set; }

        [JsonPropertyName("img")]
        public List<string> img { get; set; }

        [JsonPropertyName("type")]
        public string type { get; set; }

        [JsonPropertyName("thumbnail")]
        public string thumbnail { get; set; }

        [JsonPropertyName("price")]
        public float price { get; set; }

        [JsonPropertyName("price_cupon")]
        public float price_cupon { get; set; }

        [JsonPropertyName("address")]
        public string address { get; set; }

        [JsonPropertyName("quantity_open")]
        public int quantity_open { get; set; } = 0;

        [JsonPropertyName("quantiry_closed")]
        public int quantity_closed { get; set; } = 0;

        public DateTime? createdAt { get; set; } = DateTime.Now;
        public DateTime? updatedAt { get; set; } = DateTime.Now;
    }
}
