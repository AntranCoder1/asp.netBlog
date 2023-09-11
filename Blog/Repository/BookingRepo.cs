﻿using Blog.Config;
using Blog.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Blog.Repository
{
    public class BookingRepo
    {
        private readonly IMongoCollection<BookingModel> _bookingCollection;

        public BookingRepo(IOptions<DatabaseSettings> DatabaseSettings)
        {
            var mongoClient = new MongoClient(DatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(DatabaseSettings.Value.DatabaseName);

            _bookingCollection = mongoDatabase.GetCollection<BookingModel>("booking");
        }

        public async Task createBooking(BookingModel booking) => await _bookingCollection.InsertOneAsync(booking);

        public async Task<List<BookingModel>> findAllBooking()
        {
            return await _bookingCollection.Find(booking => true).ToListAsync();
        }

        public async Task<BookingModel> findBookingWithId(string id)
        {
            return await _bookingCollection.Find(booking => booking.Id.ToString() == id).FirstOrDefaultAsync();
        }

        public async Task updateBooking(string id, BookingModel updateBooking)
        {
            var filter = Builders<BookingModel>.Filter.Eq(booking => booking.Id, new ObjectId(id));

            var update = Builders<BookingModel>.Update
                .Set(booking => booking.title, updateBooking.title)
                .Set(booking => booking.description, updateBooking.description)
                .Set(booking => booking.img, updateBooking.img)
                .Set(booking => booking.type, updateBooking.type)
                .Set(booking => booking.thumbnail, updateBooking.thumbnail)
                .Set(booking => booking.price, updateBooking.price)
                .Set(booking => booking.price_cupon, updateBooking.price_cupon)
                .Set(booking => booking.address, updateBooking.address);

            await _bookingCollection.UpdateOneAsync(filter, update);
        }

        public async Task RemoveBooking(string id) => await _bookingCollection.DeleteOneAsync(booking => booking.Id.ToString() == id);
    }
}