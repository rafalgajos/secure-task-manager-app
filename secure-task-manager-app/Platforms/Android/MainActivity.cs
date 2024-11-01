using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace secure_task_manager_app;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    private Android.Views.View _overlayView;
    private TextView _overlayTextView;

    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // Preventing screen capture on Android
        Window.SetFlags(WindowManagerFlags.Secure, WindowManagerFlags.Secure);

        // Create overlay view to cover the screen
        _overlayView = new FrameLayout(this)
        {
            LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent),
            Background = new Android.Graphics.Drawables.ColorDrawable(Android.Graphics.Color.Black)
        };
        _overlayView.Visibility = ViewStates.Gone;

        // Create text view for overlay message
        _overlayTextView = new TextView(this)
        {
            Text = "Aplikacja jest nieaktywna",
            TextSize = 24f,
            Gravity = GravityFlags.Center
        };
        _overlayTextView.SetTextColor(Android.Graphics.Color.White);

        // Add text to overlay
        (_overlayView as FrameLayout)?.AddView(_overlayTextView);

        // Add overlay to the root view
        var rootView = Window.DecorView.RootView as ViewGroup;
        if (rootView != null)
        {
            rootView.AddView(_overlayView);
        }
        else
        {
            Console.WriteLine("RootView is not a ViewGroup, unable to add overlay.");
        }
    }

    protected override void OnPause()
    {
        base.OnPause();
        // Show overlay when app loses focus
        _overlayView.Visibility = ViewStates.Visible;
    }

    protected override void OnResume()
    {
        base.OnResume();
        // Hide overlay when app regains focus
        _overlayView.Visibility = ViewStates.Gone;
    }
}
