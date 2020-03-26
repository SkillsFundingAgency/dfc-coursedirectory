using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.WebUtilities;

namespace Dfc.CourseDirectory.WebV2.MultiPageTransaction
{
    [HtmlTargetElement("a", Attributes = "mptx-href")]
    public class AnchorTagHelper : TagHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AnchorTagHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        [HtmlAttributeName("mptx-href")]
        public bool IsMptxAction { get; set; }

        public override int Order => 1000;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (!IsMptxAction)
            {
                return;
            }

            var httpContext = _httpContextAccessor.HttpContext;
            var instanceFeature = httpContext.Features.Get<MptxInstanceFeature>();

            if (instanceFeature == null)
            {
                throw new InvalidOperationException("No active MPTX instance found.");
            }

            var existingHref = output.Attributes["href"].Value.ToString();

            var newHref = QueryHelpers.AddQueryString(
                existingHref,
                Constants.InstanceIdQueryParameter,
                instanceFeature.Instance.InstanceId);

            output.Attributes.SetAttribute("href", newHref);
        }
    }
}
