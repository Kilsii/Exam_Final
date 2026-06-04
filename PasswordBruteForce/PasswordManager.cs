
using System.Security.Cryptography;
using System.Text;

namespace PasswordBruteForce;

/// <summary>
/// Generates random passwords and computes SHA-256 hashes with a constant static salt.
/// </summary>
public class PasswordManager
{
    public static readonly string SALT = "Kilsi_is_A_SaLt_HaHaha2026$$";
    public static readonly string CHARSET = "abcdefghijklmnopqrstuvwxyz0123456789";

    private readonly Random _rng = new();

    /// Generates a random password whose length is in [4, 6).
    public string GeneratePassword()
    {
        int length = _rng.Next(4, 6);
        char[] chars = new char[length];
        for (int i = 0; i < length; i++)
            chars[i] = CHARSET[_rng.Next(CHARSET.Length)];
        return new string(chars);
    }

    /// <summary>
    /// Returns SHA-256( SALT + password ) encoded as a Base64 string.
    /// </summary>
    public static string HashPassword(string password)
    {
        // Prepend salt before hashing
        byte[] data = Encoding.UTF8.GetBytes(SALT + password);
        byte[] hash = SHA256.HashData(data);          
        return Convert.ToBase64String(hash);
    }
}
