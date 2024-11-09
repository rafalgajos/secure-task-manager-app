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
            _sqliteService = new SQLiteService();
            _apiService = new ApiService();
            Tasks = new ObservableCollection<Models.Task>();
            TasksListView.ItemsSource = Tasks;

            LoadTasks();

            // Nasłuchuj wiadomości z LoginPage dotyczącej synchronizacji zadań
            MessagingCenter.Subscribe<LoginPage>(this, "SyncTasks", async (sender) =>
            {
                await SyncTasksWithServer();
            });
        }

        // Ładowanie zadań z lokalnej bazy danych
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

        // Dodawanie nowego zadania
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


        // Przechodzenie do strony logowania w celu synchronizacji zadań
        private async void OnSyncClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new LoginPage());
        }

        // Synchronizacja zadań z serwerem (po udanym logowaniu)
        private async System.Threading.Tasks.Task SyncTasksWithServer()
        {
            try
            {
                // Pobierz zadania z serwera
                var serverTasks = await _apiService.GetTasksAsync();
                if (serverTasks == null)
                {
                    await DisplayAlert("Błąd", "Nie udało się pobrać zadań z serwera", "OK");
                    return;
                }

                // Pobierz lokalne zadania
                var localTasks = await _sqliteService.GetTasksAsync();

                // Tworzymy słownik lokalnych zadań dla szybszego dostępu po Id
                var localTaskDict = localTasks.ToDictionary(task => task.Id);

                // Synchronizacja - Dodanie lub aktualizacja zadań z serwera do lokalnej bazy
                foreach (var serverTask in serverTasks)
                {
                    if (localTaskDict.TryGetValue(serverTask.Id, out var localTask))
                    {
                        // Zadanie istnieje lokalnie, sprawdzamy czy wymaga aktualizacji
                        if (serverTask.LastSyncDate > localTask.LastSyncDate)
                        {
                            // Zadanie na serwerze jest nowsze - zaktualizuj lokalne zadanie
                            localTask.Title = serverTask.Title;
                            localTask.Description = serverTask.Description;
                            localTask.DueDate = serverTask.DueDate;
                            localTask.Completed = serverTask.Completed;
                            localTask.LastSyncDate = serverTask.LastSyncDate;
                            await _sqliteService.SaveTaskAsync(localTask);

                            // Upewnij się, że zadanie jest widoczne na liście bez duplikatów
                            if (!Tasks.Contains(localTask))
                            {
                                Tasks.Add(localTask);
                            }
                            Console.WriteLine($"Updated local task with server data: {serverTask.Title}");
                        }
                    }
                    else
                    {
                        // Zadanie nie istnieje lokalnie - dodajemy je
                        serverTask.LastSyncDate = DateTime.UtcNow;
                        await _sqliteService.SaveTaskAsync(serverTask);

                        // Dodajemy zadanie do ObservableCollection, jeśli go jeszcze tam nie ma
                        if (!Tasks.Any(t => t.Id == serverTask.Id))
                        {
                            Tasks.Add(serverTask);
                        }
                        Console.WriteLine($"Task from server added locally: {serverTask.Title}");
                    }
                }

                // Synchronizacja - Dodanie lub aktualizacja lokalnych zadań na serwer
                foreach (var localTask in localTasks)
                {
                    // Sprawdzamy, czy zadanie lokalne nie jest jeszcze na serwerze lub jest nowsze
                    if (localTask.LastSyncDate == default)
                    {
                        // Zadanie lokalne nie było jeszcze zsynchronizowane - dodaj na serwer
                        if (await _apiService.AddTaskAsync(localTask))
                        {
                            localTask.LastSyncDate = DateTime.UtcNow; // Aktualizuj datę synchronizacji
                            await _sqliteService.SaveTaskAsync(localTask);

                            // Dodajemy zadanie do ObservableCollection, jeśli go jeszcze tam nie ma
                            if (!Tasks.Contains(localTask))
                            {
                                Tasks.Add(localTask);
                            }
                            Console.WriteLine($"Local task added to server: {localTask.Title}");
                        }
                    }
                    else if (serverTasks.FirstOrDefault(t => t.Id == localTask.Id) is Models.Task serverTask && localTask.LastSyncDate > serverTask.LastSyncDate)
                    {
                        // Zadanie lokalne jest nowsze - zaktualizuj na serwerze
                        if (await _apiService.UpdateTaskAsync(localTask))
                        {
                            localTask.LastSyncDate = DateTime.UtcNow;
                            await _sqliteService.SaveTaskAsync(localTask);
                            Console.WriteLine($"Updated task on server from local: {localTask.Title}");
                        }
                    }
                }

                await DisplayAlert("Sukces", "Synchronizacja zadań zakończona pomyślnie", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Błąd", $"Wystąpił błąd podczas synchronizacji: {ex.Message}", "OK");
                Console.WriteLine($"Exception during sync: {ex.Message}");
            }
        }
    }
}
