namespace TheNextLevel.Application.Interfaces
{
    public interface ISecureConfigurationService
    {
        /// <summary>
        /// Gets the database provider (SQLite or SqlServer)
        /// </summary>
        Task<string> GetDatabaseProviderAsync();

        /// <summary>
        /// Sets the database provider
        /// </summary>
        Task SetDatabaseProviderAsync(string provider);

        /// <summary>
        /// Gets the connection string for the specified provider
        /// </summary>
        Task<string?> GetConnectionStringAsync(string provider);

        /// <summary>
        /// Sets the connection string for the specified provider
        /// </summary>
        Task SetConnectionStringAsync(string provider, string connectionString);

        /// <summary>
        /// Tests if a connection string is valid
        /// </summary>
        Task<(bool isValid, string? errorMessage)> TestConnectionAsync(string provider, string connectionString);

        /// <summary>
        /// Clears all stored credentials
        /// </summary>
        Task ClearAllAsync();
    }
}
