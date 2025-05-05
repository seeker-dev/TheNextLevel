using SQLite;

namespace TaskManagement.Data.Sqlite
{
    [Table("DatabaseVersionInfo")]
    public class DatabaseVersionInfo
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int Version { get; set; }
    }
}
