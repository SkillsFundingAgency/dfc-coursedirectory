using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.WebUtilities;

namespace Dfc.CourseDirectory.WebV2.MultiPageTransaction
{
    [HtmlTargetElement("form", Attributes = "mptx-action")]
    public class FormTagHelper : TagHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FormTagHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        [HtmlAttributeName("mptx-action")]
        public bool IsMptxAction { get; set; }

        public override int Order => 1000;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (!IsMptxAction)
            {
                return;
            }

            var httpContext = _httpContextAccessor.HttpContext;
            var instanceFeature = httpContext.Features.Get<MptxInstanceContextFeature>();

            if (instanceFeature == null)
            {
                throw new InvalidOperationException("No active MPTX instance found.");
            }

            var instanceId = instanceFeature.InstanceContext.InstanceId;

            if (output.Attributes["method"]?.Value.ToString().ToLower() == "post")
            {
                var existingAction = output.Attributes["action"].Value.ToString();

                var newAction = QueryHelpers.AddQueryString(
                    existingAction,
                    Constants.InstanceIdQueryParameter,
                    instanceId);

                output.Attributes.SetAttribute("action", newAction);
            }
            else
            {
                var hiddenField = new TagBuilder("input");
                hiddenField.Attributes.Add("type", "hidden");
                hiddenField.Attributes.Add("name", Constants.InstanceIdQueryParameter);
                hiddenField.Attributes.Add("value", instanceId);

                output.PreContent.AppendHtml(hiddenField);
            }
        }
    }
}
