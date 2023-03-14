namespace CommunityToolkit.Maui.Maps.Handlers;

using System.Globalization;
using System.Text.Json;
using MauiMvvmMap.Controls;
using MauiMvvmMap.Controls.Interfaces;
using MauiMvvmMap.Extensions;
using MauiMvvmMap.Models;
using Microsoft.Maui.Maps;
using Microsoft.Maui.Maps.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using Windows.Devices.Geolocation;
using IMap = Microsoft.Maui.Maps.IMap;

public class CustomMapHandler : MapHandler
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private MapSpan? regionToGo;

    public CustomMapHandler()
        : base(Mapper, CommandMapper)
    {
        Mapper.ModifyMapping(nameof(IMap.MapType), (handler, map, _) => MapMapType(handler, map));
        Mapper.ModifyMapping(nameof(IMap.IsShowingUser), (handler, map, _) => MapIsShowingUser(handler, map));
        Mapper.ModifyMapping(nameof(IMap.IsScrollEnabled), (handler, map, _) => MapIsScrollEnabled(handler, map));
        Mapper.ModifyMapping(nameof(IMap.IsTrafficEnabled), (handler, map, _) => MapIsTrafficEnabled(handler, map));
        Mapper.ModifyMapping(nameof(IMap.IsZoomEnabled), (handler, map, _) => MapIsZoomEnabled(handler, map));
        Mapper.ModifyMapping(nameof(IMap.Pins), (handler, map, _) => MapPins(handler, map));
        Mapper.ModifyMapping(nameof(IMap.Elements), (handler, map, _) => MapElements(handler, map));
        CommandMapper.ModifyMapping(nameof(IMap.MoveToRegion), (handler, map, args, _) => MapMoveToRegion(handler, map, args));
    }

    internal static string? MapsKey { get; set; }

    internal static bool EnableStreetView { get; set; }

    public static new void MapMapType(IMapHandler handler, IMap map) => CallJSMethod(handler, $"setMapType('{map.MapType}');");

    public static new void MapIsZoomEnabled(IMapHandler handler, IMap map) => CallJSMethod(handler, $"disableMapZoom({(!map.IsZoomEnabled).ToString().ToLower(CultureInfo.InvariantCulture)});");

    public static new void MapIsScrollEnabled(IMapHandler handler, IMap map) => CallJSMethod(handler, $"disablePanning({(!map.IsScrollEnabled).ToString().ToLower(CultureInfo.InvariantCulture)});");

    public static new void MapIsTrafficEnabled(IMapHandler handler, IMap map) => CallJSMethod(handler, $"disableTraffic({(!map.IsTrafficEnabled).ToString().ToLower(CultureInfo.InvariantCulture)});");

    public static new async void MapIsShowingUser(IMapHandler handler, IMap map)
    {
        if (map.IsShowingUser)
        {
            var location = await GetCurrentLocation();
            if (location != null)
            {
                CallJSMethod(handler, $"addLocationPin({location.Latitude},{location.Longitude});");
            }
        }
        else
        {
            CallJSMethod(handler, "removeLocationPin();");
        }
    }

    public static new void MapPins(IMapHandler handler, IMap map)
    {
        var customPins = map.Pins.Cast<ICustomPin>().Select(p => new { p.Location, p.Label, p.PinColor }).ToList();
        var pinsJson = JsonSerializer.Serialize(customPins, JsonOptions);
        CallJSMethod(handler, $"addPins({pinsJson});");
    }

    public static new void MapElements(IMapHandler handler, IMap map)
    {
    }

    public static new void UpdateMapElement(IMapElement element)
    {
    }

    public static new void MapMoveToRegion(IMapHandler handler, IMap map, object? arg)
    {
        var newRegion = arg as MapSpan;
        if (newRegion != null)
        {
            if (handler is CustomMapHandler mapHandler)
            {
                mapHandler.regionToGo = newRegion;
            }

            var (southWest, northEast) = newRegion.ToBounds();

            CallJSMethod(handler, $"setRegion({southWest.Latitude}, {southWest.Longitude}, {northEast.Latitude}, {northEast.Longitude});");
        }
    }

    /// <inheritdoc/>
    protected override FrameworkElement CreatePlatformView()
    {
        if (string.IsNullOrWhiteSpace(MapsKey))
        {
            throw new InvalidOperationException("You need to specify a Bing Maps Key");
        }

        var mapPage = GetMapHtmlPage(MapsKey, !EnableStreetView);
        var webView = new MauiWebView();
        webView.NavigationCompleted += this.WebViewNavigationCompleted;
        webView.WebMessageReceived += this.WebViewWebMessageReceived;
        webView.LoadHtml(mapPage, null);
        return webView;
    }

    protected override void DisconnectHandler(FrameworkElement platformView)
    {
        if (platformView is MauiWebView mauiWebView)
        {
            mauiWebView.NavigationCompleted -= this.WebViewNavigationCompleted;
            mauiWebView.WebMessageReceived -= this.WebViewWebMessageReceived;
        }

        base.DisconnectHandler(platformView);
    }

    private static void CallJSMethod(IMapHandler handler, string script)
    {
        if (handler.PlatformView is WebView2 webView2 && webView2.CoreWebView2 != null)
        {
            _ = webView2.DispatcherQueue.TryEnqueue(async () => await webView2.ExecuteScriptAsync(script));
        }
    }

    private static string GetMapHtmlPage(string key, bool disableStreetView) =>
        $$$"""
        <!DOCTYPE html>
        <html>
            <head>
            <title></title>
            <meta http-equiv="Content-Security-Policy" content="default-src 'self' data: gap: https://ssl.gstatic.com 'unsafe-eval' 'unsafe-inline' https://*.bing.com https://*.virtualearth.net; style-src 'self' 'unsafe-inline' https://*.bing.com https://*.virtualearth.net; media-src *">
            <meta name="viewport" content="user-scalable=no, initial-scale=1, maximum-scale=1, minimum-scale=1, width=device-width">
            <script type="text/javascript" src="https://www.bing.com/api/maps/mapcontrol?key={{{key}}}"></script>
            <script type="text/javascript">
                var map;
                var selectedPin;
                var locationPin;
                function loadMap() {
                    map = new Microsoft.Maps.Map(document.getElementById('myMap'), {
                        disableBirdseye : true,
                    	disableZooming: true,
                    //	disablePanning: true,
                        disableStreetside: {{{disableStreetView.ToString().ToLowerInvariant()}}},
                        showScalebar: false,
                        showLocateMeButton: false,
                        showDashboard: false,
                        showTermsLink: false,
                        showTrafficButton: false
                    });
                    Microsoft.Maps.Events.addHandler(map, 'viewrendered', function () { var bounds = map.getBounds(); invokeHandlerAction(bounds); });
                    Microsoft.Maps.Events.addHandler(map, "dblclick", function(e) { e.location.wasClicked = true; invokeHandlerAction(e.location); });
                }

                function disableMapZoom(disable) {
                    map.setOptions({
                        disableZooming: disable,
                    });
                }

                function disablePanning(disable) {
                    map.setOptions({
                        disablePanning: disable,
                    });
                }

                function setMapType(mauiMapType) {
                    var mapTypeID = Microsoft.Maps.MapTypeId.road;
                    switch(mauiMapType) {
                        case 'Street':
                        mapTypeID = Microsoft.Maps.MapTypeId.road;
                        break;
                        case 'Satellite':
                        mapTypeID = Microsoft.Maps.MapTypeId.aerial;
                        break;
                        case 'Hybrid':
                        mapTypeID = Microsoft.Maps.MapTypeId.aerial;
                        break;
                        default:
                        mapTypeID = Microsoft.Maps.MapTypeId.road;
                    }
                    map.setView({
                        mapTypeId: mapTypeID
                    });
                }

                function setRegion(southWestLat, southWestLong, northEastLat, northEastLong) {
                    let southWest = new Microsoft.Maps.Location(southWestLat, southWestLong);
                    let northEast = new Microsoft.Maps.Location(northEastLat, northEastLong);
                    let locs = [southWest, northEast];
                    let rect = Microsoft.Maps.LocationRect.fromLocations(locs);
                    map.setView({ bounds: rect, padding: 80 });
                }

                function addPins(pins) {
                    map.entities.clear();
        
                    pins.forEach(pin => {
                        let location = new Microsoft.Maps.Location(pin.location.latitude, pin.location.longitude);
                        let newPin = new Microsoft.Maps.Pushpin(location, { color: pin.pinColor });
                        newPin.originalColor = pin.pinColor;
        
                        // Add PushPin data to the data source
                        map.entities.push(newPin);
                        Microsoft.Maps.Events.addHandler(newPin, 'click', updateSelectedPin);
                    });
                }

               function updateSelectedPin(e) {
                    let newSelectedPin = e.target;
                    invokeHandlerAction(newSelectedPin.getLocation());
                    newSelectedPin.setOptions({ color: 'yellow' });
                    if (selectedPin)
                    {
                        selectedPin.setOptions({ color: selectedPin.originalColor });
                    }
                    selectedPin = newSelectedPin;
                }

                function addPin(latitude, longitude) {
                    let location = new Microsoft.Maps.Location(latitude, longitude);
                    let newPin = new Microsoft.Maps.Pushpin(location, null);
                    map.entities.push(newPin);
                }

                function addLocationPin(latitude, longitude) {
                    if(!locationPin)
                    {
                        let location = new Microsoft.Maps.Location(latitude, longitude);
                        locationPin = new Microsoft.Maps.Pushpin(location, { title: 'Current Location' });
                        map.entities.push(locationPin);
                    }
                }

                function removeLocationPin() {
                    if(locationPin != null)
                    {
                        map.entities.remove(locationPin);
                        locationPin = null;
                    }
                }

                function invokeHandlerAction(data) {
                    window.chrome.webview.postMessage(data);
                }
            </script>
            <style>
                body, html{
                    padding:0;
                    margin:0;
                }
            </style>
        </head>
        <body onload='loadMap();'>
            <div id="myMap"></div>
        </body>
        </html>
        """;

    private static async Task<Location?> GetCurrentLocation()
    {
        try
        {
            _ = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

            var geolocator = new Geolocator() { DesiredAccuracy = PositionAccuracy.High };
            var position = await geolocator.GetGeopositionAsync();
            return new Location(position.Coordinate.Latitude, position.Coordinate.Longitude);
        }
        catch
        {
            return null;
        }
    }

    private void WebViewNavigationCompleted(WebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
    {
        // Update initial properties when our page is loaded
        Mapper.UpdateProperties(this, this.VirtualView);

        if (this.regionToGo is not null)
        {
            MapMoveToRegion(this, this.VirtualView, this.regionToGo);
        }
    }

    private void WebViewWebMessageReceived(WebView2 sender, CoreWebView2WebMessageReceivedEventArgs args)
    {
        if (args.WebMessageAsJson is "undefined")
        {
            return;
        }

        ClickableLocation? location;

        var mapRect = JsonSerializer.Deserialize<Bounds>(args.WebMessageAsJson, JsonOptions);
        if (mapRect?.Center is not null)
        {
            location = new ClickableLocation(mapRect.Center?.Latitude ?? 0, mapRect.Center?.Longitude ?? 0);
            this.VirtualView.VisibleRegion = new MapSpan(location, mapRect.Height, mapRect.Width);
            return;
        }

        location = JsonSerializer.Deserialize<ClickableLocation>(args.WebMessageAsJson, JsonOptions);
        if (location is not null)
        {
            var locationString = $"{location.Latitude}, {location.Longitude}";
            var customMap = (MvvmMap)this.VirtualView;

            if (location.WasClicked)
            {
                this.VirtualView.Clicked(location);
            }
            else
            {
                customMap.SelectedItem = customMap.ItemsSource.Cast<Record>().FirstOrDefault(i => i.Location == locationString);
            }
        }
    }

    private class ClickableLocation : Location
    {
        public ClickableLocation(double latitude, double longitude)
            : base(latitude, longitude)
        {
        }

        public bool WasClicked { get; set; }
    }

    private sealed class Center
    {
        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public int Altitude { get; set; }

        public int AltitudeReference { get; set; }
    }

    private sealed class Bounds
    {
        public Center? Center { get; set; }

        public double Width { get; set; }

        public double Height { get; set; }
    }
}
