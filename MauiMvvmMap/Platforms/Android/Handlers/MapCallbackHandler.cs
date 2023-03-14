namespace MauiMvvmMap.Platforms.Android.Handlers;

using global::Android.Gms.Maps;
using Microsoft.Maui.Maps.Handlers;
using IMap = Microsoft.Maui.Maps.IMap;

public class MapCallbackHandler : Java.Lang.Object, IOnMapReadyCallback
{
    private readonly IMapHandler mapHandler;

    public MapCallbackHandler(IMapHandler mapHandler) => this.mapHandler = mapHandler;

    public void OnMapReady(GoogleMap googleMap) => this.mapHandler.UpdateValue(nameof(IMap.Pins));
}
