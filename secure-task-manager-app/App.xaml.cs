using Microsoft.Maui.Controls;

namespace secure_task_manager_app
{
    public partial class App : Application
    {
        public static string ApiBaseUrl { get; private set; }
        public static string DatabasePassword { get; private set; } = "secure_password"; // Ustawienie hasła do bazy danych

        public App()
        {
            InitializeComponent();

            Task.Run(async () =>
            {
                ApiBaseUrl = await LoadNgrokUrl();
                Console.WriteLine($"Loaded ApiBaseUrl: {ApiBaseUrl}");
            }).Wait();

            MainPage = new NavigationPage(new Views.TaskListPage());
        }

        private async Task<string> LoadNgrokUrl()
        {
            try
            {
                Console.WriteLine("Attempting to load ngrok URL from ngrok_url.txt in Raw folder");

                using var stream = await FileSystem.OpenAppPackageFileAsync("ngrok_url.txt");
                using var reader = new StreamReader(stream);

                string url = await reader.ReadToEndAsync();
                return url.Trim();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading ngrok URL: {ex.Message}. Using default localhost.");
                return "https://127.0.0.1:8443";
            }
        }
    }
}
