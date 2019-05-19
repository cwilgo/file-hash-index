using System;

namespace FileHashIndex
{
    public class Options
    {
        const string DEFAULT_FILENAME = "hash.md5";

        public Options()
        {
            BasePath = System.IO.Directory.GetCurrentDirectory();
            HashFilename = DEFAULT_FILENAME;
            DatastoreType = DatastoreType.Md5SumFile;
        }

        public Options(string[] args)
            : this()
        {
            ProcessCommandParameters(args);
        }

        public string BasePath { get; private set; }
        public string HashFilename { get; private set; }
        public bool DisplayHelp { get; private set; }
        public DatastoreType DatastoreType { get; private set; }

        public void ProcessCommandParameters(string[] args)
        {
            foreach (var item in args)
            {
                switch (item)
                {
                    case "-sqlite":
                        DatastoreType = DatastoreType.Sqlite;
                        if (HashFilename == DEFAULT_FILENAME)
                            HashFilename = "hash.sqlite";
                        break;
                    case "--help":
                    case "-h":
                        DisplayHelp = true;
                        break;
                }
            }
        }

    }
}