using System.Collections.Concurrent;

namespace BereansPath.Api.Diagnostics;

public sealed class AppLogStore
{
    private readonly ConcurrentQueue<string> _lines = new();
    private readonly object _fileLock = new();
    private readonly string _logDirectory;
    private readonly string _logFilePath;
    private const int MaxBufferedLines = 800;

    public AppLogStore(IHostEnvironment env)
    {
        _logDirectory = Path.Combine(env.ContentRootPath, "logs");
        Directory.CreateDirectory(_logDirectory);
        _logFilePath = Path.Combine(_logDirectory, "bereans-api.log");
    }

    public string LogFilePath => _logFilePath;

    public void Write(string level, string category, string message)
    {
        var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {category}: {message}";
        _lines.Enqueue(line);
        while (_lines.Count > MaxBufferedLines && _lines.TryDequeue(out _)) { }

        try
        {
            lock (_fileLock)
            {
                File.AppendAllText(_logFilePath, line + Environment.NewLine);
            }
        }
        catch
        {
            // Never break the app because logging failed.
        }
    }

    public IReadOnlyList<string> GetRecent(int take = 200)
    {
        take = Math.Clamp(take, 1, MaxBufferedLines);
        return _lines.Reverse().Take(take).Reverse().ToList();
    }

    public void ClearBuffer()
    {
        while (_lines.TryDequeue(out _)) { }
        Write("INFO", "AppLogStore", "In-memory log buffer cleared.");
    }
}
