using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.MultiPageTransaction
{
    public static class RedirectToActionResultExtensions
    {
        public static RedirectToActionResult WithMptxInstanceId<T>(
            this RedirectToActionResult result,
            MptxInstanceContext<T> instanceContext)
            where T : IMptxState
        {
            return WithMptxInstanceId(result, instanceContext.InstanceId);
        }

        public static RedirectToActionResult WithMptxInstanceId(
            this RedirectToActionResult result,
            string instanceId)
        {
            result.RouteValues ??= new Microsoft.AspNetCore.Routing.RouteValueDictionary();
            result.RouteValues.Add(Constants.InstanceIdQueryParameter, instanceId);
            return result;
        }
    }
}
