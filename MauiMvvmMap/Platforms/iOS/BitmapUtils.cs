namespace MauiMvvmMap;

using System.Collections.Concurrent;
using Microsoft.Maui;
using Microsoft.Maui.Platform;
using UIKit;

internal static class BitmapUtils
{
    private static readonly ConcurrentDictionary<IView, UIImage> Cache = new();

    public static UIImage GetViewForIcon(IView icon, IMauiContext mauiContext) => Cache.GetOrAdd(icon, i => ConvertViewToImage(i, mauiContext));

    private static UIView ConvertFormsToNative(IElement element, IMauiContext mauiContext)
    {
        var nativeView = element.ToPlatform(mauiContext);

        nativeView.BackgroundColor = UIColor.FromWhiteAlpha(1, new(0.01));
        nativeView.AutoresizingMask = UIViewAutoresizing.All;
        nativeView.ContentMode = UIViewContentMode.ScaleToFill;

        return nativeView;
    }

    private static UIImage ConvertViewToImage(IView icon, IMauiContext mauiContext)
    {
        var iconView = ConvertFormsToNative(icon, mauiContext);
        return ConvertViewToImage(iconView);
    }

    private static UIImage ConvertViewToImage(UIView view)
    {
        UIGraphics.BeginImageContextWithOptions(view.Bounds.Size, false, 0);
        view.Layer.RenderInContext(UIGraphics.GetCurrentContext());
        var img = UIGraphics.GetImageFromCurrentImageContext();
        UIGraphics.EndImageContext();

        return img;
    }
}
