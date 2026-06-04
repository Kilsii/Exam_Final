
using System.Text;

namespace PasswordBruteForce;

public class PerformanceLogger
{
    // State 

    private readonly List<string> _entries = [];
    private long _singleMs = -1;    // -1 = not yet recorded
    private long _multiMs = -1;
    private int _threads = 0;

    // Properties
       
    public bool HasBothResults => _singleMs >= 0 && _multiMs >= 0;     ///True when both single-thread and multi-thread results have been logged.

    ///Records the single-thread result.
    public void LogSingleThread(string password, long elapsedMs)
    {
        _singleMs = elapsedMs;
        _entries.Add($"[SINGLE-THREAD]              Found '{password}'  in {elapsedMs,6} ms");
    }

    ///Records the multi-thread result.
    public void LogMultiThread(string password, long elapsedMs, int threadCount)
    {
        _multiMs = elapsedMs;
        _threads = threadCount;
        _entries.Add($"[MULTI-THREAD ({threadCount,2} threads)]  Found '{password}'  in {elapsedMs,6} ms");
    }

    /// <summary>
    /// Returns the full log as a formatted string.
    /// When both results are present, appends a speedup comparison.
    /// </summary>
    public string GetReport()
    {
        var sb = new StringBuilder();

        foreach (var entry in _entries)
            sb.AppendLine(entry);

        if (HasBothResults)
        {
            sb.AppendLine(new string('─', 52));
            sb.AppendLine($"  Single-thread :  {_singleMs,6} ms");
            sb.AppendLine($"  Multi-thread  :  {_multiMs,6} ms  ({_threads} threads)");
            double speedup = (double)_singleMs / _multiMs;
            sb.AppendLine($"  Speedup       :  {speedup,6:F2}x");
        }

        return sb.ToString();
    }

    ///Resets all stored results (called before each new attack).
    public void Clear()
    {
        _entries.Clear();
        _singleMs = -1;
        _multiMs = -1;
    }
}
