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

            // Wczytanie adresu API
            Task.Run(async () =>
            {
                ApiBaseUrl = await LoadNgrokUrl();
                Console.WriteLine($"Loaded ApiBaseUrl: {ApiBaseUrl}");
            }).Wait();

            // Ustawienie strony początkowej na TaskListPage
            MainPage = new NavigationPage(new Views.TaskListPage());
        }

        private async Task<string> LoadNgrokUrl()
        {
            try
            {
                Console.WriteLine("Attempting to load ngrok URL from ngrok_url.txt in Raw folder");

                // Wczytaj plik ngrok_url.txt z zasobów aplikacji
                using var stream = await FileSystem.OpenAppPackageFileAsync("ngrok_url.txt");
                using var reader = new StreamReader(stream);

                string url = await reader.ReadToEndAsync();
                return url.Trim(); // usuwa białe znaki, aby upewnić się, że URL jest prawidłowy
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading ngrok URL: {ex.Message}. Using default localhost.");
                return "https://127.0.0.1:8443"; // Domyślny URL w przypadku błędu
            }
        }
    }
}
