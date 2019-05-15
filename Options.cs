using System;

namespace FileHashIndex
{
    public class Options
    {
        public Options()
        {
            BasePath = System.IO.Directory.GetCurrentDirectory();
            HashFilename = "hash.md5";
        }

        public Options(string[] args)
            : this()
        {
            ProcessCommandParameters(args);
        }

        public string BasePath { get; private set; }
        public string HashFilename { get; private set; }
        public bool DisplayHelp { get; private set; }

        public void ProcessCommandParameters(string[] args)
        {
            foreach (var item in args)
            {
                switch (item)
                {
                    case "--help":
                    case "-h":
                        DisplayHelp = true;
                        break;
                }
            }
        }

    }
}