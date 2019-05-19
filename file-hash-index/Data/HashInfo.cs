using System;
using System.Collections.Generic;
using System.Text;

namespace FileHashIndex.Data
{
    public class HashInfo
    {
        public HashInfo()
        {
            FileId = -1;
            HashId = -1;
            ModifiedDate = DateTime.MinValue;
            Size = -1;
        }

        public long FileId { get; set; }
        public long HashId { get; set; }
        public string Path { get; set; }
        public string Hash { get; set; }
        public DateTime ModifiedDate { get; set; }
        public long Size { get; set; }
    }
}
