using GovUk.Frontend.AspNetCore;
using GovUk.Frontend.AspNetCore.TagHelpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Dfc.CourseDirectory.WebV2.TagHelpers
{
    [HtmlTargetElement("pttcd-search-input")]
    [RestrictChildren("pttcd-search-input-label", "pttcd-search-input-hint", "pttcd-search-input-error-message")]
    public class SearchInputTagHelper : InputTagHelper
    {
        public SearchInputTagHelper(IGovUkHtmlGenerator htmlGenerator)
            : base(htmlGenerator)
        {
        }

        protected override TagBuilder GenerateElement(
            TagHelperContext context,
            FormGroupBuilder builder,
            FormGroupElementContext elementContext)
        {
            var input = base.GenerateElement(context, builder, elementContext);

            var wrapper = new TagBuilder("div");
            wrapper.AddCssClass("pttcd-c-search-input");
            wrapper.InnerHtml.AppendHtml(input);

            var button = new TagBuilder("button");
            button.AddCssClass("pttcd-c-search-input__button");
            button.Attributes.Add("type", "submit");
            button.InnerHtml.Append("Search");
            wrapper.InnerHtml.AppendHtml(button);

            return wrapper;
        }
    }

    [HtmlTargetElement("pttcd-search-input-label", ParentTag = "pttcd-search-input")]
    public class SearchInputLabelTagHelper : InputLabelTagHelper
    {
    }

    [HtmlTargetElement("pttcd-search-input-hint", ParentTag = "pttcd-search-input")]
    public class SearchInputHintTagHelper : InputHintTagHelper
    {
    }

    [HtmlTargetElement("pttcd-search-input-error-message", ParentTag = "pttcd-search-input")]
    public class SearchInputErrorMessageTagHelper : InputErrorMessageTagHelper
    {
    }
}
