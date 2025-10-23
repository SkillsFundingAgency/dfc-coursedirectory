using System;
using System.Linq;

namespace Dfc.CourseDirectory.FindACourseApi.Features
{
    public static class ViewModelFormatting
    {
        public static string EnsureHttpPrefixed(string url) => !string.IsNullOrEmpty(url)
            ? url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                ? url
                : $"http://{url}"
            : null;

        public static string ConcatAddressLines(string saon, string paon, string street)
            => string.Join(" ", new[] { saon, paon, street }.Where(v => !string.IsNullOrWhiteSpace(v)).Select(v => v.Trim()));
    }
}
