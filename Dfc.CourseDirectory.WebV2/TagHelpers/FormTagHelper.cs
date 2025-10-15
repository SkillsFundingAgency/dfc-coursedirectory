using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Razor.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.WebV2.TagHelpers
{
    [HtmlTargetElement("form")]
    public class FormTagHelper : TagHelperComponentTagHelper
    {
        public FormTagHelper(ITagHelperComponentManager manager, ILoggerFactory loggerFactory)
            : base(manager, loggerFactory)
        {
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            await base.ProcessAsync(context, output);

            output.Attributes.Add("novalidate", string.Empty);
        }
    }
}
