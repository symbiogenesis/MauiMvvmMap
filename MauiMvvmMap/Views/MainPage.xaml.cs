namespace MauiMvvmMap.Views;

using MauiMvvmMap.ViewModels;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        this.InitializeComponent();
        this.BindingContext = new MainViewModel();
    }
}
