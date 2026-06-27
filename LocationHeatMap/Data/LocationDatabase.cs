using LocationHeatMap.Models;
using SQLite;

namespace LocationHeatMap.Data;

/// <summary>
/// Wraps an SQLite database used to persist recorded location points.
/// A single instance is registered as a singleton via dependency
/// injection so the underlying connection is reused throughout the
/// app's lifetime.
/// </summary>
public class LocationDatabase
{
    private SQLiteAsyncConnection? _connection;
    private readonly string _dbPath;
    private readonly SemaphoreSlim _initLock = new(1, 1);

    public LocationDatabase()
    {
        _dbPath = Path.Combine(FileSystem.AppDataDirectory, "locations.db3");
    }

    /// <summary>
    /// Lazily opens the connection and ensures the schema exists.
    /// Safe to call repeatedly; only initializes once.
    /// </summary>
    private async Task<SQLiteAsyncConnection> GetConnectionAsync()
    {
        if (_connection is not null)
            return _connection;

        await _initLock.WaitAsync();
        try
        {
            if (_connection is null)
            {
                var conn = new SQLiteAsyncConnection(_dbPath);
                await conn.CreateTableAsync<LocationPoint>();
                // Index speeds up date-range queries used for filtering the heat map.
                // NOTE: sqlite-net-pcl names the table after the class itself
                // ("LocationPoint", singular) — not the property name on this
                // class or any plural form, so the index DDL below must match exactly.
                await conn.ExecuteAsync(
                    "CREATE INDEX IF NOT EXISTS IX_LocationPoint_TimestampUtc ON LocationPoint(TimestampUtc);");
                _connection = conn;
            }
        }
        finally
        {
            _initLock.Release();
        }

        return _connection;
    }

    public async Task<int> InsertAsync(LocationPoint point)
    {
        var conn = await GetConnectionAsync();
        return await conn.InsertAsync(point);
    }

    public async Task<List<LocationPoint>> GetAllAsync()
    {
        var conn = await GetConnectionAsync();
        return await conn.Table<LocationPoint>()
                          .OrderBy(p => p.TimestampUtc)
                          .ToListAsync();
    }

    public async Task<List<LocationPoint>> GetSinceAsync(DateTime sinceUtc)
    {
        var conn = await GetConnectionAsync();
        return await conn.Table<LocationPoint>()
                          .Where(p => p.TimestampUtc >= sinceUtc)
                          .OrderBy(p => p.TimestampUtc)
                          .ToListAsync();
    }

    public async Task<int> GetCountAsync()
    {
        var conn = await GetConnectionAsync();
        return await conn.Table<LocationPoint>().CountAsync();
    }

    public async Task<int> ClearAllAsync()
    {
        var conn = await GetConnectionAsync();
        return await conn.DeleteAllAsync<LocationPoint>();
    }
}
