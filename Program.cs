using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace FileHashIndex
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("file-hash-index v0.1");

            var options = new Options(args);
            if (options.DisplayHelp)
            {
                DisplayHelp();
                return;
            }

            Console.WriteLine($"Processing directory: {options.BasePath}");
            var list = new SortedDictionary<string, string>();
            DateTime updateDate = DateTime.MinValue;

            if (File.Exists(Path.Combine(options.BasePath, options.HashFilename)))
            {
                Console.WriteLine($"Found hash database: {options.HashFilename}");
                updateDate = File.GetLastWriteTime(Path.Combine(options.BasePath, options.HashFilename));
                using (var hashdb = new StreamReader(Path.Combine(options.BasePath, options.HashFilename)))
                {
                    while (!hashdb.EndOfStream)
                    {
                        string line = hashdb.ReadLine();
                        list.Add(line.Substring(34), line.Substring(0, 32));
                    }
                }
            }

            if (list.Count > 0)
            {
                Console.WriteLine($"Read {list.Count} items from hash database.");
            }

            var files = Directory.GetFiles(options.BasePath, "*", SearchOption.AllDirectories);
            int newFileCount = 0;
            int modifiedFileCount = 0;

            using (var md5 = MD5.Create())
            {
                foreach (var file in files)
                {
                    if (Path.GetFileName(file) != options.HashFilename)
                    {
                        string path = "." + Path.DirectorySeparatorChar + System.IO.Path.GetRelativePath(options.BasePath, file);
                        if (!list.ContainsKey(path))
                        {
                            list.Add(path, string.Empty);
                            //newFileCount++;
                        }
                        if (string.IsNullOrEmpty(list.GetValueOrDefault(path, null)) ||
                            File.GetLastWriteTime(file) > updateDate
                            )
                        {
                            using (var stream = File.OpenRead(file))
                            {
                                string hash = BitConverter.ToString(md5.ComputeHash(stream))
                                    .Replace("-", string.Empty)
                                    .ToLowerInvariant();
                                if (list.GetValueOrDefault(path, string.Empty) != hash)
                                {
                                    if (string.IsNullOrEmpty(list.GetValueOrDefault(path, string.Empty)))
                                        newFileCount++;
                                    else
                                        modifiedFileCount++;
                                    list.Remove(path);
                                    list.Add(path, hash);
                                    Console.WriteLine($"{hash} *{path}");
                                }
                            }
                        }
                    }
                }
            }
            Console.WriteLine($"Found {newFileCount} new files.");
            Console.WriteLine($"Updated {modifiedFileCount} files.");

            string outputPath = Path.Combine(options.BasePath, options.HashFilename);
            using (var output = new StreamWriter(outputPath))
            {
                foreach (var item in list)
                {
                    output.WriteLine($"{item.Value} *{item.Key}");
                }
            }
        }

        private static void DisplayHelp()
        {
            Console.WriteLine("\nUSAGE: md5db [OPTIONS]\n");
            Console.WriteLine("Options:");
            Console.WriteLine("  -h, --help     Display Help");
            Console.WriteLine();
        }
    }
}
