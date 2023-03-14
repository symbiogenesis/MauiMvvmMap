namespace MauiMvvmMap.Platforms.Android;

using global::Android.App;
using global::Android.Content.PM;
using global::Android.OS;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        AppDomain.CurrentDomain.UnhandledException += (_, args) => CurrentDomainOnUnhandledException(args);
        TaskScheduler.UnobservedTaskException += (_, args) => TaskSchedulerOnUnobservedTaskException(args);

        base.OnCreate(savedInstanceState);
    }

    private static void TaskSchedulerOnUnobservedTaskException(UnobservedTaskExceptionEventArgs args) => App.LogService.LogException(args.Exception);

    private static void CurrentDomainOnUnhandledException(UnhandledExceptionEventArgs args)
    {
        if (args.ExceptionObject is Exception exception)
        {
            App.LogService.LogException(exception);
        }
    }
}
