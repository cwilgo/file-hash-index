using System;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace FileHashIndex.Data
{
    public class SQLiteDatastore : IDatastore
    {
        private SqliteConnection connection;

        public async Task Init(string path)
        {
            string connectionString = $"Data Source={path}";
            connection = new SqliteConnection(connectionString);
            await CreateDatabaseSchema();
        }

        private async Task CreateDatabaseSchema()
        {
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"CREATE TABLE IF NOT EXISTS Hash (
                    [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    [Hash] TEXT,
                    [HashType] TEXT,
                    [Path] TEXT
                )";
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}