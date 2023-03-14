namespace MauiMvvmMap;

using MapKit;
using Microsoft.Maui.Maps;
using UIKit;

public static class MapExtensions
{
    private static UIView? lastTouchedView;

    internal static MKAnnotationView GetViewForAnnotations(MKMapView mapView, IMKAnnotation annotation)
    {
        MKAnnotationView? annotationView = null;

        if (annotation is CustomAnnotation customAnnotation)
        {
            annotationView = mapView.DequeueReusableAnnotation(customAnnotation.Identifier.ToString()) ??
                             new MKAnnotationView(annotation, customAnnotation.Identifier.ToString());
            annotationView.Image = customAnnotation.Image;
            annotationView.CanShowCallout = true;
        }

        var result = annotationView ?? new MKAnnotationView(annotation, null);
        AttachGestureToPin(result, annotation);
        return result;
    }

    private static void OnCalloutClicked(IMKAnnotation annotation)
    {
        var pin = GetPinForAnnotation(annotation);

        if (lastTouchedView is MKAnnotationView)
        {
            _ = pin?.SendMarkerClick();
        }
        else
        {
            _ = pin?.SendInfoWindowClick();
        }
    }

    private static void AttachGestureToPin(MKAnnotationView mapPin, IMKAnnotation annotation)
    {
        var recognizers = mapPin.GestureRecognizers;

        if (recognizers != null)
        {
            foreach (var r in recognizers)
            {
                mapPin.RemoveGestureRecognizer(r);
            }
        }

        var recognizer = new UITapGestureRecognizer(_ => OnCalloutClicked(annotation))
        {
            ShouldReceiveTouch = (_, touch) =>
            {
                lastTouchedView = touch.View;
                return true;
            },
        };

        mapPin.AddGestureRecognizer(recognizer);
    }

    private static IMapPin? GetPinForAnnotation(IMKAnnotation? annotation)
    {
        if (annotation is CustomAnnotation customAnnotation)
        {
            return customAnnotation.Pin;
        }

        return null;
    }
}
