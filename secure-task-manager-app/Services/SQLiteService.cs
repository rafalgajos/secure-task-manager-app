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
            if (task.Id != 0)
            {
                return await _database.UpdateAsync(task);
            }
            else
            {
                return await _database.InsertAsync(task);
            }
        }

        public async Task<int> DeleteTaskAsync(Models.Task task)
        {
            return await _database.DeleteAsync(task);
        }
    }
}
