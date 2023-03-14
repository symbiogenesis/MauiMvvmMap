namespace MauiMvvmMap;

using Foundation;
using UIKit;

#pragma warning disable CA1711 // AppDelegate is the standard name

[Register(nameof(AppDelegate))]
public class AppDelegate : MauiUIApplicationDelegate
{
    public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
    {
        AppDomain.CurrentDomain.UnhandledException += (_, args) => CurrentDomainOnUnhandledException(args);
        TaskScheduler.UnobservedTaskException += (_, args) => TaskSchedulerOnUnobservedTaskException(args);

        return base.FinishedLaunching(application, launchOptions);
    }

    protected override MauiApp CreateMauiApp()
    {
        return MauiProgram.CreateMauiApp();
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

#pragma warning restore CA1711
