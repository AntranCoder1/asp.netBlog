using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Blog.Models
{
    public class OrderModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }

        [BsonElement("userId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId UserId { get; set; }

        [BsonElement("bookingId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId BookingId { get; set; }

        public string noted { get; set; }

        public DateTime? createdAt { get; set; } = DateTime.Now;
        public DateTime? updatedAt { get; set; } = DateTime.Now;
    }
}
