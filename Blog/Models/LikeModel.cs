using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Runtime.Serialization;

namespace Blog.Models
{
    public enum LikeType
    {
        [EnumMember(Value = "like")]
        Like,

        [EnumMember(Value = "dislike")]
        Dislike
    }

    public class LikeModel
    {

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }

        [BsonElement("userId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId UserId { get; set; }

        [BsonElement("postId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId PostId { get; set; }

        [BsonElement("type")]
        [BsonRepresentation(BsonType.String)]
        public LikeType Type { get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; } = DateTime.Now;
    }
}
