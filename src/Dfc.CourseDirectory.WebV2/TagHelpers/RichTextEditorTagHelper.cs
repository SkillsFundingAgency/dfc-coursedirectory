using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Dfc.CourseDirectory.WebV2.TagHelpers
{
    [HtmlTargetElement(ParentTag = "pttcd-rich-text-editor", TagStructure = TagStructure.NormalOrSelfClosing)]
    [RestrictChildren("govuk-textarea", "govuk-character-count")]
    public class RichTextEditorTagHelper : TagHelper
    {
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.Attributes.Add("data-pttcd-module", "richtext");

            await output.GetChildContentAsync();
        }
    }
}
