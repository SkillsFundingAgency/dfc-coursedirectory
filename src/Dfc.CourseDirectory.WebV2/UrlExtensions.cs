using Dfc.CourseDirectory.Core.Middleware;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction;
using Flurl;

namespace Dfc.CourseDirectory.WebV2
{
    public static class UrlExtensions
    {
        public static Url WithMptxInstanceId<T>(this Url url, MptxInstanceContext<T> context)
            where T : IMptxState =>
            WithMptxInstanceId(url, context.InstanceId);

        public static Url WithMptxInstanceId(this Url url, string instanceId) =>
            url.SetQueryParam(Constants.InstanceIdQueryParameter, instanceId);
    }
}
