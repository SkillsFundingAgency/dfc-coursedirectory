using Dfc.CourseDirectory.WebV2.Filters;
using Dfc.CourseDirectory.WebV2.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.WebUtilities;

namespace Dfc.CourseDirectory.WebV2.TagHelpers
{
    [HtmlTargetElement("form", Attributes = "append-current-provider")]
    public class CurrentProviderFormTagHelper : TagHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICurrentUserProvider _currentUserProvider;

        public CurrentProviderFormTagHelper(
            IHttpContextAccessor httpContextAccessor,
            ICurrentUserProvider currentUserProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _currentUserProvider = currentUserProvider;
        }

        [HtmlAttributeName("append-current-provider")]
        public bool AppendCurrentProvider { get; set; }

        public override int Order => 100;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (!AppendCurrentProvider ||
                !output.Attributes.ContainsName("action") ||
                _currentUserProvider.GetCurrentUser().IsProvider)
            {
                return;
            }

            var currentProviderFeature = _httpContextAccessor.HttpContext.Features.Get<CurrentProviderFeature>();
            if (currentProviderFeature == null)
            {
                return;
            }

            var currentProviderId = currentProviderFeature.ProviderInfo.ProviderId;

            var resolvedAction = output.Attributes["action"].Value.ToString();

            var updatedAction = QueryHelpers.AddQueryString(
                resolvedAction,
                CurrentProviderResourceFilter.RouteValueKey,
                currentProviderId.ToString());

            output.Attributes.SetAttribute("action", updatedAction);
        }
    }
}
