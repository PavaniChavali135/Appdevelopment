using LocationHeatMap.Services;
using LocationHeatMap.ViewModels;
using Microsoft.Maui.Maps;

namespace LocationHeatMap.Views;

public partial class MainPage : ContentPage
{
    private readonly MainPageViewModel _viewModel;
    private readonly HeatMapDrawable _heatMapDrawable = new();

    public MainPage(MainPageViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        BindingContext = _viewModel;

        HeatMapOverlay.Drawable = _heatMapDrawable;

        // Re-paint the heat map whenever the underlying point collection changes
        // (new GPS fix recorded, history loaded, or history cleared).
        _viewModel.PointsUpdated += (_, _) =>
        {
            _heatMapDrawable.Points = _viewModel.Points.ToList();
            HeatMapOverlay.Invalidate();
        };

        // Re-project the heat map every time the visible map region changes
        // (pan or zoom), since screen coordinates depend on the current viewport.
        MapView.MapClicked += (_, _) => { /* no-op placeholder for future tap-to-inspect feature */ };
        MapView.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(MapView.VisibleRegion))
            {
                _heatMapDrawable.VisibleRegion = MapView.VisibleRegion;
                HeatMapOverlay.Invalidate();
            }
        };

        SizeChanged += (_, _) => HeatMapOverlay.Invalidate();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadPointsCommand.ExecuteAsync(null);

        // Center the map on the user's last known location, falling back to a
        // sensible default region if no fix is available yet (e.g. first launch).
        if (_viewModel.Points.Count > 0)
        {
            var last = _viewModel.Points[^1];
            MapView.MoveToRegion(MapSpan.FromCenterAndRadius(
                new Location(last.Latitude, last.Longitude),
                Distance.FromKilometers(1)));
        }

        // The native map view initializes asynchronously, so VisibleRegion is
        // frequently still null immediately after OnAppearing runs — there is
        // no cross-platform "map ready" event to await instead, so poll briefly.
        await WaitForVisibleRegionAndDrawAsync();
    }

    private async Task WaitForVisibleRegionAndDrawAsync()
    {
        const int maxAttempts = 20; // ~5 seconds total at 250ms intervals
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            if (MapView.VisibleRegion is not null)
            {
                _heatMapDrawable.VisibleRegion = MapView.VisibleRegion;
                _heatMapDrawable.Points = _viewModel.Points.ToList();
                HeatMapOverlay.Invalidate();
                return;
            }

            await Task.Delay(250);
        }

        // Even if VisibleRegion never became available (e.g. emulator quirk),
        // still push the latest points through so PropertyChanged-driven
        // redraws later have correct data to work with.
        _heatMapDrawable.Points = _viewModel.Points.ToList();
    }
}
