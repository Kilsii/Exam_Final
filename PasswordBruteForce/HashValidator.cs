
using System.Security.Cryptography;
using System.Text;
namespace PasswordBruteForce;

/// <summary>
/// Checks whether a candidate password, when hashed with the static salt,
/// matches the target SHA-256 hash.
/// </summary>

public class HashValidator
{
    private readonly string _targetHash;   // the hash we are trying to crack
    private readonly string _salt;         // must equal PasswordManager.SALT

    //Constructor    
    public HashValidator(string targetHash, string salt)
    {
        _targetHash = targetHash;
        _salt = salt;
    }

    // Public methods 

    /// <summary>
    /// Returns <c>true</c> when SHA-256(salt + candidate) equals the stored target hash.
    /// The exact inverse of <see cref="PasswordManager.HashPassword"/>.
    /// </summary>
    public bool Validate(string candidate)
    {
        byte[] data = Encoding.UTF8.GetBytes(_salt + candidate);
        byte[] hash = SHA256.HashData(data);
        return Convert.ToBase64String(hash) == _targetHash;
    }
}
