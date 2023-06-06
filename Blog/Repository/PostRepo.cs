using Blog.Config;
using Blog.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Blog.Repository
{
    public class PostRepo
    {
        private readonly IMongoCollection<PostModel> _postCollection;
        private readonly IMongoCollection<UserModel> _userCollection;
        private readonly UserRepo _userRepo;

        public PostRepo(IOptions<DatabaseSettings> DatabaseSettings)
        {
            var mongoClient = new MongoClient(DatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(DatabaseSettings.Value.DatabaseName);

            _postCollection = mongoDatabase.GetCollection<PostModel>("posts");
        }

        public async Task createPost(PostModel post) => await _postCollection.InsertOneAsync(post);

        public async Task<List<PostModel>> GetPostsPagination(int limit, int page)
        {
            int skipCount = (page - 1) * limit;

            var sortOptions = new SortDefinitionBuilder<PostModel>().Descending(post => post.CreatedAt);

            return await _postCollection.Find(post => true)
                                        .Skip(skipCount)
                                        .Limit(limit)
                                        .Sort(sortOptions)
                                        .ToListAsync();
        }

        public async Task<List<PostModel>> GetPosts()
        {
            return await _postCollection.Find(post => true).ToListAsync();
        }

        public async Task<PostModel> GetPost(string id)
        {
            return await _postCollection.Find(post => post.Id.ToString() == id).FirstOrDefaultAsync();
        }

        public async Task RemovePost(string id) => await _postCollection.DeleteOneAsync(post => post.Id.ToString() == id);

        public async Task updatePost(string id, PostModel post)
        {
            var filter = Builders<PostModel>.Filter.Eq(post => post.Id, new ObjectId(id));

            var update = Builders<PostModel>.Update
                .Set(post => post.Title, post.Title)
                .Set(post => post.Description, post.Description)
                .Set(post => post.View, post.View)
                .Set(post => post.Like, post.Like)
                .Set(post => post.Image, post.Image);

            await _postCollection.UpdateOneAsync(filter, update);
        }

        public async Task LikePost(string postId)
        {
            var filter = Builders<PostModel>.Filter.Eq(post => post.Id, new ObjectId(postId));

            var findPost = await GetPost(postId);

            var updateDefinition = Builders<PostModel>.Update.Inc(x => x.Like, 1);

            await _postCollection.UpdateOneAsync(filter, updateDefinition);
        }

        public async Task DislikePost(string postId)
        {
            var filter = Builders<PostModel>.Filter.Eq(post => post.Id, new ObjectId(postId));

            var updateDefinition = Builders<PostModel>.Update.Inc(x => x.Like, -1);

            await _postCollection.UpdateOneAsync(filter, updateDefinition);
        }
    }
}
