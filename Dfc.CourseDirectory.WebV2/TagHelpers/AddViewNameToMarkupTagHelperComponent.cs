using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Hosting;

namespace Dfc.CourseDirectory.WebV2.TagHelpers
{
    [HtmlTargetElement("html")]
    public class AddViewNameToMarkupTagHelperComponent : TagHelper
    {
        private readonly IWebHostEnvironment _environment;

        public AddViewNameToMarkupTagHelperComponent(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (!_environment.IsProduction())
            {
                output.Attributes.Add("data-viewpath", ViewContext.View.Path);
            }
        }
    }
}
