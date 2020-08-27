using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Dfc.CourseDirectory.WebV2.TagHelpers
{
    [HtmlTargetElement("pttcd-notification")]
    public class NotificationTagHelper : TagHelper
    {
        [HtmlAttributeName("id")]
        public string Id { get; set; }

        [HtmlAttributeName("title")]
        public string Title { get; set; }

        [HtmlAttributeName("type")]
        public NotificationType Type { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.AddClass("pttcd-c-notification", HtmlEncoder.Default);
            output.AddClass("govuk-!-margin-bottom-5", HtmlEncoder.Default);
            output.AddClass("govuk-!-margin-top-3", HtmlEncoder.Default);

            if (Id != null)
            {
                output.Attributes.Add("id", Id);
            }

            if (Type == NotificationType.Success)
            {
                output.AddClass("pttcd-c-notification-success", HtmlEncoder.Default);
            }

            var title = new TagBuilder("h2");
            title.AddCssClass("govuk-heading-m");
            title.InnerHtml.Append(Title);

            var childContent = await output.GetChildContentAsync();
            if (childContent.IsEmptyOrWhiteSpace)
            {
                title.AddCssClass("govuk-!-margin-bottom-0");
            }

            output.Content.AppendHtml(title);
            output.Content.AppendHtml(childContent);
        }

        public enum NotificationType
        {
            Notification,
            Success
        }
    }
}