using Foundation;
using UIKit;
using CoreGraphics;

namespace secure_task_manager_app;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    private UIView blurView;

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    public override bool FinishedLaunching(UIApplication app, NSDictionary options)
    {
        // Initialisation of the blur view, which will cover the application when it goes into an inactive state
        blurView = new UIView(UIScreen.MainScreen.Bounds);
        blurView.BackgroundColor = UIColor.White;
        blurView.Alpha = 0.95f;

        NSNotificationCenter.DefaultCenter.AddObserver(
            UIApplication.UserDidTakeScreenshotNotification,
            (notification) =>
            {
                Console.WriteLine("A screenshot has been taken!");
                // Additional logic can be added here, e.g. notifying the user
            }
        );

        return base.FinishedLaunching(app, options);
    }

    public override void OnResignActivation(UIApplication application)
    {
        // Adding a blur when the application is inactive
        AddBlurOverlay();
        Console.WriteLine("Application no longer active - sensitive data obscured.");
    }

    public override void OnActivated(UIApplication application)
    {
        // Remove blur when the application becomes active
        RemoveBlurOverlay();
        Console.WriteLine("Application is active again - sensitive data visible.");
    }

    private void AddBlurOverlay()
    {
        if (blurView.Superview == null && UIApplication.SharedApplication.KeyWindow != null)
        {
            UIApplication.SharedApplication.KeyWindow.AddSubview(blurView);
        }
    }

    private void RemoveBlurOverlay()
    {
        blurView?.RemoveFromSuperview();
    }
}
