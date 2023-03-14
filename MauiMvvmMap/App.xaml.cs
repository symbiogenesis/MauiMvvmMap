namespace MauiMvvmMap;

using MauiMvvmMap.Services;
using MauiMvvmMap.Services.Interfaces;

public partial class App : Application
{
    public App()
    {
        this.InitializeComponent();

        this.MainPage = new AppShell();
    }
    public static ILogService LogService { get; set; } = new LogService();
}
