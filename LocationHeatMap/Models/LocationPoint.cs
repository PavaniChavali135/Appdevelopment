using SQLite;

namespace LocationHeatMap.Models;

/// <summary>
/// Represents a single recorded GPS fix. Each row in the SQLite
/// "LocationPoints" table maps to one instance of this class.
/// </summary>
public class LocationPoint
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    /// <summary>
    /// Horizontal accuracy in meters, as reported by the platform's
    /// location provider. Used to weight heat map intensity (a more
    /// accurate fix contributes a tighter, stronger "hot" point).
    /// </summary>
    public double Accuracy { get; set; }

    /// <summary>
    /// UTC timestamp the fix was captured.
    /// </summary>
    public DateTime TimestampUtc { get; set; }

    /// <summary>
    /// Optional speed in meters/second, if supplied by the platform.
    /// </summary>
    public double? Speed { get; set; }

    public LocationPoint()
    {
    }

    public LocationPoint(double latitude, double longitude, double accuracy, DateTime timestampUtc, double? speed = null)
    {
        Latitude = latitude;
        Longitude = longitude;
        Accuracy = accuracy;
        TimestampUtc = timestampUtc;
        Speed = speed;
    }
}
