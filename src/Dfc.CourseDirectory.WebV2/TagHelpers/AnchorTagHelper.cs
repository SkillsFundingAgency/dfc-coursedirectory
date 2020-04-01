using Microsoft.AspNetCore.Mvc.Razor.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.WebV2.TagHelpers
{
    [HtmlTargetElement("a")]
    public class AnchorTagHelper : TagHelperComponentTagHelper
    {
        public AnchorTagHelper(ITagHelperComponentManager manager, ILoggerFactory loggerFactory)
            : base(manager, loggerFactory)
        {
        }
    }
}
