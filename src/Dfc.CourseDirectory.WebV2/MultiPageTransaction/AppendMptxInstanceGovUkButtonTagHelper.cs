using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.WebUtilities;

namespace Dfc.CourseDirectory.WebV2.MultiPageTransaction
{
    [HtmlTargetElement("govuk-button", Attributes = "append-mptx-id")]
    public class AppendMptxInstanceGovUkButtonTagHelper : TagHelper
    {
        private readonly MptxInstanceProvider _mptxInstanceProvider;

        public AppendMptxInstanceGovUkButtonTagHelper(MptxInstanceProvider mptxInstanceProvider)
        {
            _mptxInstanceProvider = mptxInstanceProvider;
        }

        public override int Order => 10;

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            await base.ProcessAsync(context, output);

            var mptxInstance = _mptxInstanceProvider.GetInstance();
            if (mptxInstance == null)
            {
                throw new InvalidOperationException("No active MPTX instance.");
            }

            var currentHref = output.Attributes["href"].Value.ToString();

            var newHref = QueryHelpers.AddQueryString(
                currentHref,
                Constants.InstanceIdQueryParameter,
                mptxInstance.InstanceId);

            output.Attributes.SetAttribute("href", newHref);
        }
    }
}
