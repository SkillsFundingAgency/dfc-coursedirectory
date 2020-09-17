using System;
using Dfc.CourseDirectory.WebV2.HttpContextFeatures;
using Flurl;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Dfc.CourseDirectory.WebV2.ViewHelpers
{
    public class ProviderContextHelper : IViewContextAware
    {
        private ViewContext _viewContext;

        public ProviderInfo ProviderInfo => ProviderContext?.ProviderInfo;

        private ProviderContext ProviderContext =>
            _viewContext.HttpContext.Features.Get<ProviderContextFeature>()?.ProviderContext;

        public static implicit operator ProviderContext(ProviderContextHelper helper) =>
            helper.ProviderContext;

        public string AppendToUrl(string url)
        {
            if (url == null)
            {
                throw new ArgumentNullException(nameof(url));
            }

            return new Url(url).WithProviderContext(this);
        }

        void IViewContextAware.Contextualize(ViewContext viewContext)
        {
            _viewContext = viewContext;
        }
    }
}
