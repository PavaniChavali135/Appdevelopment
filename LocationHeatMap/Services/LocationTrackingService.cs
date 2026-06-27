using LocationHeatMap.Data;
using LocationHeatMap.Models;

namespace LocationHeatMap.Services;


public class LocationTrackingService
{
    private readonly LocationDatabase _database;
    private CancellationTokenSource? _cts;
    private bool _isTracking;

    /// <summary>How often to request a new GPS fix.</summary>
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(15);

    /// <summary>Raised on the calling thread whenever a new point is captured and saved.</summary>
    public event EventHandler<LocationPoint>? LocationCaptured;

    public bool IsTracking => _isTracking;

    public LocationTrackingService(LocationDatabase database)
    {
        _database = database;
    }

    /// <summary>
    /// Requests location permission from the user. Must be called (and
    /// granted) before StartTrackingAsync will succeed.
    /// </summary>
    public async Task<bool> RequestPermissionsAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.LocationAlways>();

        if (status != PermissionStatus.Granted)
            status = await Permissions.RequestAsync<Permissions.LocationAlways>();

        // Fall back to "while in use" if "always" (background) isn't granted —
        // the app will still track while open, just not in the background.
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        }

        return status == PermissionStatus.Granted;
    }

    /// <summary>
    /// Starts the polling loop on a background task. Each captured fix
    /// is immediately written to SQLite.
    /// </summary>
    public Task StartTrackingAsync()
    {
        if (_isTracking)
            return Task.CompletedTask;

        _cts = new CancellationTokenSource();
        _isTracking = true;

        _ = PollLoopAsync(_cts.Token);

        return Task.CompletedTask;
    }

    public void StopTracking()
    {
        _isTracking = false;
        _cts?.Cancel();
        _cts = null;
    }

    private async Task PollLoopAsync(CancellationToken token)
    {
        var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));

        while (!token.IsCancellationRequested)
        {
            try
            {
                Location? location = await Geolocation.Default.GetLocationAsync(request, token);

                if (location is not null)
                {
                    var point = new LocationPoint(
                        latitude: location.Latitude,
                        longitude: location.Longitude,
                        accuracy: location.Accuracy ?? 25.0,
                        timestampUtc: location.Timestamp.UtcDateTime,
                        speed: location.Speed);

                    await _database.InsertAsync(point);
                    LocationCaptured?.Invoke(this, point);
                }
            }
            catch (FeatureNotEnabledException)
            {
                // GPS/location services turned off at the OS level.
                // Surface this to the UI layer via the event with a null check upstream,
                // or extend with a dedicated error event as needed.
            }
            catch (PermissionException)
            {
                StopTracking();
                break;
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception)
            {
                // Swallow transient GPS errors (e.g. timeout) and retry on next interval.
            }

            try
            {
                await Task.Delay(PollingInterval, token);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }
}
