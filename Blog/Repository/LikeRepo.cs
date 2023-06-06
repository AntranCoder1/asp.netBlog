using Blog.Config;
using Blog.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Blog.Repository
{
    public class LikeRepo
    {
        private readonly IMongoCollection<LikeModel> _likeCollection;
        private readonly IMongoCollection<UserModel> _userCollection;
        private readonly IMongoCollection<PostModel> _postCollection;

        public LikeRepo(IOptions<DatabaseSettings> DatabaseSettings)
        {
            var mongoClient = new MongoClient(DatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(DatabaseSettings.Value.DatabaseName);

            _likeCollection = mongoDatabase.GetCollection<LikeModel>("like");
        }

        public async Task createLike(LikeModel like) => await _likeCollection.InsertOneAsync(like);

        public async Task<LikeModel> findLikeWithUsrIdAndPostId(string userId, string postId)
        {
            return await _likeCollection.Find(p => p.UserId.ToString() == userId && p.PostId.ToString() == postId).FirstOrDefaultAsync();
        }

        public async Task RemoveLike(string id) => await _likeCollection.DeleteOneAsync(like => like.Id.ToString() == id);

        public async Task<List<PostModel>> GetLikedPosts(string userId)
        {
            var pipeline = new BsonDocument[]
            {
                // Match stage to filter likes by userId
                new BsonDocument("$match", new BsonDocument("userId", new BsonObjectId(new ObjectId(userId)))),

                // Lookup stage to join likes with posts
                new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", "posts" },  // Name of the posts collection
                    { "localField", "postId" },  // Field in likes collection
                    { "foreignField", "_id" },  // Field in posts collection
                    { "as", "likedPosts" }  // Output array field name
                }),

                // Unwind stage to deconstruct the likedPosts array
                new BsonDocument("$unwind", "$likedPosts"),

                // Replace the root document with the merged fields
                new BsonDocument("$replaceRoot", new BsonDocument
                {
                    { "newRoot", "$likedPosts" }
                })
            };

            var result = await _likeCollection
                .Aggregate<PostModel>(pipeline)
                .ToListAsync();

            return result;
        }
    }
}
