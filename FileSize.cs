namespace dff;

public class FileSize
{
    public int NumScannedFiles { get; private set; }
    public int NumDuplicatesBySize { get; private set; }
    public int NumSizeBuckets { get; private set; }

    public IEnumerable<string> GetSameFilesBySize(IEnumerable<string> paths, int minSize, int maxSize)
    {
        if (!paths.Any()) paths = new[] { ".\\" };

        Dictionary<long, (string file, int count)> sizeLookup = new();
        
        foreach(var info in paths.GetFiles(minSize, maxSize))
        {
            NumScannedFiles++;
            foreach(var e in Add(sizeLookup, info.Length, info.FullName))
            {
                NumDuplicatesBySize++;
                yield return e;
            }
        }

        NumSizeBuckets = sizeLookup.Count;
    }

    public Dictionary<long, int> GroupFileCountBySize(IEnumerable<string> paths, int minSize, int maxSize) =>
        paths.GetFiles(minSize, maxSize)
            .GroupBy(k => k.Length, e => 1)
            .ToDictionary(k => k.Key, e => e.Count());

    private static IEnumerable<string> Add<TKey>(IDictionary<TKey, (string file, int count)> lookup, TKey key, string file)
    {
        if (key != null)
        {
            if (!lookup.TryGetValue(key, out var value))
            {
                lookup[key] = (file, 1);
            }
            else if (value.count == 1)
            {
                value.count++;
                yield return value.file;
                yield return file;
            }
            else
            {
                value.count++;
                yield return file;
            }
        }
    }
}