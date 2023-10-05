using Blog.Config;
using Blog.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Blog.Repository
{
    public class OrderRepo
    {
        private readonly IMongoCollection<OrderModel> _orderCollection;
        private readonly IMongoCollection<UserModel> _userCollection;
        private readonly IMongoCollection<PostModel> _postCollection;

        public OrderRepo(IOptions<DatabaseSettings> DatabaseSettings)
        {
            var mongoClient = new MongoClient(DatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(DatabaseSettings.Value.DatabaseName);

            _orderCollection = mongoDatabase.GetCollection<OrderModel>("orders");
        }

        public async Task<List<OrderModel>> FindOrders(int limit, int offset)
        {
            int skipCount = (offset - 1) * limit;

            var sortOptions = new SortDefinitionBuilder<OrderModel>().Descending(order => order.createdAt);

            return await _orderCollection.Find(order => true).Skip(skipCount).Limit(limit).Sort(sortOptions).ToListAsync();
        }

        public async Task<List<OrderModel>> FindOrderByUserId(string userId, int limit, int offset)
        {
            // int skipCount = (offset - 1) * limit;
            int skipCount = offset * limit;

            // var sortOptions = new SortDefinitionBuilder<OrderModel>().Descending(order => order.createdAt);

            // return await _orderCollection.Find(order => order.UserId.ToString() == userId)
            //                              .Skip(skipCount)
            //                              .Limit(limit)
            //                              .Sort(sortOptions)
            //                              .FirstOrDefaultAsync();

            try
            {
                var pipeline = new BsonDocument[]
                {
                    new BsonDocument("$match", new BsonDocument("UserId", new BsonObjectId(new ObjectId(userId)))),
                    new BsonDocument("$lookup", new BsonDocument
                    {
                        { "from", "booking" },
                        { "localField", "BookingId" },
                        { "foreignField", "_id" },
                        { "as", "booking" }
                    }),

                    new BsonDocument("$skip", skipCount),
                    new BsonDocument("$limit", limit),
                };

                var result = await _orderCollection.Aggregate<OrderModel>(pipeline).ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving the order: {ex}");
                return null;
            }
        }

        public async Task<OrderModel> FindOrderById(string orderId)
        {
            // return await _orderCollection.Find(order => order.Id.ToString() == orderId).FirstOrDefaultAsync();
            try
            {
                var pipeline = new BsonDocument[]
                {
                    new BsonDocument("$match", new BsonDocument("_id", new BsonObjectId(new ObjectId(orderId)))),
                    new BsonDocument("$lookup", new BsonDocument
                    {
                        { "from", "booking" },
                        { "localField", "BookingId" },
                        { "foreignField", "_id" },
                        { "as", "booking" }
                    }),
                    new BsonDocument("$lookup", new BsonDocument
                    {
                        { "from", "users" },
                        { "localField", "UserId" },
                        { "foreignField", "_id" },
                        { "as", "user" }
                    }),
                };

                var result = await _orderCollection.Aggregate<OrderModel>(pipeline).FirstOrDefaultAsync();

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving the order: {ex}");
                return null;
            }
        }

        public async Task RemoveOrder(string id, string userId) => await _orderCollection.DeleteOneAsync(order => order.Id.ToString() == id && order.UserId.ToString() == userId);

        public async Task createOrder(OrderModel order) => await _orderCollection.InsertOneAsync(order);

        public async Task<OrderModel> findOrderWithUserIdAndOrderId(string orderId, string userId)
        {
            return await _orderCollection.Find(order => order.Id.ToString() == orderId && order.UserId.ToString() == userId).FirstOrDefaultAsync();
        }

        public async Task<OrderModel> UpdateOrder(string orderId, OrderModel orderData)
        {
            var filter = Builders<OrderModel>.Filter.Eq(order => order.Id, new ObjectId(orderId));

            var update = Builders<OrderModel>.Update
                .Set(order => order.BookingId, orderData.BookingId)
                .Set(order => order.noted, orderData.noted);

            var updateResult = await _orderCollection.UpdateOneAsync(filter, update);

            if (updateResult.ModifiedCount > 0)
            {
                return orderData;
            }
            else
            {
                return null;
            }
        }

    }
}
