// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
namespace MauiMvvmMap.WinUI;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : MauiWinUIApplication
{
    /// <summary>
    /// Initializes a new instance of the <see cref="App"/> class.
    /// This will generally be run as a singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();
        this.UnhandledException += (_, s) => MauiMvvmMap.App.LogService.LogException(s.Exception);
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
