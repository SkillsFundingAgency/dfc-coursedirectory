using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2
{
    public static class ViewResultExtensions
    {
        public static ViewResult WithViewData(this ViewResult viewResult, string key, object value)
        {
            viewResult.ViewData.Add(key, value);
            return viewResult;
        }
    }
}
