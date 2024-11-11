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

                // Pobieramy zadania lokalne
                var localTasks = await _sqliteService.GetTasksAsync();
                var localTaskDict = localTasks.ToDictionary(task => task.Id);

                // Synchronizacja zadań z serwera do lokalnej bazy danych
                foreach (var serverTask in serverTasks)
                {
                    if (localTaskDict.TryGetValue(serverTask.Id, out var localTask))
                    {
                        if (serverTask.LastSyncDate > localTask.LastSyncDate)
                        {
                            // Aktualizujemy lokalne zadanie
                            localTask.Title = serverTask.Title;
                            localTask.Description = serverTask.Description;
                            localTask.DueDate = serverTask.DueDate ?? DateTime.MinValue;
                            localTask.Completed = serverTask.Completed;
                            localTask.LastSyncDate = serverTask.LastSyncDate;

                            await _sqliteService.SaveTaskAsync(localTask);
                        }
                    }
                    else
                    {
                        // Dodajemy nowe zadanie z serwera do bazy lokalnej
                        await _sqliteService.SaveTaskAsync(serverTask);
                    }
                }

                // Synchronizacja lokalnych zadań do serwera
                foreach (var localTask in localTasks)
                {
                    if (localTask.LastSyncDate == default)
                    {
                        if (await _apiService.AddTaskAsync(localTask))
                        {
                            localTask.LastSyncDate = DateTime.UtcNow;
                            await _sqliteService.SaveTaskAsync(localTask);
                        }
                    }
                    else if (serverTasks.FirstOrDefault(t => t.Id == localTask.Id) is Models.Task serverTask && localTask.LastSyncDate > serverTask.LastSyncDate)
                    {
                        if (await _apiService.UpdateTaskAsync(localTask))
                        {
                            localTask.LastSyncDate = DateTime.UtcNow;
                            await _sqliteService.SaveTaskAsync(localTask);
                        }
                    }
                }

                // Resetowanie i ponowne budowanie listy zadań w interfejsie
                Tasks.Clear();
                var updatedTasks = await _sqliteService.GetTasksAsync();
                foreach (var task in updatedTasks)
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
