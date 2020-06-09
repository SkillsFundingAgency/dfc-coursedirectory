using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Dfc.CourseDirectory.WebV2.TagHelpers
{
    [HtmlTargetElement("*", Attributes = "needs-js")]
    public class NeedsJsTagHelper : TagHelper
    {
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.Attributes.RemoveAll("needs-js");
            output.Attributes.Add("aria-hidden", string.Empty);
            output.AddClass("pttcd-needs-js", HtmlEncoder.Default);

            await base.ProcessAsync(context, output);
        }
    }
}
