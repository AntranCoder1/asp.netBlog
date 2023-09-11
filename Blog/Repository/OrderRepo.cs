using Blog.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Blog.Repository
{
    public class OrderRepo
    {
        private readonly IMongoCollection<OrderModel> _orderCollection;
        private readonly IMongoCollection<UserModel> _userCollection;
        private readonly IMongoCollection<PostModel> _postCollection;

        public async Task<List<OrderModel>> FindOrders()
        {
            return await _orderCollection.Find(order => true).ToListAsync();
        }

        public async Task<OrderModel> FindOrderById(string id)
        {
            var filter = Builders<OrderModel>.Filter.Eq(user => user.Id, new ObjectId(id));

            return await _orderCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task RemoveOrder(string id) => await _orderCollection.DeleteOneAsync(order => order.Id.ToString() == id);
    }
}
