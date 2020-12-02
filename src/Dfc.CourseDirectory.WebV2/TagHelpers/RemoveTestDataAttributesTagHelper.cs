using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Hosting;

namespace Dfc.CourseDirectory.WebV2.TagHelpers
{
    [HtmlTargetElement("*", Attributes = "data-testid")]
    public class RemoveTestDataAttributesTagHelper : TagHelper
    {
        private readonly IWebHostEnvironment _environment;

        public override int Order => 99;

        public RemoveTestDataAttributesTagHelper(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (_environment.IsProduction())
            {
                output.Attributes.RemoveAll("data-testid");
            }
        }
    }
}
