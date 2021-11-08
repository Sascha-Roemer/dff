```
Usage: dff hash <path> -i <hash_file> [-m <min_file_size>]
       dff scan <path> [-o <out_file>] [-f <format>] [-m <min_file_size>] [-i <hash_file>]
       dff compare <path> -i <hash_file> [-o <out_file>] [-f <format>] [-m <min_file_size>]

Version: 0.5

Description:
  Duplicate Files Finder searches for similar files by size and content. Two files are considered to be duplicages if 
  the MD5 of the file contents is the same. Hashing can be done in a separate step to interrupt and resume the process 
  for large/slow drives.

Commands:
  scan     Searches for duplicates in the specified <path>. If no <hash-file> is specified all duplicate files by size 
           are hashed on-the-fly.
  hash     Creates a hash for each file and writes it to <hash_file>. It can be used as a pre-processing step for 
           'scan' to generate a lookup for file hashes. If <hash_file> already exists, new hashes will be appended.
           The hash file can be used as a cache for 'scan' and 'compare' that will look up hashes there and create a 
           new hash only if no hash is found there.
  compare  Uses <hash_file> as the primary file source. Files from <path> are compared to <hash_file>. If a matching 
           hash is found in <hash_file> the file fom <path> is considered to be a duplicate. Useful to hash one 
           location and then compare the hashes to multiple other locations. A second -i <hash_file> is used as hash 
           for <path>.

Arguments:
  <path>[ <path2>[ ...]]  One or multiples pathes that will be scanned recursively.

Options:
  -m <min_file_size>          The minimum file size that will be processed.
  -o <out_file>               Output file. If not specified output is written to a temporary file.
                                null  No file is written.
  -i <hash_file>              A file containing a list of files and the corresponding MD5 hashs.
  -f <format>                 The format of the output of 'scan'
                                path  A text file with file pathes.
                                del   A batch file containing 'del' statements.

Known Issues:
  A file stream is not properly closed and throws an exception before the program quits.

  File access issues like denied access or readonly are not handles. Files that cannot be accessed are ignored 
  silently.

  Calling compare with hashes from the same location (<path>) will print out all existing duplicates.
  For example if you hash 'D:\a.txt' and 'D:\a - copy.txt' (both same hash) and then call compare on D:\ it will 
  report 'a.txt' and 'a - copy.txt' as duplicates.
```