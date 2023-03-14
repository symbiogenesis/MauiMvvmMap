namespace MauiMvvmMap.Platforms.Android;

using global::Android.App;
using global::Android.Content;
using Microsoft.Identity.Client;

[Activity(Exported = true)]
[IntentFilter(
    new[] { Intent.ActionView },
    Categories = new[] { Intent.CategoryBrowsable, Intent.CategoryDefault },
    DataHost = "auth",
    DataScheme = $"msal{Constants.ApplicationId}")]
public class MsalActivity : BrowserTabActivity
{
}
