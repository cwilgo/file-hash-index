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
            await connection.OpenAsync();
            await CreateDatabaseSchema();
        }

        private async Task CreateDatabaseSchema()
        {
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"CREATE TABLE IF NOT EXISTS File (
                    [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    [Path] TEXT
                )";
                await cmd.ExecuteNonQueryAsync();
                cmd.CommandText = @"CREATE TABLE IF NOT EXISTS Hash (
                    [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    [FileId] INTEGER NOT NULL,
                    [Hash] TEXT,
                    [HashType] TEXT,
                    [Size] INTEGER,
                    [ModifiedDate] DATETIME,
                    [Timestamp] DATETIME,
                    FOREIGN KEY (FileId) REFERENCES File(Id)
                )";
                await cmd.ExecuteNonQueryAsync();
                cmd.CommandText = @"CREATE INDEX IF NOT EXISTS idx_FilePath ON File(Path)";
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task<long> Count()
        {
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"SELECT COUNT([Id]) FROM File";
                object result = await cmd.ExecuteScalarAsync();
                return (long)result;
            }
        }

        public async Task<HashInfo> GetHashInfo(string path)
        {
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText =
                    @"SELECT 
	                    Hash.Id AS HashId, 
	                    [FileId], 
	                    Path, 
	                    Hash,
	                    Size,
	                    ModifiedDate,
	                    Timestamp
                    FROM Hash 
                    INNER JOIN File ON File.Id = Hash.FileId
                    WHERE File.Path = @Path";
                cmd.Parameters.Add(new SqliteParameter("Path", path));
                var result = await cmd.ExecuteReaderAsync();
                if (await result.ReadAsync())
                {
                    var hash = new HashInfo() {
                        HashId = result.GetInt64(0),
                        FileId = result.GetInt64(1),
                        Path = result.GetString(2),
                        Hash = result.GetString(3),
                        Size = result.GetInt64(4),
                        ModifiedDate = result.GetDateTime(5),
                    };
                    return hash;
                }
                else
                {
                    return null;
                }
            }
        }

        public async Task<HashInfo> UpdateHashInfo(HashInfo hash)
        {
            var nextHash = await InsertOrUpdateFile(hash);
            nextHash = await InsertOrUpdateHash(hash);
            return nextHash;
        }

        private async Task<HashInfo> InsertOrUpdateFile(HashInfo hash)
        {
            using (var cmd = connection.CreateCommand())
            {
                if (hash.FileId < 0)
                {
                    cmd.CommandText = "INSERT INTO File(Path) VALUES (@Path); SELECT last_insert_rowid();";
                    cmd.Parameters.Add(new SqliteParameter("@Path", hash.Path));
                    hash.FileId = (long) await cmd.ExecuteScalarAsync();
                }
                else
                { 
                    //cmd.CommandText = "UPDATE File SET Path=@Path WHERE Id=@Id";
                    //cmd.Parameters.Add(new SqliteParameter("@Path", hash.Path));
                    //cmd.Parameters.Add(new SqliteParameter("@Id", hash.FileId));
                    //await cmd.ExecuteNonQueryAsync();
                }
                return hash;
            }
        }

        private async Task<HashInfo> InsertOrUpdateHash(HashInfo hash)
        {
            using (var cmd = connection.CreateCommand())
            {
                if (hash.HashId < 0)
                {
                    cmd.CommandText = "INSERT INTO Hash(FileId, Hash, HashType, Size, ModifiedDate, Timestamp) "
                        + "VALUES (@FileId, @Hash, 'MD5', @Size, @ModifiedDate, @Timestamp); "
                        + "SELECT last_insert_rowid();";
                    cmd.Parameters.Add(new SqliteParameter("@FileId", hash.FileId));
                    cmd.Parameters.Add(new SqliteParameter("@Hash", hash.Hash));
                    cmd.Parameters.Add(new SqliteParameter("@Size", hash.Size));
                    cmd.Parameters.Add(new SqliteParameter("@ModifiedDate", hash.ModifiedDate));
                    cmd.Parameters.Add(new SqliteParameter("@Timestamp", DateTime.Now));
                    hash.HashId = (long)await cmd.ExecuteScalarAsync();
                }
                else
                {
                    cmd.CommandText = @"UPDATE Hash SET 
                        FileId=@FileId, 
                        Hash=@Hash, 
                        Size=@Size, 
                        ModifiedDate=@ModifiedDate, 
                        Timestamp=@Timestamp 
                        WHERE Id=@Id";
                    cmd.Parameters.Add(new SqliteParameter("@Id", hash.HashId));
                    cmd.Parameters.Add(new SqliteParameter("@FileId", hash.FileId));
                    cmd.Parameters.Add(new SqliteParameter("@Hash", hash.Hash));
                    cmd.Parameters.Add(new SqliteParameter("@Size", hash.Size));
                    cmd.Parameters.Add(new SqliteParameter("@ModifiedDate", hash.ModifiedDate));
                    cmd.Parameters.Add(new SqliteParameter("@Timestamp", DateTime.Now));
                    await cmd.ExecuteNonQueryAsync();
                }
                return hash;
            }
        }

        public async Task SaveAndClose()
        {
            connection.Close();
        }
    }
}