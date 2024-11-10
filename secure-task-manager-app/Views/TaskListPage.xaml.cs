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
                var serverTasks = await _apiService.GetTasksAsync();
                if (serverTasks == null)
                {
                    await DisplayAlert("Błąd", "Nie udało się pobrać zadań z serwera", "OK");
                    return;
                }

                var localTasks = await _sqliteService.GetTasksAsync();
                var localTaskDict = localTasks.ToDictionary(task => task.Id);

                // Synchronizacja - Dodanie lub aktualizacja zadań z serwera do lokalnej bazy
                foreach (var serverTask in serverTasks)
                {
                    if (localTaskDict.TryGetValue(serverTask.Id, out var localTask))
                    {
                        // Zadanie istnieje lokalnie, sprawdzamy, czy wymaga aktualizacji
                        if (serverTask.LastSyncDate > localTask.LastSyncDate)
                        {
                            localTask.Title = serverTask.Title;
                            localTask.Description = serverTask.Description;
                            localTask.DueDate = serverTask.DueDate ?? DateTime.MinValue;
                            localTask.Completed = serverTask.Completed;
                            localTask.LastSyncDate = serverTask.LastSyncDate;

                            await _sqliteService.SaveTaskAsync(localTask);

                            // Aktualizujemy również na liście widocznej w interfejsie
                            var taskInList = Tasks.FirstOrDefault(t => t.Id == localTask.Id);
                            if (taskInList != null)
                            {
                                int index = Tasks.IndexOf(taskInList);
                                Tasks[index] = localTask; // Aktualizujemy element na liście
                            }
                            else
                            {
                                Tasks.Add(localTask); // Dodajemy tylko, jeśli jeszcze go nie ma
                            }
                        }
                    }
                    else
                    {
                        // Nowe zadanie z serwera - dodajemy do bazy lokalnej
                        serverTask.LastSyncDate = DateTime.UtcNow;
                        await _sqliteService.SaveTaskAsync(serverTask);

                        if (!Tasks.Any(t => t.Id == serverTask.Id))
                        {
                            Tasks.Add(serverTask); // Dodajemy zadanie do listy, tylko jeśli jeszcze go nie ma
                        }
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

                await DisplayAlert("Sukces", "Synchronizacja zadań zakończona pomyślnie", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Błąd", $"Wystąpił błąd podczas synchronizacji: {ex.Message}", "OK");
            }
        }
    }
}
