using System.Text.RegularExpressions;

namespace JudgesTournament.Application.Helpers;

public static partial class TeamNameNormalizer
{
    /// <summary>
    /// Normalizes a team name for consistent comparison and duplicate detection.
    /// Steps: Trim → collapse spaces → normalize Arabic variants → ToUpperInvariant
    /// </summary>
    public static string Normalize(string teamName)
    {
        if (string.IsNullOrWhiteSpace(teamName))
            return string.Empty;

        var result = teamName.Trim();

        // Collapse multiple spaces
        result = MultipleSpaces().Replace(result, " ");

        // Normalize Arabic letter variants
        result = NormalizeArabic(result);

        return result.ToUpperInvariant();
    }

    private static string NormalizeArabic(string text)
    {
        // أ إ آ → ا
        text = text.Replace('أ', 'ا')
                   .Replace('إ', 'ا')
                   .Replace('آ', 'ا');

        // ة → ه
        text = text.Replace('ة', 'ه');

        // ى → ي
        text = text.Replace('ى', 'ي');

        return text;
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex MultipleSpaces();
}
