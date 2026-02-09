using System.Text.RegularExpressions;

namespace cnu_cinema_practice.Web.Helpers;

public static class VideoHelper
{
    public static bool IsYouTube(string? url)
    {
        if (string.IsNullOrEmpty(url)) return false;
        return url.Contains("youtube.com") || url.Contains("youtu.be");
    }

    public static string GetYouTubeEmbedUrl(string url, bool isBackground = true)
    {
        if (string.IsNullOrEmpty(url)) return string.Empty;

        string videoId = string.Empty;

        // Match standard watch URL
        var match = Regex.Match(url, @"(?:v=|\/v\/|embed\/|youtu\.be\/|\/v=)([^&?#\/\s]+)");
        if (match.Success)
        {
            videoId = match.Groups[1].Value;
        }

        if (string.IsNullOrEmpty(videoId)) return url;

        if (isBackground)
        {
            // Background parameters: muted, loop, no controls
            return $"https://www.youtube.com/embed/{videoId}?autoplay=1&mute=1&controls=0&loop=1&playlist={videoId}&playsinline=1&rel=0&modestbranding=1&iv_load_policy=3&enablejsapi=1&vq=hd1080";
        }

        // Modal/Active parameters: unmuted, with controls
        return $"https://www.youtube.com/embed/{videoId}?autoplay=1&controls=1&rel=0&modestbranding=1&playsinline=1&vq=hd1080";
    }
}
