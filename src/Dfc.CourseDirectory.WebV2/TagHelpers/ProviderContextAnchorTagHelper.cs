using Dfc.CourseDirectory.WebV2.Filters;
using Dfc.CourseDirectory.WebV2.HttpContextFeatures;
using Dfc.CourseDirectory.WebV2.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.WebUtilities;

namespace Dfc.CourseDirectory.WebV2.TagHelpers
{
    [HtmlTargetElement("a", Attributes = "append-provider-context")]
    public class ProviderContextAnchorTagHelper : TagHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICurrentUserProvider _currentUserProvider;

        public ProviderContextAnchorTagHelper(
            IHttpContextAccessor httpContextAccessor,
            ICurrentUserProvider currentUserProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _currentUserProvider = currentUserProvider;
        }

        [HtmlAttributeName("append-provider-context")]
        public bool AppendProviderContext { get; set; }

        public override int Order => 100;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (!AppendProviderContext ||
                _currentUserProvider.GetCurrentUser().IsProvider)
            {
                return;
            }

            var providerContextFeature = _httpContextAccessor.HttpContext.Features.Get<ProviderContextFeature>();
            if (providerContextFeature == null)
            {
                return;
            }

            var currentProviderId = providerContextFeature.ProviderInfo.ProviderId;

            var existingHref = output.Attributes["href"].Value.ToString();

            var newHref = QueryHelpers.AddQueryString(
                existingHref,
                ProviderContextResourceFilter.RouteValueKey,
                currentProviderId.ToString());

            output.Attributes.SetAttribute("href", newHref);
        }
    }
}
