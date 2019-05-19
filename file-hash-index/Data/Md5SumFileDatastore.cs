using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FileHashIndex.Data
{
    public class Md5SumFileDatastore : IDatastore
    {
        private string path;
        private SortedDictionary<string, string> list = new SortedDictionary<string, string>();
        private DateTime updateDate = DateTime.MinValue;

        public async Task Init(string path)
        {
            this.path = path;
            if (File.Exists(path))
            {
                Console.WriteLine($"Found hash database: {path}");
                updateDate = File.GetLastWriteTime(path);
                using (var hashdb = new StreamReader(path))
                {
                    while (!hashdb.EndOfStream)
                    {
                        string line = await hashdb.ReadLineAsync();
                        list.Add(line.Substring(34), line.Substring(0, 32));
                    }
                }
            }
        }

        public async Task<long> Count()
        {
            return list.Count;
        }

        public async Task<HashInfo> GetHashInfo(string path)
        {
            string hash;
            if (list.TryGetValue(path, out hash))
            {
                return new HashInfo() {
                    Path = path,
                    Hash = hash,
                    ModifiedDate = updateDate,
                    Size = -1 // Md5SumFile does not store file size
                };
            }
            else
            {
                return null;
            }
        }

        public async Task<HashInfo> UpdateHashInfo(HashInfo hash)
        {
            if (list.ContainsKey(hash.Path))
            { 
                list.Remove(hash.Path);
                list.Add(hash.Path, hash.Hash);
            }
            else
            {
                list.Add(hash.Path, hash.Hash);
            }
            return hash;
        }

        public async Task SaveAndClose()
        {
            string outputPath = Path.Combine(path);
            using (var output = new StreamWriter(outputPath))
            {
                foreach (var item in list)
                {
                    await output.WriteLineAsync($"{item.Value} *{item.Key}");
                }
            }
        }
    }
}
