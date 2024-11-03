using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace secure_task_manager_app
{
    public partial class App : Application
    {
        public static string ApiBaseUrl { get; private set; }

        public App()
        {
            InitializeComponent();

            // Asynchroniczne wczytanie URL-a z pliku ngrok_url.txt
            Task.Run(async () =>
            {
                ApiBaseUrl = await LoadNgrokUrl();
            }).Wait(); // Poczekaj na zakończenie wczytywania URL-a

            // Ustawienie strony początkowej
            MainPage = new NavigationPage(new Views.LoginPage());
        }

        private async Task<string> LoadNgrokUrl()
        {
            try
            {
                // Wczytaj plik ngrok_url.txt z zasobów pakietu aplikacji
                using var stream = await FileSystem.OpenAppPackageFileAsync("ngrok_url.txt");
                using var reader = new StreamReader(stream);

                string url = await reader.ReadToEndAsync();
                return url.Trim();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading ngrok URL: {ex.Message}. Using default localhost.");
                return "https://127.0.0.1:8443"; // Domyślny URL w przypadku błędu
            }
        }
    }
}
