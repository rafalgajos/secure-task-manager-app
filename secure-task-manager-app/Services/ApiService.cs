using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using secure_task_manager_app.Models;

namespace secure_task_manager_app.Services
{
    public class ApiService
    {
        private static readonly HttpClient _httpClient;

        static ApiService()
        {
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true;

            // Setting BaseAddress to a fixed URL
            _httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri(App.ApiBaseUrl)
            };
        }

        private async System.Threading.Tasks.Task SetJwtToken(string jwtToken)
        {
            // Save the JWT token in an encrypted local file
            await SecureTokenStorage.SaveTokenAsync(jwtToken);
        }

        private async Task<string> GetJwtToken()
        {
            // Read JWT token from encrypted local file
            return await SecureTokenStorage.GetTokenAsync();
        }

        private void ClearJwtToken()
        {
            // Remove the JWT token from the encrypted local file
            SecureTokenStorage.ClearToken();
        }

        public async Task<bool> RegisterAsync(User user)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/register", user);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Registration failed: {errorContent}");
                    return false;
                }

                Console.WriteLine("User registered successfully.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during registration: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> LoginAsync(User user)
        {
            var response = await _httpClient.PostAsJsonAsync("/login", user);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(content);
                string token = result.token;

                await SetJwtToken(token); // Saving the token in an encrypted file
                Console.WriteLine("Login successful and JWT token stored securely.");
                return true;
            }

            Console.WriteLine("Login failed");
            return false;
        }

        public async Task<List<Models.Task>> GetTasksAsync()
        {
            try
            {
                var token = await GetJwtToken();
                if (string.IsNullOrEmpty(token))
                {
                    Console.WriteLine("Token missing or expired, re-login required.");
                    return null;
                }

                var request = new HttpRequestMessage(HttpMethod.Get, "/tasks");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    // Manually parse JSON response to handle nullable DateTime values
                    var tasksJson = await response.Content.ReadAsStringAsync();
                    var taskDictionaries = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(tasksJson);

                    var tasks = new List<Models.Task>();

                    foreach (var taskDict in taskDictionaries)
                    {
                        var task = new Models.Task
                        {
                            Id = Convert.ToInt32(taskDict["id"]),
                            Title = taskDict["title"]?.ToString(),
                            Description = taskDict["description"]?.ToString(),
                            DueDate = taskDict["due_date"] != null ? DateTime.Parse(taskDict["due_date"].ToString()) : (DateTime?)null,
                            Completed = Convert.ToBoolean(taskDict["completed"]),
                            LastSyncDate = DateTime.Parse(taskDict["last_sync_date"].ToString())
                        };

                        // Debug log to confirm due dates
                        Console.WriteLine($"Task ID: {task.Id}, DueDate: {task.DueDate}");
                        tasks.Add(task);
                    }

                    Console.WriteLine($"Fetched {tasks.Count} tasks from server.");
                    return tasks;
                }
                else
                {
                    Console.WriteLine($"Error fetching tasks: {response.ReasonPhrase}");
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error fetching tasks: {ex.Message}");
            }
            return null;
        }

        public async Task<bool> AddTaskAsync(Models.Task task)
        {
            var token = await GetJwtToken();

            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("Token missing or expired, re-login required.");
                return false;
            }

            var taskData = new
            {
                title = task.Title,
                description = task.Description,
                due_date = task.DueDate?.ToString("o"),
                completed = task.Completed,
                location = task.Location
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "/tasks")
            {
                Content = JsonContent.Create(taskData)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                if (content.Contains("Task already exists"))
                {
                    Console.WriteLine("Task already exists on the server.");
                    return true; // We acknowledge that the task is already synchronised
                }
                else
                {
                    var addedTask = Newtonsoft.Json.JsonConvert.DeserializeObject<Models.Task>(content);
                    task.Id = addedTask.Id;
                    return true;
                }
            }

            return false;
        }

        public async Task<bool> UpdateTaskAsync(Models.Task task)
        {
            var token = await GetJwtToken();

            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("Token missing or expired, re-login required.");
                return false;
            }

            // Convert DueDate to ISO 8601 format if not null
            var taskData = new
            {
                title = task.Title,
                description = task.Description,
                due_date = task.DueDate?.ToString("o"), // ISO 8601 format
                completed = task.Completed
            };

            var request = new HttpRequestMessage(HttpMethod.Put, $"/tasks/{task.Id}")
            {
                Content = JsonContent.Create(taskData)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteTaskAsync(int taskId)
        {
            var token = await GetJwtToken();

            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("Error: Token missing or corrupted. Please try synchronizing tasks.");
                return false;
            }

            var request = new HttpRequestMessage(HttpMethod.Delete, $"/tasks/{taskId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public void Logout()
        {
            ClearJwtToken(); // Clearing the token on log-off
            Console.WriteLine("User logged out and JWT token removed from secure storage.");
        }
    }
}
