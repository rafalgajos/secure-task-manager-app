using secure_task_manager_app.Models;
using secure_task_manager_app.Services;
using System;

namespace secure_task_manager_app.Views
{
    public partial class RegisterPage : ContentPage
    {
        private readonly ApiService _apiService = new ApiService();

        public RegisterPage()
        {
            InitializeComponent();
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            var user = new User
            {
                Username = UsernameEntry.Text,
                Password = PasswordEntry.Text
            };

            bool isRegistered = await _apiService.RegisterAsync(user);
            if (isRegistered)
            {
                await DisplayAlert("Success", "Registration successful! Please log in.", "OK");
                await Navigation.PopAsync(); // Powrót do strony logowania
            }
            else
            {
                await DisplayAlert("Error", "Registration failed. Try again.", "OK");
            }
        }
    }
}
