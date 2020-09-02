using Microsoft.AspNetCore.Mvc.Rendering;

namespace Dfc.CourseDirectory.WebV2.SharedViews
{
    public static class ViewContextExtensions
    {
        public static void SetLayoutData(
            this ViewContext viewContext,
            string title,
            bool showBackLink = false)
        {
            viewContext.ViewData["Title"] = title;
            viewContext.ViewData["ShowBackLink"] = showBackLink;
        }
    }
}
