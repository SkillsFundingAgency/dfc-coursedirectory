namespace Dfc.CourseDirectory.Web.ViewComponents.Extensions
{
    public static class ViewComponentExtensions
    {
        public static string ThenCheck(this bool extendee)
        {
            return extendee ? "checked=\"checked\"" : string.Empty;
        }

        public static string ThenAriaCurrent(this bool extendee)
        {
            return extendee ? "aria-current=\"true\"" : string.Empty;
        }
    }
}