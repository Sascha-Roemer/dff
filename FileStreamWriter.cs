namespace dff;

public class FileStreamWriter : IDisposable
{
    private bool isDisposed;
    private FileStream _file;
    private StreamWriter _stream;
    private string _path;
    private SeekOrigin _origin;
    private static object _initLock = new object();

    public string FilePath => _path;

    public FileStreamWriter(string path, SeekOrigin origin)
    {
        _path = path;
        _origin = origin;
    }

    public void WriteLine(string value)
    {
        Initialize();
        Console.WriteLine(value);
        lock(_stream)
        {
            _stream.WriteLine(value);
        }
    }

    public void WriteLine()
    {
        Initialize();
        Console.WriteLine();
        lock(_stream)
        {
            _stream.WriteLine();
        }
    }

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
            _stream?.Dispose();
            _file?.Dispose();
        }

        isDisposed = true;
    }

    private void Initialize()
    {
        if (_stream != null) return;
        lock(_initLock)
        {
            if (_stream != null) return;
            
            if (string.IsNullOrEmpty(_path))
                _path = Path.Combine(Path.GetTempPath(), Path.ChangeExtension(Guid.NewGuid().ToString(), ".txt"));

            _file =
            _path?.ToLowerInvariant() != "null"
            ? File.Open(_path, FileMode.OpenOrCreate)
            : null;

            _file?.Seek(0, _origin);

            _stream =
            _file != null
            ? new StreamWriter(_file)
            : StreamWriter.Null;

            _stream.AutoFlush = true;
        }
    }
}