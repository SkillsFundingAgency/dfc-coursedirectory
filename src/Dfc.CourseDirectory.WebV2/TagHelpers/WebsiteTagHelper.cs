using System.Collections.Generic;
using System.Linq;
using GovUk.Frontend.AspNetCore;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using static GovUk.Frontend.AspNetCore.ComponentDefaults;

namespace Dfc.CourseDirectory.WebV2.TagHelpers
{
    [HtmlTargetElement("pttcd-website-input")]
    public class WebsiteTagHelper : TagHelper
    {
        private readonly IGovUkHtmlGenerator _generator;

        [HtmlAttributeName("asp-for")]
        public ModelExpression For { get; set; }

        public string Id { get; set; } = "Website";
        public string Name { get; set; } = "Website";
        public string Label { get; set; }
        public string Hint { get; set; }
        public string Type { get; set; } = "text";
        public string Value { get; set; }        
        public ModelStateEntry ModelStateEntry { get; set; }

        public WebsiteTagHelper(IGovUkHtmlGenerator generator)
        {
            _generator = generator;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.Attributes.Add("class", "govuk-form-group");

            var errorHtml = string.Empty;
            var errorInputClass = string.Empty;
            
            var error = ModelStateEntry?.Errors.Any() ?? false;
            var errorMessage = string.Empty;
            var webSecurityRiskError = false;
            if (error)
            {
                errorMessage = ModelStateEntry.Errors[0].ErrorMessage;

                webSecurityRiskError = errorMessage.Contains("security issue");

                output.Attributes.SetAttribute("class", "govuk-form-group--error govuk-form-group");
                errorHtml = $@"
                <span class=""govuk-error-message"" id =""{Id}-error""><span class=""govuk-visually-hidden"">Error</span>{errorMessage}</span>";
                errorInputClass = "govuk-input--error";
            }            

            // Generate the label
            var labelHtml = $@"
            <label class='govuk-label--m govuk-label' for='{Id}'>
                {Label}
            </label>";

            // Generate the hint
            var hintHtml = string.IsNullOrEmpty(Hint) ? "" : $@"
            <div id='{Id}-hint' class='govuk-hint'>
                {Hint}
            </div>";

            var inputHtml = $@"
            <input aria-describedby='{Id}-hint' class='{errorInputClass} govuk-input' id='{Id}' name='{Name}' type='{Type}' value='{Value}'>";

            var secureWebsiteHintMessage = string.Empty;
            var htmlContent = string.Empty;
            if (error && webSecurityRiskError)
            {
                secureWebsiteHintMessage = $@"<div secure-website-message-hint-text style=""display:block"">
            <span class=""govuk-hint govuk-!-margin-top-3"">
                This URL has been reported by <a href=""https://cloud.google.com/web-risk/docs/advisory"" target=""_blank"">Google Cloud's Web Risk (opens in a new tab)</a> as having a security issue and might be malicious.
            </span>
            <span class=""govuk-hint govuk-!-margin-top-3"">
                Check the URL you have entered is correct. If the URL is correct, you'll need to review the security of your website.
                You cannot upload this URL to the course directory until it can pass this security check.
            </span>
        </div>";

                htmlContent = $"{labelHtml}{errorHtml}{hintHtml}{inputHtml}{secureWebsiteHintMessage}";
            }
            else
            {
                htmlContent = $"{labelHtml}{hintHtml}{errorHtml}{inputHtml}";
            }

            // Combine all elements
            output.Content.SetHtmlContent(htmlContent);
        }
    }

    public static class ModelExpressionExtensions
    {
        // Extension method to extract validation attributes from the model metadata
        public static IDictionary<string, string> GetValidationAttributes(this ModelExpression modelExpression)
        {
            var attributes = new Dictionary<string, string>();

            if (modelExpression?.Metadata?.IsRequired ?? false)
            {
                attributes.Add("data-val", "true");
                attributes.Add("data-val-required", modelExpression.Metadata.ModelBindingMessageProvider.ValueMustNotBeNullAccessor(modelExpression.Name));
            }

            return attributes;
        }
    }
}
