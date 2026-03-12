using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SeriesTracker.Models
{
    public enum WatchStatus
    {
        Watching,
        Completed,
        Dropped,
        PlanToWatch,
        OnHold
    }

    public class WatchListEntry
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("userId")]
        public string UserId { get; set; } = string.Empty;

        [BsonElement("showName")]
        public string ShowName { get; set; } = string.Empty;

        // Total number of seasons the show has, "-" if ongoing/single continuation
        [BsonElement("totalSeasons")]
        public string TotalSeasons { get; set; } = "-";

        [BsonElement("isOngoing")]
        public bool IsOngoing { get; set; }

        // The season the user is currently on, "-" if not applicable
        [BsonElement("currentSeason")]
        public string CurrentSeason { get; set; } = "-";

        [BsonElement("currentEpisode")]
        public int CurrentEpisode { get; set; } = 1;

        [BsonElement("watchStatus")]
        [BsonRepresentation(BsonType.String)]
        public WatchStatus WatchStatus { get; set; } = WatchStatus.Watching;

        [BsonElement("dateAdded")]
        public DateTime DateAdded { get; set; } = DateTime.UtcNow;

        [BsonElement("lastUpdated")]
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}