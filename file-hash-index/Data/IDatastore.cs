using System;
using System.Threading.Tasks;

namespace FileHashIndex.Data
{
    public interface IDatastore
    {
        Task Init(string path);
        Task<long> Count();
        Task<HashInfo> GetHashInfo(string path);
        Task<HashInfo> UpdateHashInfo(HashInfo hash);
        Task SaveAndClose();
    }
}