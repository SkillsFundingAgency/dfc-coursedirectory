using System.Collections.Generic;
using GovUk.Frontend.AspNetCore;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Dfc.CourseDirectory.WebV2.TagHelpers
{
    [HtmlTargetElement("pttcd-back-link")]
    public class BackLinkTagHelper : TagHelper
    {
        private readonly IGovUkHtmlGenerator _generator;

        public BackLinkTagHelper(IGovUkHtmlGenerator generator)
        {
            _generator = generator;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var tagBuilder = _generator.GenerateBackLink(
                href: "#",
                content: new HtmlString("Back"),
                attributes: new Dictionary<string, string>()
                {
                    { "class", "needs-js pttcd-c-back-link" },
                    { "aria-hidden", "" },
                    { "onclick", "window.history.back()" },
                });

            tagBuilder.Attributes.Remove("href");

            output.TagName = tagBuilder.TagName;
            output.TagMode = TagMode.StartTagAndEndTag;

            output.MergeAttributes(tagBuilder);
            output.Content.SetHtmlContent(tagBuilder.InnerHtml);
        }
    }
}
