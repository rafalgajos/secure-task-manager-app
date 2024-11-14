using secure_task_manager_app.Models;
using secure_task_manager_app.Services;
using System.Collections.ObjectModel;
using System.Linq;

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
            _sqliteService = new SQLiteService(App.DatabasePassword);
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
                // Synchronizuj zadania oznaczone do synchronizacji (dodawanie zadań lokalnych na serwer)
                var tasksToSync = await _sqliteService.GetTasksAsync();
                tasksToSync = tasksToSync.Where(task => task.SyncWithBackend).ToList();

                foreach (var task in tasksToSync)
                {
                    bool success = await _apiService.AddTaskAsync(task);
                    if (success)
                    {
                        // Po pomyślnej synchronizacji usuń zadanie z lokalnej bazy danych
                        await _sqliteService.DeleteTaskAsync(task);
                        Tasks.Remove(task); // Usuń zadanie z listy wyświetlanej w interfejsie użytkownika
                    }
                }

                // Pobierz zadania z serwera
                var serverTasks = await _apiService.GetTasksAsync();
                if (serverTasks != null)
                {
                    foreach (var serverTask in serverTasks)
                    {
                        // Sprawdź, czy zadanie z serwera już istnieje w lokalnej bazie (na podstawie tytułu, daty i opisu)
                        var existingTask = Tasks.FirstOrDefault(t =>
                            t.Title == serverTask.Title &&
                            t.Description == serverTask.Description &&
                            t.DueDate == serverTask.DueDate &&
                            t.Completed == serverTask.Completed);

                        if (existingTask == null)
                        {
                            // Dodaj nowe zadanie z serwera do lokalnej bazy i interfejsu użytkownika
                            await _sqliteService.SaveTaskAsync(serverTask);
                            Tasks.Add(serverTask);
                        }
                        else
                        {
                            // Aktualizacja istniejącego zadania (na wypadek, gdyby zostało zaktualizowane na serwerze)
                            existingTask.Title = serverTask.Title;
                            existingTask.Description = serverTask.Description;
                            existingTask.DueDate = serverTask.DueDate;
                            existingTask.Completed = serverTask.Completed;
                            existingTask.Location = serverTask.Location;
                            existingTask.LastSyncDate = serverTask.LastSyncDate;

                            await _sqliteService.SaveTaskAsync(existingTask);
                        }
                    }
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
