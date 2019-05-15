using System;
using System.Threading.Tasks;

namespace FileHashIndex.Data
{
    public interface IDatastore
    {
        Task Init(string path);
    }
}