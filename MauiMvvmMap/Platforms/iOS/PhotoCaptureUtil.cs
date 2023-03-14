namespace MauiMvvmMap;

using System;
using System.Threading.Tasks;
using Foundation;
using Microsoft.Maui.Graphics.Platform;
using UIKit;

public static class PhotoCaptureUtil
{
    public static async Task<FileResult?> CapturePhotoAsync(MediaPickerOptions? options = null)
    {
        options ??= new MediaPickerOptions { Title = "Take a photo" };

        return await MainThread.InvokeOnMainThreadAsync(async () => await InternalCapturePhotoAsync(options));
    }

    private static async Task<FileResult?> InternalCapturePhotoAsync(MediaPickerOptions options)
    {
        var taskCompletionSource = new TaskCompletionSource<FileResult?>();

        // Create an image picker object
        var imagePicker = new UIImagePickerController
        {
            SourceType = UIImagePickerControllerSourceType.Camera,
            MediaTypes = UIImagePickerController.AvailableMediaTypes(UIImagePickerControllerSourceType.PhotoLibrary),
        };

        if (string.IsNullOrWhiteSpace(options.Title))
        {
            imagePicker.Title = options.Title;
        }

        var vc = Platform.GetCurrentUIViewController() ?? throw new InvalidOperationException($"Cannot retrieve {nameof(UIViewController)}");

        imagePicker.AllowsEditing = false;
        imagePicker.FinishedPickingMedia += async (sender, e) =>
        {
            var jpegFilename = Path.Combine(FileSystem.CacheDirectory, $"{Guid.NewGuid()}.jpg");
            var uiImage = e.Info[UIImagePickerController.OriginalImage] as UIImage;
            var normalizedImage = uiImage.NormalizeOrientation();
            var normalizedData = normalizedImage.AsJPEG();

            await vc.DismissViewControllerAsync(true);

            if (normalizedData.Save(jpegFilename, false, out var error))
            {
                _ = taskCompletionSource.TrySetResult(new FileResult(jpegFilename));
            }
            else
            {
                _ = taskCompletionSource.TrySetException(new IOException($"Error saving the image: {error}"));
            }

            imagePicker?.Dispose();
            imagePicker = null;
        };

        imagePicker.Canceled += async (sender, e) =>
        {
            await vc.DismissViewControllerAsync(true);
            _ = taskCompletionSource.TrySetResult(null);
            imagePicker?.Dispose();
            imagePicker = null;
        };

        await vc.PresentViewControllerAsync(imagePicker, true);

        return await taskCompletionSource.Task;
    }

    private class CameraDelegate : UIImagePickerControllerDelegate
    {
        public override void FinishedPickingMedia(UIImagePickerController picker, NSDictionary info) => picker.DismissViewController(false, null);
    }
}
