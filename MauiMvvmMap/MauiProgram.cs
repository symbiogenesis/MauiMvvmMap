namespace MauiMvvmMap;

using CommunityToolkit.Maui;
#if WINDOWS
using CommunityToolkit.Maui.Maps;
#endif

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder()
            .UseMauiApp<App>()
#if DEBUG
            .UseMauiCommunityToolkit()
#else
            .UseMauiCommunityToolkit(options =>
            {
                options.SetShouldSuppressExceptionsInAnimations(true);
                options.SetShouldSuppressExceptionsInBehaviors(true);
                options.SetShouldSuppressExceptionsInConverters(true);
            })
#endif
            .UseMauiMaps()
#if WINDOWS
            .UseMauiCommunityToolkitMaps(Constants.BingMapsApiKey)
#endif
#if ANDROID || IOS
            .ConfigureMauiHandlers(handlers => handlers.AddHandler<Microsoft.Maui.Controls.Maps.Map, CustomMapHandler>())
#endif
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        return builder.Build();
    }
}
