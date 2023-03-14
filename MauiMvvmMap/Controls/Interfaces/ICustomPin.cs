namespace MauiMvvmMap.Controls.Interfaces;

using Microsoft.Maui;
using Microsoft.Maui.Maps;

public interface ICustomPin : IMapPin
{
    IView? Icon { get; set; }

    string? PinColor { get; }
}
