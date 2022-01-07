
## Duplicate Files Finder

```
Usage: dff hash <path> -i <hash_file> [-m <min_file_size>]
    dff scan <path> [-o <out_file>] [-f <format>] [-m <min_file_size>] [-i <hash_file>]
    dff compare <path> -i <hash_file> [-o <out_file>] [-f <format>] [-m <min_file_size>]

Version: 0.6

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
-m <max_file_size>          A second value set the maximum file size.
-o <out_file>               Output file. If not specified output is written to a temporary file.
                            null  No file is written.
-i <hash_file>              A file containing a list of files and the corresponding MD5 hashs.
-f <format>                 The format of the output of 'scan'
                            path  A text file with file pathes.
                            del   A batch file containing 'del' statements.

Known Issues:
File access issues like denied access or readonly are not handles. Files that cannot be accessed are ignored 
silently.

Calling compare with hashes from the same location (<path>) will print out all existing duplicates.
For example if you hash 'D:\a.txt' and 'D:\a - copy.txt' (both same hash) and then call compare on D:\ it will 
report 'a.txt' and 'a - copy.txt' as duplicates.
```

## Example

1. List files in test folder:

```
C:\>cd dff

C:\dff>dir /S /B test

C:\dff\test\a
C:\dff\test\b
C:\dff\test\a\file1 copy.txt
C:\dff\test\a\file1.txt
C:\dff\test\a\file2.txt
C:\dff\test\b\file1.txt
```

2. Create a hash file of a:

```
C:\dff>dff hash test\a -i a-hash.txt

826E8142E6BAABE8AF779F5F490CF5F5 C:\dff\test\a\file1 copy.txt
1C1C96FD2CF8330DB0BFA936CE82F3B9 C:\dff\test\a\file2.txt
826E8142E6BAABE8AF779F5F490CF5F5 C:\dff\test\a\file1.txt
```

3. Compare hashes of a with b:

```
C:\dff>dff compare test\b -i a-hash.txt -f del -o duplicates.bat

@rem del /F "C:\Users\sasch\Documents\Programme\Home\dff\test\b\file1.txt"
```

4. Open duplicates.bat in your favorite editor and remove @rem for the file(s) you would like to delte

5. Run duplicates.bat


All of the above in one `scan`:

```
C:\dff>dff scan test\a test\b -f del -o duplicates.bat

@rem del /F "C:\Users\sasch\Documents\Programme\Home\dff\test\a\file1 copy.txt"
@rem del /F "C:\Users\sasch\Documents\Programme\Home\dff\test\b\file1.txt"
@rem del /F "C:\Users\sasch\Documents\Programme\Home\dff\test\a\file1.txt"
```

**Note that all three files are reported as duplicates!!! You have to decide which file to keep (leave the @rem). The program will group same files next to each other. The next group is separated by a blank line.**