using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;

namespace secure_task_manager_app;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // Preventing screen capture on Android
        Window.SetFlags(WindowManagerFlags.Secure, WindowManagerFlags.Secure);
    }
}
