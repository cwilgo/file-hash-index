# file-hash-index

Tool for creating and updating a database of file hashes

This tool is in the very early stages of development. Essentially I wanted a tool that can both create and update a text file that is compatible with the md5sum tool. Instead of using md5sum to create the file, which requires all files have their hash calculated every time it is run, this tool will read the hash file first and then only calculate a hash for new files that are not yet in the file. It will also recalculate hashes for files that have been modified since the hash file was created.
