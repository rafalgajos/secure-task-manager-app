using secure_task_manager_app.Views;
using Microsoft.Maui.Controls;

namespace secure_task_manager_app.Views
{
    public partial class RegisterPage : ContentPage
    {
        public RegisterPage()
        {
            InitializeComponent();

            // Set the registration endpoint URL to a dynamically loaded address
            string registerUrl = $"{App.ApiBaseUrl}/register_form";
            RegisterWebView.Source = registerUrl;
        }
    }
}
