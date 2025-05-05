using SQLite;

namespace TaskManagement.Data.Sqlite
{
    public class SqliteService(SQLiteAsyncConnection connection) : IDatabaseService
    {
        public async Task<int> DeleteTask(int taskId)
        {
            return await connection.DeleteAsync(taskId);
        }

        public async Task<IEnumerable<ITMTask>> GetAllTasksAsync()
        {
            return await connection.Table<TMTask>().ToListAsync();
        }

        public async Task<ITMTask> GetTaskByIdAsync(int taskId)
        {
            return await connection.Table<TMTask>().Where(t => t.Id == taskId).FirstOrDefaultAsync();
        }

        public async Task<int> SaveTask(ITMTask task)
        {
            return task.Id != 0 ? await connection.UpdateAsync(task) : await connection.InsertAsync(task);
        }
    }
}
