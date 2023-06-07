using Blog.Config;
using Blog.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Blog.Repository
{
    public class UserRepo
    {
        private readonly IMongoCollection<UserModel> _userCollection;

        public UserRepo(IOptions<DatabaseSettings> DatabaseSettings)
        {
            var mongoClient = new MongoClient(DatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(DatabaseSettings.Value.DatabaseName);

            _userCollection = mongoDatabase.GetCollection<UserModel>("users");
        }

        public async Task<List<UserModel>> GetUsers()
        {
            return await _userCollection.Find(user => true).ToListAsync();
        }

        public async Task<UserModel?> GetUser(string id) => await _userCollection.Find(user => user.Id.ToString() == id).FirstOrDefaultAsync();

        public async Task createUser(UserModel user) => await _userCollection.InsertOneAsync(user);

        public async Task updateUser(string id, UserModel updateUser)
        {
            var filter = Builders<UserModel>.Filter.Eq(user => user.Id, new ObjectId(id));

            var update = Builders<UserModel>.Update
                .Set(user => user.username, updateUser.username)
                .Set(user => user.email, updateUser.email)
                .Set(user => user.password, BCrypt.Net.BCrypt.HashPassword(updateUser.password))
                .Set(user => user.image, updateUser.image);

            await _userCollection.UpdateOneAsync(filter, update);
        }

        public async Task RemoveUser(string id) => await _userCollection.DeleteOneAsync(user => user.Id.ToString() == id);

        public async Task<UserModel> findUserWithEmail(string email)
        {
            var user = await _userCollection.Find(user => user.email == email).FirstOrDefaultAsync();

            return user;
        }

        public async Task<UserModel> getGovermentUser(string userId)
        {
            var filter = Builders<UserModel>.Filter.Eq(user => user.Id, new ObjectId(userId));

            return await _userCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task updateVerify(string email, UserModel updateUser)
        {
            var filter = Builders<UserModel>.Filter.Eq(user => user.email, email);

            var update = Builders<UserModel>.Update
                .Set(user => user.verify_code, updateUser.verify_code)
                .Set(user => user.verify_token, updateUser.verify_token);

            await _userCollection.UpdateOneAsync(filter, update);
        }

        public async Task<UserModel> getUserWithEmail(string email)
        {
            var filter = Builders<UserModel>.Filter.Eq(user => user.email, email);

            return await _userCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<UserModel> getUserWithVerifyCodeAndToken(int verifyCode, string verifyToken)
        {
            var filter = Builders<UserModel>.Filter.And(
                Builders<UserModel>.Filter.Eq(user => user.verify_code, verifyCode),
                Builders<UserModel>.Filter.Eq(user => user.verify_token, verifyToken)
            );

            return await _userCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task updateLimitLogin(string userId)
        {
            var filter = Builders<UserModel>.Filter.Eq(user => user.Id, new ObjectId(userId));

            var updateDefinition = Builders<UserModel>.Update.Inc(x => x.limitLogin, 1);

            await _userCollection.UpdateOneAsync(filter, updateDefinition);
        }

        public async Task updateLimitLoginZero(string userId)
        {
            var filter = Builders<UserModel>.Filter.Eq(user => user.Id, new ObjectId(userId));

            var updateDefinition = Builders<UserModel>.Update.Set(user => user.limitLogin, 0);

            await _userCollection.UpdateOneAsync(filter, updateDefinition);
        }

        public async Task updateCoutLogin(string userId)
        {
            var filter = Builders<UserModel>.Filter.Eq(user => user.Id, new ObjectId(userId));

            var updateDefinition = Builders<UserModel>.Update.Set(user => user.countLogin, 1);

            await _userCollection.UpdateOneAsync(filter, updateDefinition);
        }
    }
}
