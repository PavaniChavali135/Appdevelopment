using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocationHeatMap.Data;
using LocationHeatMap.Models;
using LocationHeatMap.Services;
using Microsoft.Maui.Maps;

namespace LocationHeatMap.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    private readonly LocationDatabase _database;
    private readonly LocationTrackingService _trackingService;

    [ObservableProperty]
    private bool isTracking;

    [ObservableProperty]
    private string statusText = "Not tracking";

    [ObservableProperty]
    private int recordedPointCount;

    /// <summary>Backing collection for the heat map overlay; bound via the drawable, not directly to UI elements.</summary>
    public ObservableCollection<LocationPoint> Points { get; } = new();

    /// <summary>Raised whenever Points changes, so the page can invalidate the heat map GraphicsView.</summary>
    public event EventHandler? PointsUpdated;

    public MainPageViewModel(LocationDatabase database, LocationTrackingService trackingService)
    {
        _database = database;
        _trackingService = trackingService;
        _trackingService.LocationCaptured += OnLocationCaptured;
    }

    [RelayCommand]
    private async Task LoadPointsAsync()
    {
        var all = await _database.GetAllAsync();
        Points.Clear();
        foreach (var p in all)
            Points.Add(p);

        RecordedPointCount = Points.Count;
        PointsUpdated?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private async Task ToggleTrackingAsync()
    {
        if (IsTracking)
        {
            _trackingService.StopTracking();
            IsTracking = false;
            StatusText = $"Stopped — {RecordedPointCount} points recorded";
            return;
        }

        var granted = await _trackingService.RequestPermissionsAsync();
        if (!granted)
        {
            StatusText = "Location permission denied";
            return;
        }

        await _trackingService.StartTrackingAsync();
        IsTracking = true;
        StatusText = "Tracking...";
    }

    [RelayCommand]
    private async Task ClearHistoryAsync()
    {
        await _database.ClearAllAsync();
        Points.Clear();
        RecordedPointCount = 0;
        StatusText = "History cleared";
        PointsUpdated?.Invoke(this, EventArgs.Empty);
    }

    private void OnLocationCaptured(object? sender, LocationPoint point)
    {
        // Marshal back to the UI thread since GetLocationAsync's continuation
        // may resume on a background thread depending on platform.
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Points.Add(point);
            RecordedPointCount = Points.Count;
            StatusText = $"Tracking... ({RecordedPointCount} points)";
            PointsUpdated?.Invoke(this, EventArgs.Empty);
        });
    }
}
