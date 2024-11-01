using Foundation;
using UIKit;
using CoreGraphics;
using System.Threading.Tasks;

namespace secure_task_manager_app;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    private UIView _overlayView;
    private UILabel _overlayLabel;

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    public override bool FinishedLaunching(UIApplication app, NSDictionary options)
    {
        // Tworzenie nakładki, która zasłoni ekran
        _overlayView = new UIView(UIScreen.MainScreen.Bounds)
        {
            BackgroundColor = UIColor.Black,
            Alpha = 1.0f, // Pełna nieprzezroczystość
            Hidden = true  // Początkowo ukryta
        };

        // Dodawanie komunikatu do nakładki
        _overlayLabel = new UILabel
        {
            Frame = UIScreen.MainScreen.Bounds,
            Text = "Aplikacja jest nieaktywna",
            TextAlignment = UITextAlignment.Center,
            Font = UIFont.BoldSystemFontOfSize(24),
            TextColor = UIColor.White,
            BackgroundColor = UIColor.Clear
        };
        _overlayView.AddSubview(_overlayLabel);

        Console.WriteLine("The application has been launched.");
        return base.FinishedLaunching(app, options);
    }

    public override void OnActivated(UIApplication application)
    {
        Console.WriteLine("Application became active.");

        // Wyświetlenie nakładki przez 3 sekundy po powrocie do aktywnego stanu
        ShowOverlayWithDelay();
    }

    public override void OnResignActivation(UIApplication application)
    {
        Console.WriteLine("Application is going inactive.");

        // Wyświetlenie nakładki, gdy aplikacja traci aktywność
        ShowOverlay();
    }

    private void ShowOverlay()
    {
        var keyWindow = UIApplication.SharedApplication.KeyWindow;

        if (keyWindow != null)
        {
            // Upewnij się, że nakładka jest dodana do okna
            if (_overlayView.Superview == null)
            {
                keyWindow.AddSubview(_overlayView);
            }
            _overlayView.Hidden = false;
        }
    }

    private async void ShowOverlayWithDelay()
    {
        // Wyświetlenie nakładki natychmiast po aktywacji
        ShowOverlay();

        // Opóźnienie przed ukryciem nakładki
        await Task.Delay(3000);

        // Ukrycie nakładki po opóźnieniu
        _overlayView.Hidden = true;
        Console.WriteLine("Overlay hidden after delay.");
    }
}
