namespace dff;

public class Hash : IDisposable, ICloneable
{
    private bool isDisposed;
    private ConcurrentDictionary<string, HashSet<string>> _hashList = new();
    private ConcurrentDictionary<string, string> _fileList = new();
    private MD5 _md5 = MD5.Create();

    public string GetMD5(string file)
    =>
    GetMD5(file, out var value, 0) ? value : value;

    public string GetMD5(string file, int count)
    =>
    GetMD5(file, out var value, count) ? value : value;

    public bool GetMD5(string file, out string value, int count = 0)
    {
        if (_fileList.TryGetValue(file, out value))
        {
            return false;
        }

        try
        {
            lock(_md5)
            {
                using var stream = File.OpenRead(file);
                if (count > 0)
                {
                    var buffer = new byte[count];
                    stream.Read(buffer, 0, count);
                    value = BytesToString(_md5.ComputeHash(buffer));
                }
                else
                value = BytesToString(_md5.ComputeHash(stream));
                return true;
            }
        }
        catch(Exception)
        {
            value = null;
            return false;
        }
    }

    public int ReadHashfile(string file)
    {
        if (string.IsNullOrEmpty(file)) return 0;

        if (!File.Exists(file)) return 0;

        var linesRead = 0;

        foreach(var line in File.ReadLines(file))
        {
            if (line.Length < 37) continue;

            linesRead++;
            
            var hashPart = line.Substring(0, 32);
            var filePart =line.Substring(33);
            if (!_hashList.TryGetValue(hashPart, out var list))
            {
                list = new HashSet<string>{ filePart };
                _hashList[hashPart] = list;
            }
            else
            {
                list.Add(filePart);
            }
            _fileList[filePart] = hashPart;
        }

        return linesRead;
    }

    public bool HasDuplicate(string file, string hash)
    =>
    _hashList.TryGetValue(hash ?? GetMD5(file), out var files)
    && files.Any(e => !string.Equals(e, file, StringComparison.OrdinalIgnoreCase));

    public object Clone() =>
    new Hash(){ _fileList = _fileList, _hashList = _hashList };
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (isDisposed) return;

        if (disposing)
        {
            _md5.Dispose();
        }

        isDisposed = true;
    }

    private static string BytesToString(byte[] value)
    {
        if (value == null) return null;

        var result = new StringBuilder();
        foreach(var b in value)
        {
            result.Append(b.ToString("X2"));
        }
        return result.ToString();
    }
}