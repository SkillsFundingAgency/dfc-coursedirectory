using Microsoft.AspNetCore.Mvc.Rendering;

namespace Dfc.CourseDirectory.WebV2.SharedViews
{
    public static class ViewContextExtensions
    {
        public static void SetLayoutData(
            this ViewContext viewContext,
            string title = null,
            AdminTopNavSection? activeAdminTopNavSection = null)
        {
            if (title != null)
            {
                viewContext.ViewData["Title"] = title;
            }

            if (activeAdminTopNavSection != null)
            {
                viewContext.ViewData["ActiveAdminTopNavSection"] = activeAdminTopNavSection;
            }
        }
    }
}
