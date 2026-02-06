using System.Text.RegularExpressions;

namespace cnu_cinema_practice.Web.Helpers;

public static class SlugHelper
{
    public static string GenerateSlug(string title)
    {
        if (string.IsNullOrEmpty(title))
            return string.Empty;

        // Convert to lower case
        string str = title.ToLowerInvariant();

        // Remove invalid chars (keep letters, numbers, spaces, and hyphens)
        // This regex allows various language characters (Unicode categories usually covered by \w but restricted here to safe URL chars)
        // Simpler approach: replace anything that is NOT a letter or digit or space with empty string
        // But for Ukrainian titles, we need to ensure Cyrillic is handled or we just keep it as is (modern browsers handle UTF-8 URLs fine).
        // However, standard slug best practice usually involves transliteration if ASCII-only is required. 
        // Given the requirement "Remove special chars and diacritics", we should probably transliterate or just strip.
        // For now, let's just strip special chars and spaces -> hyphens.

        str = Regex.Replace(str, @"[^a-z0-9\s-а-яіїєґ]", ""); // Allow lowercase alphanumeric, spaces, hyphens, and Cyrillic chars

        // Replace multiple spaces with one space
        str = Regex.Replace(str, @"\s+", " ").Trim();

        // Cut and trim
        str = str.Substring(0, str.Length <= 80 ? str.Length : 80).Trim();

        // Replace spaces with hyphens
        str = Regex.Replace(str, @"\s", "-");

        return str;
    }
}
