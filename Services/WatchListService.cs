using MongoDB.Driver;
using SeriesTracker.Models;

namespace SeriesTracker.Services
{
    public class WatchListService
    {
        private readonly MongoDbService _mongoDb;

        public WatchListService(MongoDbService mongoDb)
        {
            _mongoDb = mongoDb;
        }

        // Get all entries for a specific user
        public async Task<List<WatchListEntry>> GetUserWatchListAsync(string userId)
        {
            return await _mongoDb.WatchList
                .Find(w => w.UserId == userId)
                .SortBy(w => w.ShowName)
                .ToListAsync();
        }

        // Add a new show to the watchlist
        public async Task<(bool Success, string Message)> AddEntryAsync(WatchListEntry entry)
        {
            if (string.IsNullOrWhiteSpace(entry.ShowName))
                return (false, "Show name is required.");

            if (entry.CurrentEpisode < 1)
                return (false, "Episode number must be at least 1.");

            // Check for duplicate show name for this user
            var existing = await _mongoDb.WatchList
                .Find(w => w.UserId == entry.UserId &&
                           w.ShowName.ToLower() == entry.ShowName.ToLower())
                .FirstOrDefaultAsync();

            if (existing != null)
                return (false, "This show is already in your watchlist.");

            entry.DateAdded = DateTime.UtcNow;
            entry.LastUpdated = DateTime.UtcNow;

            await _mongoDb.WatchList.InsertOneAsync(entry);
            return (true, "Show added successfully!");
        }

        // Update an existing entry
        public async Task<(bool Success, string Message)> UpdateEntryAsync(WatchListEntry entry)
        {
            if (string.IsNullOrWhiteSpace(entry.ShowName))
                return (false, "Show name is required.");

            if (entry.CurrentEpisode < 1)
                return (false, "Episode number must be at least 1.");

            entry.LastUpdated = DateTime.UtcNow;

            var update = Builders<WatchListEntry>.Update
                .Set(w => w.ShowName, entry.ShowName)
                .Set(w => w.TotalSeasons, entry.TotalSeasons)
                .Set(w => w.IsOngoing, entry.IsOngoing)
                .Set(w => w.CurrentSeason, entry.CurrentSeason)
                .Set(w => w.CurrentEpisode, entry.CurrentEpisode)
                .Set(w => w.WatchStatus, entry.WatchStatus)
                .Set(w => w.LastUpdated, entry.LastUpdated);

            var result = await _mongoDb.WatchList
                .UpdateOneAsync(w => w.Id == entry.Id, update);

            return result.ModifiedCount > 0
                ? (true, "Entry updated successfully!")
                : (false, "Entry not found or no changes made.");
        }

        // Delete a single entry
        public async Task<(bool Success, string Message)> DeleteEntryAsync(string entryId)
        {
            var result = await _mongoDb.WatchList
                .DeleteOneAsync(w => w.Id == entryId);

            return result.DeletedCount > 0
                ? (true, "Show removed from watchlist.")
                : (false, "Entry not found.");
        }

        // Quick episode increment without reloading full entry
        public async Task IncrementEpisodeAsync(string entryId)
        {
            var update = Builders<WatchListEntry>.Update
                .Inc(w => w.CurrentEpisode, 1)
                .Set(w => w.LastUpdated, DateTime.UtcNow);

            await _mongoDb.WatchList
                .UpdateOneAsync(w => w.Id == entryId, update);
        }

        // Quick episode decrement without reloading full entry
        public async Task DecrementEpisodeAsync(string entryId, int currentEpisode)
        {
            // Prevent going below episode 1
            if (currentEpisode <= 1) return;

            var update = Builders<WatchListEntry>.Update
                .Inc(w => w.CurrentEpisode, -1)
                .Set(w => w.LastUpdated, DateTime.UtcNow);

            await _mongoDb.WatchList
                .UpdateOneAsync(w => w.Id == entryId, update);
        }

        // Get counts per status for a summary dashboard
        public async Task<Dictionary<WatchStatus, int>> GetStatusSummaryAsync(string userId)
        {
            var entries = await GetUserWatchListAsync(userId);

            return entries
                .GroupBy(w => w.WatchStatus)
                .ToDictionary(g => g.Key, g => g.Count());
        }
    }
}