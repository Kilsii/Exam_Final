
namespace PasswordBruteForce;

public enum AttackMode { SingleThread, MultiThread }

/// <summary>
/// Coordinates the brute-force attack using either a single thread or
/// (CPU cores − 1) parallel threads.
/// </summary>
public class BruteForceEngine
{
    //  Dependencies 

    private readonly BruteForceGenerator _generator;
    private readonly HashValidator _validator;

    // Thread-safe state

    private CancellationTokenSource? _cts;

    private long _checked;       // total candidates tested so far  (Interlocked)
    private int _currentLength; // length currently being searched (written by engine thread only)
    private int _foundFlag;     // 0 = not found yet / 1 = found   (Interlocked CAS)

    //  Public surface 

    /// <summary>CPU cores − 1, minimum 1.</summary>
    public int MaxThreadCount { get; } = Math.Max(1, Environment.ProcessorCount - 1);

    /// Thread-safe read of the running candidate counter.
    public long CheckedCount => Interlocked.Read(ref _checked);

    /// Total candidates across all lengths 1…6 (set at the start of each run).
    public long GrandTotal { get; private set; }

    /// password length currently being searched.
    public int CurrentLength => _currentLength;

    /// <summary>
    /// Fired exactly once, from whichever background thread finds the password.
    /// The MainForm subscribes and marshals to the UI thread via Invoke.
    /// </summary>
    public event Action<string>? OnPasswordFound;

    /// <summary>Fired each time the search advances to a new length.</summary>
    public event Action<string>? OnStatusChanged;

    // ── Constructor ──────────────────────────────────────────────────────────

    public BruteForceEngine(BruteForceGenerator generator, HashValidator validator)
    {
        _generator = generator;
        _validator = validator;
    }

    // PUBLIC METHODS
   
    /// <summary>
    /// Starts the brute-force attack.
    /// BLOCKS until the password is found or <see cref="Stop"/> is called.
    /// </summary>
    public void Start(AttackMode mode)
    {
        _cts = new CancellationTokenSource();
        _foundFlag = 0;
        GrandTotal = _generator.GetGrandTotal();
        Interlocked.Exchange(ref _checked, 0);

        CancellationToken token = _cts.Token;

        // Brute force starts at length 1
        // It will find the password when it reaches the correct length and combination.
        for (int len = 1; len <= _generator.MaxLength && !token.IsCancellationRequested; len++)
        {
            _currentLength = len;
            long totalForLength = _generator.GetTotalCombinations(len);

            OnStatusChanged?.Invoke(
                $"Searching length {len} / {_generator.MaxLength}  —  {totalForLength:N0} candidates …");

            if (mode == AttackMode.MultiThread)
                RunParallel(len, totalForLength, token);
            else
                RunSequential(len, totalForLength, token);
        }
    }

    /// <summary>
    /// Signals all running threads to stop after their current candidate check.
    /// Called by the Stop button on the UI thread.
    /// </summary>
    public void Stop() => _cts?.Cancel();

    // PRIVATE METHODS
   

    /// <summary>
    /// Checks every index in [0, total) sequentially on the calling thread.
    /// </summary>
    private void RunSequential(int length, long total, CancellationToken token)
    {
        for (long i = 0; i < total && !token.IsCancellationRequested; i++)
            Check(_generator.GetByIndex(length, i));
    }

    /// <summary>
    /// Divides [0, total) into (MaxThreadCount) non-overlapping ranges.
    /// Creates one Thread per range, starts all threads simultaneously,
    /// then waits for all to finish before the engine moves to the next length.
    /// </summary>
    private void RunParallel(int length, long total, CancellationToken token)
    {
        // Cap thread count to the number of available combinations
        int count = (int)Math.Min(MaxThreadCount, total);
        // Ceiling division so every index is covered
        long chunk = (total + count - 1) / count;

        var threads = new Thread[count];

        for (int t = 0; t < count; t++)
        {
            long startIdx = t * chunk;
            long endIdx = Math.Min(startIdx + chunk, total);
            int id = t;    // captured per-thread to avoid closure issues

            threads[t] = new Thread(() =>
            {
                
                for (long i = startIdx; i < endIdx && !token.IsCancellationRequested; i++)  // Each thread iterates only its own slice  
                    Check(_generator.GetByIndex(length, i));
            })
            {
                Name = $"BF-Thread-{id}",
                IsBackground = true    // won't block the process from exiting
            };
        }

        //Start all threads before joining any: what makes it parallel
        foreach (var th in threads) th.Start();

        // Wait for all threads to finish before moving to the next length
        foreach (var th in threads) th.Join();
    }

    /// <summary>
    /// Increments the counter, hashes the candidate, and fires the found event
    /// using an Interlocked CAS to guarantee the event fires at most once even
    /// if two threads validate simultaneously.
    /// </summary>
    private void Check(string candidate)
    {
        Interlocked.Increment(ref _checked);

        if (_validator.Validate(candidate))
        {
            // CompareExchange(ref location, value, comparand):
            //   if (location == comparand) { location = value; return comparand; }
            //   else return location;
            // Only the FIRST thread to succeed (return value == 0) fires the event.
            if (Interlocked.CompareExchange(ref _foundFlag, 1, 0) == 0)
            {
                OnPasswordFound?.Invoke(candidate);
                _cts?.Cancel();   //stop all remaining threads immediately
            }
        }
    }
}
