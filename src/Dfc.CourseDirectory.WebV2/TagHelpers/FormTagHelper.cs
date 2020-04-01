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
    }
}
