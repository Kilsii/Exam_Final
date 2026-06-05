
using System.Diagnostics;

namespace PasswordBruteForce;

public partial class MainForm : Form
{
    // Dependencies
    private readonly PasswordManager _pwdMgr = new();
    private readonly PerformanceLogger _logger = new();

    // Per-attack state (reset before each run)
    private BruteForceEngine? _engine;           // current engine instance
    private string _targetHash = "";  // hash of the active password
    private CancellationTokenSource? _runCts;           // outer cancel (for compare mode flow)

    // UI timing
    private readonly System.Windows.Forms.Timer _uiTimer = new() { Interval = 100 };
    private readonly Stopwatch _stopwatch = new();

    
    // CONSTRUCTOR
    
    public MainForm()
    {
        InitializeComponent();

        _uiTimer.Tick += OnTimerTick;

        lblThreadInfo.Text =
            $"CPU logical cores: {Environment.ProcessorCount}   |   " +
            $"Max brute-force threads (cores − 1): {Math.Max(1, Environment.ProcessorCount - 1)}";

        // Generate a password immediately so the app is demo-ready on launch
        GeneratePassword();
    }

   
    // PASSWORD GENERATION
   
    private void btnGenerate_Click(object sender, EventArgs e) => GeneratePassword();

    private void GeneratePassword()
    {
        string pwd = _pwdMgr.GeneratePassword();
        _targetHash = PasswordManager.HashPassword(pwd);

        txtPassword.Text = pwd;
        txtHash.Text = _targetHash;
        txtFound.Text = "";
        pbProgress.Value = 0;
        lblChecked.Text = "Checked: 0 / 0   |   Length: — / 6";
        lblElapsed.Text = "Elapsed: 0.00 s";
        lblStatus.Text = $"Generated a {pwd.Length}-character password. Ready to attack.";
        lblHurray.Visible = false;
    }

    // START / STOP
    
    private async void btnStart_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_targetHash)) { GeneratePassword(); return; }

        SetRunningState(true);
        txtFound.Text = "";
        _logger.Clear();
        rtbLog.Clear();
        _runCts = new CancellationTokenSource();

        try
        {
            if (radCompare.Checked)
            {
                await RunComparison(_runCts.Token);
            }
            else
            {
                // Single run in whichever mode the user chose
                var mode = radSingle.Checked ? AttackMode.SingleThread : AttackMode.MultiThread;
                var (pwd, ms) = await RunAttack(mode, _runCts.Token);

                if (pwd is not null)
                {
                    if (mode == AttackMode.SingleThread)
                        _logger.LogSingleThread(pwd, ms);
                    else
                        _logger.LogMultiThread(pwd, ms, Math.Max(1, Environment.ProcessorCount - 1));

                    rtbLog.Text = _logger.GetReport();
                }
            }
        }
        finally
        {
            // Always re-enable the UI even if an exception slips through
            SetRunningState(false);
        }
    }

    private void btnStop_Click(object sender, EventArgs e)
    {
        _engine?.Stop();      // cancels the engine's internal CTS → worker threads exit loops
        _runCts?.Cancel();    // cancels the outer CTS → prevents starting Phase 2 in compare mode
        _stopwatch.Stop();
        _uiTimer.Stop();
        lblStatus.Text = "⛔  Stopped by user.";
    }

    // CORE ATTACK LOGIC
   

    /// <summary>
    /// Creates a fresh engine, subscribes to its events, runs it on a background thread
    /// via Task.Run, and returns (foundPassword, elapsedMs).
    ///
    /// Task.Run?:
    ///   engine.Start() is a blocking call — it returns only when the password is found or
    ///   the CancellationToken fires.  Running it with Task.Run offloads it to the thread pool,
    ///   so the UI thread (and its message pump) stays completely responsive.
    ///async/await?:
    ///   await suspends this method and yields control back to the UI thread while the Task
    ///   runs in the background.  When the Task completes, execution resumes here on the UI thread.
    /// </summary>
    private async Task<(string? password, long elapsedMs)> RunAttack(
        AttackMode mode, CancellationToken runToken)
    {
        var generator = new BruteForceGenerator(PasswordManager.CHARSET, maxLength: 6);
        var validator = new HashValidator(_targetHash, PasswordManager.SALT);
        _engine = new BruteForceEngine(generator, validator);

        string? foundPwd = null;
        long foundMs = 0;

        
        _engine.OnPasswordFound += pwd =>
        {
            // This fires from a background BF-Thread
            
            foundMs = _stopwatch.ElapsedMilliseconds;
            foundPwd = pwd;
            _stopwatch.Stop();

            if (!IsHandleCreated || IsDisposed) return;
            this.Invoke(() =>
            {
                lblHurray.Visible = true;
                txtFound.Text = pwd;
                lblStatus.Text = $"✅  Found '{pwd}'  in {foundMs} ms!";
                _uiTimer.Stop();
                UpdateProgressUI();   // one final UI refresh with the exact final count
                
            });
        };

        _engine.OnStatusChanged += msg =>
        {
            // Also fires from a background thread 
            if (!IsHandleCreated || IsDisposed) return;
            this.Invoke(() => lblStatus.Text = msg);
        };

        //Start the attack 
        _stopwatch.Restart();
        _uiTimer.Start();

        try
        {
            // runToken lets Task.Run throw immediately if cancelled before starting;
            // the engine's internal token stops threads that are already running.
            await Task.Run(() => _engine.Start(mode), runToken);
        }
        catch (OperationCanceledException) { /* user pressed Stop — normal flow */ }
        finally
        {
            _uiTimer.Stop();
            _stopwatch.Stop();
        }

        return (foundPwd, foundMs);
    }

    /// <summary>
    /// Compare mode: runs single-thread to completion, records its time,
    /// then runs multi-thread against the same hash and records its time,
    /// then shows the speedup in the log panel.
    /// </summary>
    private async Task RunComparison(CancellationToken runToken)
    {
        //Phase 1: Single Thread
        lblStatus.Text = "🔵  Phase 1 — single-thread run …";
        var (pwd1, ms1) = await RunAttack(AttackMode.SingleThread, runToken);
        if (pwd1 is not null) _logger.LogSingleThread(pwd1, ms1);
        rtbLog.Text = _logger.GetReport();

        if (runToken.IsCancellationRequested) return;

        // Brief pause so the user can see Phase 1 result before Phase 2 starts
        try { await Task.Delay(700, runToken); }
        catch (OperationCanceledException) { return; }

        // Phase 2: Multi Thread 
        lblStatus.Text = "🟢  Phase 2 — multi-thread run …";
        var (pwd2, ms2) = await RunAttack(AttackMode.MultiThread, runToken);
        if (pwd2 is not null)
            _logger.LogMultiThread(pwd2, ms2, Math.Max(1, Environment.ProcessorCount - 1));

        rtbLog.Text = _logger.GetReport();   // now shows the full comparison with speedup
    }

    
    // PROGRESS TIMER
    

    /// <summary>
    /// Fires every 100 ms on the UI thread to refresh progress labels and bar.
    /// Reads Interlocked counters from the engine — thread-safe.
    /// </summary>
    private void OnTimerTick(object? sender, EventArgs e) => UpdateProgressUI();

    private void UpdateProgressUI()
    {
        if (_engine is null) return;

        long ch = _engine.CheckedCount;
        long tot = _engine.GrandTotal;
        int len = _engine.CurrentLength;

        lblChecked.Text = $"Checked: {ch:N0} / {tot:N0}   |   Length: {len} / 6";
        lblElapsed.Text = $"Elapsed: {_stopwatch.Elapsed.TotalSeconds:F2} s";

        if (tot > 0)
        {
            // Scale to [0, 10 000] for finer ProgressBar resolution
            int val = (int)Math.Min(10000, (double)ch / tot * 10000);
            pbProgress.Value = val;
        }
    }

    
    // UI STATE HELPERS

    /// <summary>
    /// Toggles controls between "running" and "idle" states so the user

    /// </summary>
    private void SetRunningState(bool running)
    {
        btnStart.Enabled = !running;
        btnStop.Enabled = running;
        btnGenerate.Enabled = !running;
        radSingle.Enabled = !running;
        radMulti.Enabled = !running;
        radCompare.Enabled = !running;
        if (!running) { _uiTimer.Stop(); UpdateProgressUI(); }
    }

    private void btnClearLog_Click(object sender, EventArgs e)
    {
        _logger.Clear();
        rtbLog.Clear();
    }
}
