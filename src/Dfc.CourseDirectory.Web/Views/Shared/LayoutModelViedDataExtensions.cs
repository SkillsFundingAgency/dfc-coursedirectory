using System.Collections.Generic;

namespace Dfc.CourseDirectory.Web.Views.Shared
{
    public static class LayoutModelViedDataExtensions
    {
        public const string ViewDataKey = "LayoutModelKey";

        public static void SetLayoutModel(this IDictionary<string, object> viewData, LayoutModel layoutModel)
        {
            viewData[ViewDataKey] = layoutModel;
        }

        public static LayoutModel GetLayoutModel(this IDictionary<string, object> viewData)
        {
            if (!viewData.ContainsKey(ViewDataKey))
            {
                return new LayoutModel();
            }
            return (LayoutModel)viewData[ViewDataKey];
        }
    }
}
