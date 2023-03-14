// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
namespace CommunityToolkit.Maui.Maps;

public static class AppHostBuilderExtensions
{
    /// <summary>
    /// Initializes the .NET MAUI Community Toolkit Maps Library.
    /// </summary>
    /// <param name="builder"><see cref="MauiAppBuilder"/> generated by <see cref="MauiApp"/>. </param>
    /// <param name="key">Bing Maps Key.</param>
    /// <returns><see cref="MauiAppBuilder"/>.</returns>
    public static MauiAppBuilder UseMauiCommunityToolkitMaps(this MauiAppBuilder builder, string key)
    {
        builder
        .ConfigureMauiHandlers(handlers =>
        {
#if WINDOWS
            Handlers.CustomMapHandler.MapsKey = key;
#endif
            handlers.AddMauiCommunityToolkitMaps();
        });

        return builder;
    }

    public static IMauiHandlersCollection AddMauiCommunityToolkitMaps(this IMauiHandlersCollection handlersCollection)
    {
#if WINDOWS
        handlersCollection.AddHandler<Microsoft.Maui.Controls.Maps.Map, CommunityToolkit.Maui.Maps.Handlers.CustomMapHandler>();
#endif
        return handlersCollection;
    }
}
