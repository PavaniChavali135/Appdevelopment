using LocationHeatMap.Data;
using LocationHeatMap.Services;
using LocationHeatMap.ViewModels;
using LocationHeatMap.Views;
using Microsoft.Extensions.Logging;

namespace LocationHeatMap;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiMaps();


        builder.Services.AddSingleton<LocationDatabase>();


        builder.Services.AddSingleton<LocationTrackingService>();


        builder.Services.AddSingleton<MainPageViewModel>();
        builder.Services.AddSingleton<MainPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
