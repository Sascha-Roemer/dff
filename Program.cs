using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace dff
{
    class Program
    {
        private static Hash _hash = new();
        private static string _hashFile;
        private static int _minFileSize;
        #region Statistics
        private static int _numScannedFiles = 0;
        private static int _numDuplicatesBySize = 0;
        private static int _numDuplicatesByQuickHash = 0;
        private static int _numDuplicatesByFullHash = 0;
        private static TimeSpan _timeQuickHash;
        private static TimeSpan _timeFullHash;
        private static int _numSizeBuckets;
        private static int _numHashesRead;
        #endregion

        static void Main(string[] args)
        {
            try
            {
                #region Help
                if (args.Length == 0)
                {
                    Console.WriteLine(@"
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
");
                    return;
                }
                #endregion

                #region Init
                _minFileSize = args.GetInt("-m", 0);
                _hashFile = args.GetOne("-i");
                if (_hashFile != null) Console.WriteLine($"Reading hash file. {Time}");
                _numHashesRead = _hash.ReadHashfile(_hashFile);
                #endregion

                if (args.IsCommand("scan"))
                Scan(args.GetAll("scan"), args.GetOne("-o"), _minFileSize, args.GetOne("-f"));

                if (args.IsCommand("hash", "-i"))
                Hash(args.GetAll("hash"), _minFileSize);

                if (args.IsCommand("compare", "-i"))
                Compare(args.GetAll("compare"), args.GetOne("-o"), _minFileSize, args.GetOne("-i", 2), args.GetOne("-f"));

                WriteStatistic();
            }
            finally
            {
                _hash.Dispose();
            }
        }
        
        private static void Hash(IEnumerable<string> paths, int minSize)
        {
            Console.WriteLine($"Hashing files. {Time}");
            using var stream = new FileStreamWriter(_hashFile, SeekOrigin.End);

            Parallel.ForEach(
                paths.GetFiles(minSize),
                info =>
            {
                if (_hash.GetMD5(info.FullName, out var hash))
                {
                    stream.WriteLine(hash + " " + info.FullName);
                }
            });
        }

        private static void Scan(IEnumerable<string> paths, string outfile, int minSize, string outputFormat)
        {
            Console.WriteLine($"Scanning files. {Time}");
            var byteLookup = new ConcurrentDictionary<string, List<string>>();
            
            using var stream = new FileStreamWriter(outfile, SeekOrigin.End);
            Console.WriteLine($"Writing duplicates to '{stream.FilePath}'");

            var sameSize = GetSameFilesBySize(paths, minSize).ToArray();
            var time = System.Diagnostics.Stopwatch.StartNew();
            
            Parallel.ForEach(sameSize, e =>
                Add(
                    lookup: byteLookup,
                    key: _hash.GetMD5(e, 4096),
                    value: e));

            var candidates = byteLookup.Where(e => e.Value.Count > 1).SelectMany(e => e.Value).ToArray();
            _numDuplicatesByQuickHash = candidates.Length;
            _timeQuickHash = time.Elapsed;
            byteLookup.Clear();
            time.Restart();

            Parallel.ForEach(candidates, e =>
                Add(
                    lookup: byteLookup,
                    key: _hash.GetMD5(e),
                    value: e));

            var sameHash = byteLookup.Where(e => e.Value.Count > 1).ToArray();
            _timeFullHash = time.Elapsed;

            foreach (var e in sameHash)
            {
                stream.WriteLine();

                foreach (var i in e.Value)
                {
                    WriteScanFile(stream, i, outputFormat);
                    _numDuplicatesByFullHash++;
                }
            }
        }

        private static void Compare(IEnumerable<string> paths, string outfile, int minSize, string pathHashFile, string outputFormat)
        {
            Console.WriteLine($"Comparing files. {Time}");
            var duplicates = new List<string>();
            
            using var stream = new FileStreamWriter(outfile, SeekOrigin.End);
            Console.WriteLine($"Writing duplicates to '{stream.FilePath}'");

            using var hash = new Hash();
            hash.ReadHashfile(pathHashFile);
            
            Parallel.ForEach(paths.GetFiles(minSize), e =>
            {
                if (_hash.HasDuplicate(e.FullName, hash.GetMD5(e.FullName)))
                {
                    WriteScanFile(stream, e.FullName, outputFormat);
                    _numDuplicatesByFullHash++;
                }
            });
        }

        private static void WriteScanFile(FileStreamWriter stream, string text, string format)
        {
            switch (format)
            {
                case "del":
                    stream.WriteLine($"@rem del /F \"{text}\"");
                    break;
                default:
                    stream.WriteLine(text);
                    break;
            }
        }

        private static IEnumerable<string> GetSameFilesBySize(IEnumerable<string> paths, int minSize)
        {
            if (!paths.Any()) paths = new[] { ".\\" };

            var sizeLookup = new Dictionary<long, string>();
            
            foreach(var info in paths.GetFiles(minSize))
            {
                _numScannedFiles++;
                foreach(var e in Add(sizeLookup, info.Length, info.FullName))
                {
                    _numDuplicatesBySize++;
                    yield return e;
                }
            }

            _numSizeBuckets = sizeLookup.Count;
        }

        private static void Add<TKey>(IDictionary<TKey, List<string>> lookup, TKey key, string value)
        {
            if (key == null) return;

            List<string> list;

            lock(lookup)
            {
                if (!lookup.TryGetValue(key, out list))
                {
                    list = new();
                    lookup[key] = list;
                }
            }

            list.Add(value);
        }

        private static IEnumerable<string> Add<TKey>(IDictionary<TKey, string> lookup, TKey key, string file)
        {
            if (key != null)
            {
                if (!lookup.TryGetValue(key, out var value))
                {
                    lookup[key] = file;
                }
                else if (value != null)
                {
                    lookup[key] = null;
                    yield return value;
                    yield return file;
                }
                else
                {
                    yield return file;
                }
            }
        }

        private static void WriteStatistic()
        {
            Console.WriteLine("\nNumbers:");
            Console.WriteLine($"  Minimum file size ............. {_minFileSize} kb");
            Console.WriteLine($"  Number of pre-hashed files .... {_numHashesRead}");
            Console.WriteLine($"  Files scanned ................. {_numScannedFiles}");
            Console.WriteLine($"  Number of size buckets ........ {_numSizeBuckets}");
            Console.WriteLine($"  Duplicates by size ............ {_numDuplicatesBySize}");
            Console.WriteLine($"  Duplicates by 4096 bytes hash . {_numDuplicatesByQuickHash}");
            Console.WriteLine($"  Duplicates by full hash ....... {_numDuplicatesByFullHash}");
            Console.WriteLine($"  Time 4096 hash ................ {_timeQuickHash}");
            Console.WriteLine($"  Time full hash ................ {_timeFullHash}");
        }

        private static TimeSpan Time => DateTime.Now.TimeOfDay;
    }
}
