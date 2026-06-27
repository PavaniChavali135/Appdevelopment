using LocationHeatMap.Models;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Maps;

namespace LocationHeatMap.Services;


public class HeatMapDrawable : IDrawable
{
    /// The points to render.
    public IReadOnlyList<LocationPoint> Points { get; set; } = Array.Empty<LocationPoint>();

    /// Current visible map region (center + degrees span). Set by the page on VisibleRegion change / size change.
    public MapSpan? VisibleRegion { get; set; }

    /// Radius, in pixels, of each rendered dot.
    public float DotRadius { get; set; } = 7f;

    /// Fill color of each dot. Defaults to the solid blue used by Google Maps location-history trails.
    public Color DotColor { get; set; } = Color.FromArgb("#1A73E8");

    /// Border color drawn around each dot for contrast against the map.
    public Color BorderColor { get; set; } = Colors.White;

    /// Border thickness in pixels.
    public float BorderWidth { get; set; } = 1.5f;

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        System.Diagnostics.Debug.WriteLine(
            $"[HeatMapDrawable] Draw called. Points={Points.Count}, VisibleRegion={(VisibleRegion is null ? "null" : "set")}, dirtyRect={dirtyRect}");

        if (Points.Count == 0 || VisibleRegion is null)
            return;

        MapSpan region = VisibleRegion;

        canvas.StrokeColor = BorderColor;
        canvas.StrokeSize = BorderWidth;
        canvas.FillColor = DotColor;

        int drawnCount = 0;
        foreach (var p in Points)
        {
            if (!IsInsideRegion(p, region))
                continue;

            var (x, y) = ProjectToScreen(p.Latitude, p.Longitude, region, dirtyRect.Width, dirtyRect.Height);

            var rect = new RectF(x - DotRadius, y - DotRadius, DotRadius * 2, DotRadius * 2);
            canvas.FillEllipse(rect);
            canvas.DrawEllipse(rect);
            drawnCount++;
        }

        System.Diagnostics.Debug.WriteLine($"[HeatMapDrawable] Drew {drawnCount} of {Points.Count} points inside region.");
    }

    private static bool IsInsideRegion(LocationPoint p, MapSpan region)
    {
        double halfLat = region.LatitudeDegrees / 2.0;
        double halfLon = region.LongitudeDegrees / 2.0;

        bool inside = p.Latitude >= region.Center.Latitude - halfLat
            && p.Latitude <= region.Center.Latitude + halfLat
            && p.Longitude >= region.Center.Longitude - halfLon
            && p.Longitude <= region.Center.Longitude + halfLon;

        System.Diagnostics.Debug.WriteLine(
            $"[HeatMapDrawable] Point({p.Latitude:F6},{p.Longitude:F6}) vs " +
            $"RegionCenter({region.Center.Latitude:F6},{region.Center.Longitude:F6}) " +
            $"Span(lat={region.LatitudeDegrees:F6},lon={region.LongitudeDegrees:F6}) => inside={inside}");

        return inside;
    }


    private static (float X, float Y) ProjectToScreen(double lat, double lon, MapSpan region, float width, float height)
    {
        double minLat = region.Center.Latitude - region.LatitudeDegrees / 2.0;
        double maxLat = region.Center.Latitude + region.LatitudeDegrees / 2.0;
        double minLon = region.Center.Longitude - region.LongitudeDegrees / 2.0;
        double maxLon = region.Center.Longitude + region.LongitudeDegrees / 2.0;

        double xRatio = (lon - minLon) / (maxLon - minLon);
        // Latitude increases upward but screen Y increases downward, so invert.
        double yRatio = 1.0 - (lat - minLat) / (maxLat - minLat);

        float x = (float)(xRatio * width);
        float y = (float)(yRatio * height);
        return (x, y);
    }
}
