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