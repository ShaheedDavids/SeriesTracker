using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using SeriesTracker.Models;

namespace SeriesTracker.Services
{
    public class MongoDbService
    {
        private readonly IMongoDatabase _database;

        public IMongoCollection<User> Users { get; }
        public IMongoCollection<WatchListEntry> WatchList { get; }

        public MongoDbService(IConfiguration configuration)
        {
            var connectionString = configuration["MongoDB:ConnectionString"];
            var databaseName = configuration["MongoDB:DatabaseName"];

            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);

            Users = _database.GetCollection<User>("Users");
            WatchList = _database.GetCollection<WatchListEntry>("WatchList");

            CreateIndexes();
        }

        private void CreateIndexes()
        {
            // Ensure username and email are unique
            var usernameIndex = new CreateIndexModel<User>(
                Builders<User>.IndexKeys.Ascending(u => u.Username),
                new CreateIndexOptions { Unique = true }
            );

            var emailIndex = new CreateIndexModel<User>(
                Builders<User>.IndexKeys.Ascending(u => u.Email),
                new CreateIndexOptions { Unique = true }
            );

            Users.Indexes.CreateMany([usernameIndex, emailIndex]);

            // Index WatchList by userId for fast per-user queries
            var userIdIndex = new CreateIndexModel<WatchListEntry>(
                Builders<WatchListEntry>.IndexKeys.Ascending(w => w.UserId)
            );

            WatchList.Indexes.CreateOne(userIdIndex);
        }
    }
}