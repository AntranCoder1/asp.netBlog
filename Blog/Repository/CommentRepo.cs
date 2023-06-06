using Blog.Config;
using Blog.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Blog.Repository
{
    public class CommentRepo
    {
        private readonly IMongoCollection<CommentModel> _commentCollection;
        public CommentRepo(IOptions<DatabaseSettings> DatabaseSettings)
        {
            var mongoClient = new MongoClient(DatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(DatabaseSettings.Value.DatabaseName);

            _commentCollection = mongoDatabase.GetCollection<CommentModel>("comments");
        }

        public async Task<List<CommentModel>> getComments(int limit, int page)
        {
            int skipCount = (page - 1) * limit;

            var sortOptions = new SortDefinitionBuilder<CommentModel>().Descending(comment => comment.CreatedAt);

            return await _commentCollection.Find(comment => true)
                                        .Skip(skipCount)
                                        .Limit(limit)
                                        .Sort(sortOptions)
                                        .ToListAsync();
        }

        public async Task createComment(CommentModel comment) => await _commentCollection.InsertOneAsync(comment);

        public async Task<CommentModel> findParentCommemt(string id)
        {
            return await _commentCollection.Find(c => c.PostId.ToString() == id).FirstOrDefaultAsync();
        }

        public async Task updateParentComment(string commentId, CommentModel comment)
        {
            var filter = Builders<CommentModel>.Filter.Eq(comment => comment.Id, new ObjectId(commentId));

            var update = Builders<CommentModel>.Update
                .Set(comment => comment.ParentComment, comment.ParentComment);

            await _commentCollection.UpdateOneAsync(filter, update);
        }

        public async Task updateComment(string commentId, CommentModel comment)
        {
            var filter = Builders<CommentModel>.Filter.Eq(comment => comment.Id, new ObjectId(commentId));

            var update = Builders<CommentModel>.Update
                .Set(comment => comment.Comment, comment.Comment);

            await _commentCollection.UpdateOneAsync(filter, update);
        }

        public async Task<CommentModel> GetCommentWithUserId(string userId, string postId)
        {
            try
            {
                var filter = Builders<CommentModel>.Filter.And(
                    Builders<CommentModel>.Filter.Eq(comment => comment.UserId, new ObjectId(userId)),
                    Builders<CommentModel>.Filter.Eq(comment => comment.PostId, new ObjectId(postId))
                );

                return await _commentCollection.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving the comment: {ex}");

                return null;
            }
        }

        public async Task RemoveComment(string id) => await _commentCollection.DeleteOneAsync(comment => comment.Id.ToString() == id);

        public async Task<CommentModel> GetCommentWithIdAndUserId(string commentId, string userId)
        {
            try
            {
                var filter = Builders<CommentModel>.Filter.And(
                    Builders<CommentModel>.Filter.Eq(comment => comment.Id, new ObjectId(commentId)),
                    Builders<CommentModel>.Filter.Eq(comment => comment.UserId, new ObjectId(userId))
                );

                return await _commentCollection.Find(filter).FirstOrDefaultAsync();

            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving the comment: {ex}");

                return null;
            }
        }

        public async Task<List<CommentModel>> FindCommentUser(string userId)
        {
            try
            {
                var pipeline = new BsonDocument[]
                {
            new BsonDocument("$match", new BsonDocument("userId", new BsonObjectId(new ObjectId(userId)))),

            new BsonDocument("$lookup", new BsonDocument
            {
                { "from", "posts" },
                { "localField", "postId" },
                { "foreignField", "_id" },
                { "as", "commentPosts" }
            }),

            new BsonDocument("$unwind", "$commentPosts"),

            new BsonDocument("$project", new BsonDocument
            {
                { "_id", 0 },
                { "userId", 1 },
                { "postId", 1 },
                { "comment", 1 },
                { "parentComment", 1 },
                { "createdAt", 1 },
                { "updatedAt", 1 },
                { "Post", new BsonDocument
                    {
                        { "_id", "$commentPosts._id" },
                        { "title", "$commentPosts.title" },
                        { "description", "$commentPosts.description" },
                        { "view", "$commentPosts.view" },
                        { "like", "$commentPosts.like" },
                        { "image", "$commentPosts.image" },
                    }
                }
            })
                };

                var result = await _commentCollection
                    .Aggregate<CommentModel>(pipeline)
                    .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving the comment: {ex}");

                return null;
            }
        }

        //public async Task<CommentModel> GetCommentChil(string commentChilId, string userId)
        //{
        //    try
        //    {
        //        var filter = Builders<CommentModel>.Filter.ElemMatch(comment => comment.ParentComment,
        //               comment => comment.Id == commentChilId && comment.UserId == new ObjectId(userId));

        //        return await _commentCollection.Find(filter).FirstOrDefaultAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"An error occurred while retrieving the comment: {ex}");

        //        return null;
        //    }
        //}

        //public async Task UpdateCommentChil(string userId, string commentChilId, string newComment, string commentId)
        //{
        //    var filter = Builders<CommentModel>.Filter.And(
        //        Builders<CommentModel>.Filter.ElemMatch(comment => comment.ParentComment,
        //            Builders<CommentParent>.Filter.And(
        //                Builders<CommentParent>.Filter.Eq(comment => comment.Id, commentChilId),
        //                Builders<CommentParent>.Filter.Eq(comment => comment.UserId, new ObjectId(userId))
        //            )
        //        ),
        //        Builders<CommentModel>.Filter.Eq("ParentComment.Id", commentChilId),
        //        Builders<CommentModel>.Filter.Eq(comment => comment.Id, new ObjectId(commentId))
        //    );

        //    var update = Builders<CommentModel>.Update.Set("ParentComment.$.Comment", newComment);

        //    await _commentCollection.UpdateOneAsync(filter, update);
        //}

        //public async Task DeleteCommentChil(string commentChilId, string userId)
        //{
        //    var filter = Builders<CommentModel>.Filter.ElemMatch(comment => comment.ParentComment, comment => comment.Id == commentChilId && comment.UserId == new ObjectId(userId));

        //    await _commentCollection.DeleteOneAsync(filter);
        //}

        //public async Task<List<PostModel>> findUserComment(string userId)
        //{
        //    var pipline = new BsonDocument[]
        //    {
        //        // Match stage to filter likes by userId
        //        new BsonDocument("$match", new BsonDocument("userId", new BsonObjectId(new ObjectId(userId)))),
        //    };
        //}
    }
}
