using Foundation;
using UIKit;

namespace secure_task_manager_app;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    public override bool FinishedLaunching(UIApplication app, NSDictionary options)
    {
        // Support for screenshot notification on iOS
        NSNotificationCenter.DefaultCenter.AddObserver(
            UIApplication.UserDidTakeScreenshotNotification,
            (notification) =>
            {
                Console.WriteLine("A screenshot has been taken!");
                // Logic here, such as displaying an alert or blocking a view
            }
        );

        return base.FinishedLaunching(app, options);
    }
}
