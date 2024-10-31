using Foundation;
using UIKit;

namespace secure_task_manager_app;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    public override bool FinishedLaunching(UIApplication app, NSDictionary options)
    {
        // Code to be run when the application starts
        Console.WriteLine("The application has been launched.");
        return base.FinishedLaunching(app, options);
    }

    public override void OnActivated(UIApplication application)
    {
        // Called when the application becomes active
        Console.WriteLine("The application is active.");
    }

    public override void OnResignActivation(UIApplication application)
    {
        // Called when the application enters the inactive state
        Console.WriteLine("Application no longer active - alerting or hiding sensitive data.");
    }
}
