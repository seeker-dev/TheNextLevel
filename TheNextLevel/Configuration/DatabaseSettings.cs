namespace TheNextLevel.Configuration;

public class DatabaseSettings
{
    public string Provider { get; set; } = "SQLite";
    public ConnectionStrings ConnectionStrings { get; set; } = new();
}

public class ConnectionStrings
{
    public string SQLite { get; set; } = string.Empty;
    public string SqlServer { get; set; } = string.Empty;
}
