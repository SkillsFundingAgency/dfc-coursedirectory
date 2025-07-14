using System;

namespace Dfc.CourseDirectory.WebV2
{
    public static class UrlUtil
    {
        public static string EnsureHttpPrefixed(string website)
        {
            if (string.IsNullOrEmpty(website))
            {
                throw new ArgumentException("Value cannot be empty.", nameof(website));
            }

            if (website.StartsWith("http://") || website.StartsWith("https://"))
            {
                return website;
            }
            else
            {
                return "http://" + website;
            }
        }
    }
}
