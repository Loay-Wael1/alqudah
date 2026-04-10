using System.Text.RegularExpressions;

namespace JudgesTournament.Application.Helpers;

/// <summary>
/// Normalizes Egyptian phone numbers for consistent storage and duplicate detection.
/// </summary>
public static partial class PhoneNormalizer
{
    /// <summary>
    /// Normalizes a phone number: strips non-digits, converts +20/0020 to 0, ensures 01XXXXXXXXX format.
    /// Returns digits-only normalized form for storage.
    /// </summary>
    public static string Normalize(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return string.Empty;

        // Remove everything except digits
        var digits = NonDigits().Replace(phone.Trim(), "");

        if (digits.Length == 0)
            return string.Empty;

        // +20 prefix → strip to local form
        if (digits.StartsWith("20") && digits.Length == 12)
            digits = "0" + digits[2..];

        // 0020 prefix → strip to local form
        if (digits.StartsWith("0020") && digits.Length == 14)
            digits = "0" + digits[4..];

        // Ensure starts with 0 for 10-digit numbers
        if (digits.Length == 10 && digits.StartsWith("1"))
            digits = "0" + digits;

        return digits;
    }

    [GeneratedRegex(@"\D")]
    private static partial Regex NonDigits();
}
