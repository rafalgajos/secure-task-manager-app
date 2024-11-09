using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using secure_task_manager_app.Models;

namespace secure_task_manager_app.Services
{
    public class SQLiteService
    {
        private readonly SQLiteAsyncConnection _database;

        public SQLiteService(string password)
        {
            var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "tasks_secure.db");

            var options = new SQLiteConnectionString(dbPath, true, key: password); // Szyfrowanie bazy danych
            _database = new SQLiteAsyncConnection(options);
            _database.CreateTableAsync<Models.Task>().Wait();
        }

        public async Task<List<Models.Task>> GetTasksAsync()
        {
            return await _database.Table<Models.Task>().ToListAsync();
        }

        public async Task<int> SaveTaskAsync(Models.Task task)
        {
            try
            {
                if (task.Id != 0)
                {
                    return await _database.UpdateAsync(task);
                }
                else
                {
                    return await _database.InsertAsync(task);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving task: {ex.Message}");
                return 0;
            }
        }

        public async Task<int> DeleteTaskAsync(Models.Task task)
        {
            return await _database.DeleteAsync(task);
        }
    }
}
