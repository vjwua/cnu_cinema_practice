using System.Reflection;
using PdfSharpCore.Fonts;

namespace Infrastructure.Services;

public class SystemFontResolver : IFontResolver
{
    public string DefaultFontName => "Arial";

    public byte[]? GetFont(string faceName)
    {
        var fontPaths = new[]
        {
            "/System/Library/Fonts/Geneva.ttf",
            "/System/Library/Fonts/Courier.ttc",
            "/Library/Fonts/Arial Unicode.ttf",
            "/System/Library/Fonts/Supplemental/Arial.ttf",
            "/Library/Fonts/Arial.ttf",
            "/System/Library/Fonts/Helvetica.ttc",
            "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf", // Linux fallback
            @"C:\Windows\Fonts\arial.ttf" // Windows fallback
        };

        var found = fontPaths.FirstOrDefault(File.Exists);
        if (found != null) return File.ReadAllBytes(found);
        
        // Debugging info
        var debugInfo = $"OS: {Environment.OSVersion}, Checked paths: {string.Join(", ", fontPaths)}";
        throw new InvalidOperationException($"CustomFontResolver: No fonts found. {debugInfo}");
    }

    public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
        return new FontResolverInfo("Arial");
    }
}
