namespace MauiMvvmMap;

using MauiMvvmMap.CustomControls;
using CoreLocation;
using Microsoft.Maui.Maps;
using Microsoft.Maui.Maps.Handlers;
using Microsoft.Maui.Maps.Platform;
using Microsoft.Maui.Platform;
using UIKit;

public class CustomMapHandler : MapHandler
{
    public static readonly IPropertyMapper<IMap, IMapHandler> CustomMapper =
        new PropertyMapper<IMap, IMapHandler>(Mapper)
        {
            [nameof(IMap.Pins)] = MapPins,
        };

    public CustomMapHandler()
        : base(CustomMapper, CommandMapper)
    {
    }

    public CustomMapHandler(IPropertyMapper? mapper = null, CommandMapper? commandMapper = null)
        : base(
        mapper ?? CustomMapper, commandMapper ?? CommandMapper)
    {
    }

    internal static bool EnableMapSymbols { get; set; } = true;

    protected override void ConnectHandler(MauiMKMapView platformView)
    {
        base.ConnectHandler(platformView);
        platformView.GetViewForAnnotation += MapExtensions.GetViewForAnnotations;
    }

    private static new void MapPins(IMapHandler handler, IMap map)
    {
        if (handler is CustomMapHandler mapHandler)
        {
            mapHandler.AddPins(map.Pins);
        }
    }

    private void AddPins(IEnumerable<IMapPin> mapPins)
    {
        if (this.PlatformView is null || this.MauiContext is null)
        {
            return;
        }

        foreach (var pin in mapPins)
        {
            if (pin is CustomPin customPin)
            {
                UIImage? image = null;

                if (EnableMapSymbols && customPin.Icon != null)
                {
                    // var imageSourceHandler = new ImageLoaderSourceHandler();
                    // var image = await imageSourceHandler.LoadImageAsync(pin.ImageSource);
                    image = BitmapUtils.GetViewForIcon(customPin.Icon, this.MauiContext!);
                }

                var exists = this.PlatformView.Annotations.OfType<CustomAnnotation>().Any(a => a.Identifier == customPin.Id);

                if (!exists)
                {
                    var customAnnotation = new CustomAnnotation()
                    {
                        Identifier = customPin.Id,
                        Image = image,
                        Title = customPin.Label,
                        Subtitle = customPin.Address,
                        Coordinate = new CLLocationCoordinate2D(customPin.Location.Latitude, customPin.Location.Longitude),
                        Pin = customPin,
                    };

                    customPin.MarkerId = customAnnotation;

                    this.PlatformView.AddAnnotation(customAnnotation);
                }
            }
        }

        List<CustomAnnotation> annotationsToRemove = new();

        foreach (var annotation in this.PlatformView.Annotations.OfType<CustomAnnotation>())
        {
            if (!mapPins.Any(p => p.MarkerId == annotation))
            {
                annotationsToRemove.Add(annotation);
            }
        }

        this.PlatformView.RemoveAnnotations(annotationsToRemove.ToArray());
    }
}
