#if MACCATALYST
using Foundation;
using WebKit;
using Security;
#endif

namespace secure_task_manager_app.Views
{
    public partial class RegisterPage : ContentPage
    {
        public RegisterPage()
        {
            InitializeComponent();

            string registerUrl = "https://127.0.0.1:8443/register_form";

#if MACCATALYST
            RegisterWebView.HandlerChanged += (s, e) =>
            {
                if (RegisterWebView.Handler.PlatformView is WKWebView wkWebView)
                {
                    wkWebView.NavigationDelegate = new CustomNavigationDelegate();
                }
            };
#endif

            RegisterWebView.Source = registerUrl;
        }
    }

#if MACCATALYST
    public class CustomNavigationDelegate : WKNavigationDelegate
    {
        public override void DidReceiveAuthenticationChallenge(WKWebView webView, NSUrlAuthenticationChallenge challenge, Action<NSUrlSessionAuthChallengeDisposition, NSUrlCredential?> completionHandler)
        {
            if (challenge.ProtectionSpace.AuthenticationMethod == "NSURLAuthenticationMethodServerTrust")
            {
                // Sprawdzenie, czy mamy zaufany obiekt SecTrust
                if (challenge.ProtectionSpace.ServerSecTrust != null)
                {
                    var credential = NSUrlCredential.FromTrust(challenge.ProtectionSpace.ServerSecTrust);
                    completionHandler(NSUrlSessionAuthChallengeDisposition.UseCredential, credential);
                }
                else
                {
                    completionHandler(NSUrlSessionAuthChallengeDisposition.PerformDefaultHandling, null);
                }
            }
            else
            {
                completionHandler(NSUrlSessionAuthChallengeDisposition.PerformDefaultHandling, null);
            }
        }
    }
#endif
}
