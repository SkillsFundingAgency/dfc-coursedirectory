using System;

namespace Dfc.CourseDirectory.FindACourseApi.Features
{
    public static class ViewModelFormatting
    {
        public static string EnsureHttpPrefixed(string url) => !string.IsNullOrEmpty(url)
            ? url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                ? url
                : $"http://{url}"
            : null;
    }
}
