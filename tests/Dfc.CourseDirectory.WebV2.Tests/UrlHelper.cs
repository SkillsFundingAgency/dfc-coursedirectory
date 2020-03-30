namespace Dfc.CourseDirectory.WebV2.Tests
{
    public static class UrlHelper
    {
        public static string StripQueryParams(string url)
        {
            var sep = url.IndexOf("?");

            return sep >= 0 ? url.Substring(0, sep) : url;
        }
    }
}
