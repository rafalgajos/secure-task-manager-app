using SQLite;
using System;

namespace secure_task_manager_app.Models
{
    public class Task
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? DueDate { get; set; } = null;
        public bool Completed { get; set; }
        public DateTime LastSyncDate { get; set; }

        // New property for storing location
        public string Location { get; set; } // JSON string with latitude and longitude
    }

}
