using secure_task_manager_app.Models;
using secure_task_manager_app.Services;
using System.Collections.ObjectModel;

namespace secure_task_manager_app.Views
{
    public partial class TaskListPage : ContentPage
    {
        private readonly ApiService _apiService;
        public ObservableCollection<Models.Task> Tasks { get; set; }

        public TaskListPage()
        {
            InitializeComponent();
            _apiService = new ApiService();
            Tasks = new ObservableCollection<Models.Task>();
            TasksListView.ItemsSource = Tasks;
            LoadTasks();
        }

        // Ładowanie zadań z backendu
        private async void LoadTasks()
        {
            var tasks = await _apiService.GetTasksAsync();
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

        // Przechodzenie do ekranu dodawania nowego zadania
        private async void OnAddTaskClicked(object sender, EventArgs e)
        {
            var taskDetailPage = new TaskDetailPage(new Models.Task(), LoadTasks);
            await Navigation.PushAsync(taskDetailPage);
        }

        // Edytowanie zadania po tapnięciu
        private async void OnTaskTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item is Models.Task selectedTask)
            {
                var taskDetailPage = new TaskDetailPage(selectedTask, LoadTasks);
                await Navigation.PushAsync(taskDetailPage);
            }
            ((ListView)sender).SelectedItem = null;
        }
    }
}
