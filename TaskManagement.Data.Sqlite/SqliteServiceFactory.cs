using Microsoft.Maui.Storage;
using SQLite;

namespace TaskManagement.Data.Sqlite
{
    public class SqliteServiceFactory : IDatabaseServiceFactory
    {
        // Factory method to create an instance of SqliteService
        public async Task<IDatabaseService> CreateAsync()
        {
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "nextlevel.db3");
            var connection = new SQLiteAsyncConnection(dbPath);
            await connection.CreateTableAsync<TMTask>();

            // Get the current database version (or 0 if not set)
            var dbVersionRecord = await connection.Table<DatabaseVersionInfo>()
                .FirstOrDefaultAsync() ?? new DatabaseVersionInfo { Version = 0 };

            if (dbVersionRecord.Version < 1)
            {
                // Initial database setup - add default categories or sample data
                dbVersionRecord.Version = 1;
            }

            // Add more migration steps here as your app evolves
            // if (dbVersionRecord.Version < 2) { ... }

            // Save the current database version
            if (dbVersionRecord.Id == 0)
                await connection.InsertAsync(dbVersionRecord);
            else
                await connection.UpdateAsync(dbVersionRecord);

            return new SqliteService(connection);
        }
    }
}
