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

        public SQLiteService()
        {
            var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "tasks.db");
            _database = new SQLiteAsyncConnection(dbPath);
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
                    Console.WriteLine($"Updating task with Id: {task.Id}");
                    return await _database.UpdateAsync(task);
                }
                else
                {
                    Console.WriteLine($"Inserting new task with title: {task.Title}");
                    return await _database.InsertAsync(task);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving task: {ex.Message}");
                return 0; // oznacza, że zapis się nie udał
            }
        }


        public async Task<int> DeleteTaskAsync(Models.Task task)
        {
            return await _database.DeleteAsync(task);
        }
    }
}
