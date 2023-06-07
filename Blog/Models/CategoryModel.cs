using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Blog.Models
{
    public class CategoryModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("parent")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Parent { get; set; }

        [BsonElement("ancestors")]
        public List<Ancestor> Ancestors { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        //[BsonIgnoreIfNull]
        //[BsonElement("children")]
        //[BsonRepresentation(BsonType.Document)]
        //public List<CategoryModel> Children { get; set; }
    }

    public class Ancestor
    {
        [BsonElement("_id")]
        public string Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }
    }
}
