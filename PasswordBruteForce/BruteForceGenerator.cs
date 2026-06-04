
namespace PasswordBruteForce;

/// Converts between integer indices and candidate password strings.
public class BruteForceGenerator
{
    //Properties
    public string Charset { get; }
    public int MaxLength { get; }

    //Constructor
    public BruteForceGenerator(string charset, int maxLength)
    {
        Charset = charset;
        MaxLength = maxLength;
    }

    // Public methods 

    /// <summary>
    /// Converts a linear index to the string candidate of the given length.
    /// Works like converting a decimal number to a base-N numeral:
    ///   : rightmost character = index % N
    ///   : next character      = (index / N) % N
    /// </summary>
    /// <param name="length">Length of the candidate string.</param>
    /// <param name="index">Position within [0, charset^length).</param>
    public string GetByIndex(int length, long index)
    {
        char[] result = new char[length];
        int charsetN = Charset.Length;
      
        for (int i = length - 1; i >= 0; i--)       // Fills from the rightmost  to the leftmost position
        {
            result[i] = Charset[(int)(index % charsetN)];
            index /= charsetN;
        }
        return new string(result);
    }

    /// Total number of distinct candidates for one length = charset.Length ^ length.
    public long GetTotalCombinations(int length)
    {
        long total = 1;
        for (int i = 0; i < length; i++)
            total *= Charset.Length;
        return total;
    }

    /// <summary>
    /// Sum of all candidates across lengths 1 … MaxLength.
    /// Used by the UI to compute overall progress percentage.
    /// </summary>
    public long GetGrandTotal()
    {
        long total = 0;
        for (int l = 1; l <= MaxLength; l++)
            total += GetTotalCombinations(l);
        return total;
    }
}
