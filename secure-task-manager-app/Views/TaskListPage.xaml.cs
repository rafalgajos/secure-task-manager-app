using secure_task_manager_app.Models;
using secure_task_manager_app.Services;
using System.Collections.ObjectModel;

namespace secure_task_manager_app.Views
{
    public partial class TaskListPage : ContentPage
    {
        private readonly SQLiteService _sqliteService;
        private readonly ApiService _apiService;
        public ObservableCollection<Models.Task> Tasks { get; set; }

        public TaskListPage()
        {
            InitializeComponent();
            _sqliteService = new SQLiteService(App.DatabasePassword); // Przekazanie hasła do konstruktora
            _apiService = new ApiService();
            Tasks = new ObservableCollection<Models.Task>();
            TasksListView.ItemsSource = Tasks;

            LoadTasks();

            MessagingCenter.Subscribe<LoginPage>(this, "SyncTasks", async (sender) =>
            {
                await SyncTasksWithServer();
            });
        }

        private async void LoadTasks()
        {
            var tasks = await _sqliteService.GetTasksAsync();
            if (tasks != null)
            {
                Tasks.Clear();
                foreach (var task in tasks)
                {
                    Tasks.Add(task);
                }
            }
            else
            {
                await DisplayAlert("Error", "Failed to load tasks", "OK");
            }
        }

        private async void OnAddTaskClicked(object sender, EventArgs e)
        {
            var newTask = new Models.Task();
            var taskDetailPage = new TaskDetailPage(newTask, Tasks);
            await Navigation.PushAsync(taskDetailPage);
        }

        private async void OnTaskTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item is Models.Task selectedTask)
            {
                var taskDetailPage = new TaskDetailPage(selectedTask, Tasks);
                await Navigation.PushAsync(taskDetailPage);
            }
            ((ListView)sender).SelectedItem = null;
        }

        private async void OnSyncClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new LoginPage());
        }

        private async System.Threading.Tasks.Task SyncTasksWithServer()
        {
            try
            {
                // Pobieramy zadania z serwera
                var serverTasks = await _apiService.GetTasksAsync();
                if (serverTasks == null)
                {
                    await DisplayAlert("Error", "Failed to fetch tasks from server.", "OK");
                    return;
                }

                // Czyścimy lokalną bazę danych i dodajemy wszystkie zadania z serwera na nowo
                await _sqliteService.ClearAllTasksAsync(); // Nowa metoda do wyczyszczenia lokalnej tabeli

                // Zapisujemy wszystkie zadania z serwera w lokalnej bazie danych
                foreach (var serverTask in serverTasks)
                {
                    await _sqliteService.SaveTaskAsync(serverTask);
                }

                // Aktualizacja interfejsu użytkownika z zadaniami z serwera
                Tasks.Clear();
                foreach (var task in serverTasks)
                {
                    Tasks.Add(task);
                }

                await DisplayAlert("Success", "Synchronization completed successfully.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred during synchronization: {ex.Message}", "OK");
            }
        }
    }
}
