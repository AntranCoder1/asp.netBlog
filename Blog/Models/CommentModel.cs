using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Blog.Models
{
    //public class CommentParent
    //{
    //    public string Id { get; set; }
    //    public ObjectId CommentId { get; set; }
    //    public ObjectId UserId { get; set; }
    //    public string Comment { get; set; }
    //}

    public class CommentModel
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

        [BsonElement("comment")]
        [BsonRepresentation(BsonType.String)]
        public string Comment { get; set; }

        [BsonElement("parentComment")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId? ParentComment { get; set; } = null!;

        [BsonElement("createdAt")]
        [BsonRepresentation(BsonType.DateTime)]
        public DateTime? CreatedAt { get; set; } = DateTime.Now;

        [BsonElement("updatedAt")]
        [BsonRepresentation(BsonType.DateTime)]
        public DateTime? UpdatedAt { get; set; } = DateTime.Now;

        public PostModel Post { get; set; } = null!;
    }
}
