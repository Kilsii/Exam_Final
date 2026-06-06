# Password Brute-Force Cracker

**Vilnius University — Software Engineering, 2026**  
Multi-threaded password brute-force application with SHA-256 hashing and performance comparison.

---
VERSION HISTORY

v0.1 |ADDED README and PasswordManager.cs
- Handles password generation and SHA-256 hashing
- Generates random passwords with length [4,6) using Random.Next(4,6)
- Contains static SALT constant prepended before hashing
- HashPassword() computes SHA256(SALT + password) and returns Base64 string

v0.2 | ADDED BruteForceGenerator
-Produce candidate strings for the brute-force search.

Key design — INDEX-BASED generation:
 -Every string of a given length corresponds to a unique integer index in [0, N^L).
  This is identical to base-N number representation, where N = charset size and L = length.
  Example (charset="abc", L=2):index 0 → "aa",  index 1 → "ab",  index 2 → "ac".
 -This lets the engine divide `[0, N^L]` into non-overlapping numeric ranges and hand
 one range to each thread enabling genuine parallel search with zero shared state between threads .

 v0.3 | ADDED HashValidator
-Validate candidate strings against a stored SHA-256 hash.
-Independent from BruteForceGenerator:has no knowledge of how candidates are produced.
-Receives a plain-text candidate, hashes it with the static salt, compares to target hash.
-SHA256.HashData is thread-safe : multiple threads can call Validate() simultaneously.

v0.4 | Added PerformanceLogger
- Records timing results for single-thread and multi-thread runs.
- LogSingleThread() and LogMultiThread() store elapsed milliseconds.
- GetReport() generates a formatted comparison report with speedup ratio.
- Clear() resets all stored results before each new attack.

v0.5 | Added BruteForceEngine
- Orchestrates the brute-force attack in both single-thread and multi-thread modes.
- Loops through lengths 1 to 6.
- RunSequential() checks every candidate on one thread.
- RunParallel() divides index space into equal chunks, one Thread per chunk.
- All threads start simultaneously before any Join() — genuine parallel execution.
- Interlocked.CompareExchange ensures OnPasswordFound fires exactly once.
- CancellationToken stops all threads immediately when password is found.
- MaxThreadCount = CPU cores - 1, minimum 1.

v0.6 | Added MainForm
- Full GUI layout with dark theme
- Panels for Generator, Cracker, Result, Performance
- Rounded progress bar, custom colours
- Start/Stop controls, progress display, elapsed time

v0.7 | UI Polish
- Fixed found password text color to white
- Fixed Hurray label color to green
- Reduced progress bar width for more visible progress
- Added OptimizedDoubleBuffer to RoundedProgressBar to fix flickering
- Fixed progress bar Maximum reset issue
