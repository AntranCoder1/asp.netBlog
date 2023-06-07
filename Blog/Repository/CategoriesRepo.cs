using Blog.Config;
using Blog.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Blog.Repository
{
    public class CategoriesRepo
    {
        private readonly IMongoCollection<CategoryModel> _categoriesCollection;

        public CategoriesRepo(IOptions<DatabaseSettings> DatabaseSettings)
        {
            var mongoClient = new MongoClient(DatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(DatabaseSettings.Value.DatabaseName);

            _categoriesCollection = mongoDatabase.GetCollection<CategoryModel>("categories");
        }

        public async Task<CategoryModel> findParent(string parentId)
        {
            var filter = Builders<CategoryModel>.Filter.Eq(category => category.Id, parentId);

            return await _categoriesCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task createCategory(CategoryModel category)
        {
            await _categoriesCollection.InsertOneAsync(category);
        }

        public async Task<List<CategoryModel>> getCategories()
        {
            return await _categoriesCollection.Find(category => true).ToListAsync();
        }

        //    public async Task<List<CategoryModel>> getCategoriesWithMPM()
        //    {
        //        var pipeline = new[]
        //{
        //    new BsonDocument("$graphLookup",
        //        new BsonDocument
        //        {
        //            { "from", "categories" },
        //            { "startWith", "$_id" },
        //            { "connectFromField", "_id" },
        //            { "connectToField", "parent" },
        //            { "as", "children" }
        //        }
        //    ),

        //    new BsonDocument("$addFields",
        //        new BsonDocument
        //        {
        //            { "chil", new BsonDocument("$map",
        //                new BsonDocument
        //                {
        //                    { "input", "$ancestors" },
        //                    { "as", "ancestor" },
        //                    { "in", new BsonDocument
        //                        {
        //                            { "_id", "$$ancestor._id" },
        //                            { "name", "$$ancestor.name" }
        //                        }
        //                    }
        //                }
        //            )},
        //            { "createdAt", new BsonDocument("$ifNull", new BsonArray { "$createdAt", BsonNull.Value }) },
        //            { "updatedAt", new BsonDocument("$ifNull", new BsonArray { "$updatedAt", BsonNull.Value }) }
        //        }
        //    ),

        //    new BsonDocument("$sort",
        //        new BsonDocument
        //        {
        //            { "level", 1 }
        //        }
        //    ),

        //    new BsonDocument("$group",
        //        new BsonDocument
        //        {
        //            { "_id", "$_id" },
        //            { "name", new BsonDocument("$first", "$name") },
        //            { "parent", new BsonDocument("$first", "$parent") },
        //            //{ "ancestors", new BsonDocument("$first", "$chil") },
        //            { "createdAt", new BsonDocument("$first", "$createdAt") },
        //            { "updatedAt", new BsonDocument("$first", "$updatedAt") },
        //            //{ "children", new BsonDocument("$push", "$children") }
        //        }
        //    )
        //};

        //        var cursor = await _categoriesCollection.AggregateAsync<CategoryModel>(pipeline);

        //        var result = await cursor.ToListAsync();

        //        return result;
        //    }

    }
}
