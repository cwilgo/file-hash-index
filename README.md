# file-hash-index

Tool for creating and updating a database of file hashes

This tool is in the very early stages of development. Essentially I wanted a tool that can both create and update a text file that is compatible with the md5sum tool. Instead of using md5sum to create the file, which requires all files have their hash calculated every time it is run, this tool will read the hash file first and then only calculate a hash for new files that are not yet in the file. It will also recalculate hashes for files that have been modified since the hash file was created.

## Project Goals

1. Tool to create and update a database of file hashes
2. Learn .Net Core
3. Learn Visual Studio Code
4. Learn how to package apps for linux distribution

## Next Steps

- Complete command line options - currently nothing but help exists
- Add support for SHA1 hashes, possibly others
- Add support to store hashes in an actual database, such as sqlite
