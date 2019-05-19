using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace FileHashIndex
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            Console.WriteLine("file-hash-index v0.1");

            var options = new Options(args);
            if (options.DisplayHelp)
            {
                DisplayHelp();
                return;
            }

            Console.WriteLine($"Processing directory: {options.BasePath}");

            Data.IDatastore data;
            if (options.DatastoreType == DatastoreType.Md5SumFile)
                data = new Data.Md5SumFileDatastore();
            else if (options.DatastoreType == DatastoreType.Sqlite)
                data = new Data.SQLiteDatastore();
            else
                throw new Exception("No datastore set.");
            await data.Init(Path.Combine(options.BasePath, options.HashFilename));


            long count = await data.Count();
            if (count > 0)
            {
                Console.WriteLine($"Read {count} items from hash database.");
            }

            var files = Directory.GetFiles(options.BasePath, "*", SearchOption.AllDirectories);
            int newFileCount = 0;
            int modifiedFileCount = 0;

            foreach (var file in files)
            {
                if (Path.GetFileName(file) != options.HashFilename)
                {
                    string path = "." + Path.DirectorySeparatorChar + System.IO.Path.GetRelativePath(options.BasePath, file);
                    var hash = await data.GetHashInfo(path);
                    if (hash == null)
                    {
                        // New Hash
                        var fi = new FileInfo(file);
                        hash = new Data.HashInfo() {
                            Path = path,
                            Size = fi.Length,
                            ModifiedDate = fi.LastWriteTime,
                            Hash = CalculateMD5Hash(file),
                        };
                        newFileCount++;
                        Console.WriteLine($"{hash.Hash} *{hash.Path}");
                        await data.UpdateHashInfo(hash);
                    }
                    else
                    {
                        // Update existing hash if necessary
                        var fi = new FileInfo(file);
                        if (fi.LastWriteTime > hash.ModifiedDate
                            || (hash.Size >= 0 && hash.Size != fi.Length))
                        {
                            hash.Size = fi.Length;
                            hash.ModifiedDate = fi.LastWriteTime;
                            hash.Hash = CalculateMD5Hash(file);
                            modifiedFileCount++;
                            Console.WriteLine($"{hash.Hash} *{hash.Path}");
                            await data.UpdateHashInfo(hash);
                        }
                    }
                }
            }
            Console.WriteLine($"Found {newFileCount} new files.");
            Console.WriteLine($"Updated {modifiedFileCount} files.");

            await data.SaveAndClose();
        }

        private static void DisplayHelp()
        {
            Console.WriteLine("\nUSAGE: md5db [OPTIONS]\n");
            Console.WriteLine("Options:");
            Console.WriteLine("  -h, --help     Display Help");
            Console.WriteLine("  -sqlite        Use SQLite database to store hashes");
            Console.WriteLine();
        }

        private static string CalculateMD5Hash(string fullPath)
        {
            using (var md5 = MD5.Create())
            using (var stream = File.OpenRead(fullPath))
            {
                string hash = BitConverter.ToString(md5.ComputeHash(stream))
                    .Replace("-", string.Empty)
                    .ToLowerInvariant();
                return hash;
            }
        }
    }
}
