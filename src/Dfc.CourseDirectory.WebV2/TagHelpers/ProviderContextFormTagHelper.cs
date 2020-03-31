using Dfc.CourseDirectory.WebV2.Filters;
using Dfc.CourseDirectory.WebV2.HttpContextFeatures;
using Dfc.CourseDirectory.WebV2.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.WebUtilities;

namespace Dfc.CourseDirectory.WebV2.TagHelpers
{
    [HtmlTargetElement("form", Attributes = "append-provider-context")]
    public class ProviderContextFormTagHelper : TagHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICurrentUserProvider _currentUserProvider;

        public ProviderContextFormTagHelper(
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
                !output.Attributes.ContainsName("action") ||
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

            if (output.Attributes["method"]?.Value.ToString().ToLower() == "post")
            {
                var existingAction = output.Attributes["action"].Value.ToString();

                var newAction = QueryHelpers.AddQueryString(
                    existingAction,
                    ProviderContextResourceFilter.RouteValueKey,
                    currentProviderId.ToString());

                output.Attributes.SetAttribute("action", newAction);
            }
            else
            {
                var hiddenField = new TagBuilder("input");
                hiddenField.Attributes.Add("type", "hidden");
                hiddenField.Attributes.Add("name", ProviderContextResourceFilter.RouteValueKey);
                hiddenField.Attributes.Add("value", currentProviderId.ToString());

                output.PreContent.AppendHtml(hiddenField);
            }
        }
    }
}
