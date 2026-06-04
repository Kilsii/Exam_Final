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


