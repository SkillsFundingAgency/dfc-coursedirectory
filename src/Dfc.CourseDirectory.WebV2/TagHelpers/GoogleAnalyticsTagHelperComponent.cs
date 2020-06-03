using System;
using Dfc.CourseDirectory.WebV2.Cookies;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Dfc.CourseDirectory.WebV2.TagHelpers
{
    public class GoogleAnalyticsTagHelperComponent : TagHelperComponent
    {
        private readonly ICookieSettingsProvider _cookieSettingsProvider;

        private static readonly IHtmlContent _bodyTrackingCode = new HtmlString(@"
<noscript><iframe src=""https://www.googletagmanager.com/ns.html?id=GTM-P9HWJ9L"" height=""0"" width=""0"" style=""display: none; visibility: hidden""></iframe></noscript>");

        private static readonly IHtmlContent _headTrackingCode = new HtmlString(@"
<!-- Global site tag (gtag.js) - Google Analytics -->
<script async src=""https://www.googletagmanager.com/gtag/js?id=UA-39506500-1""></script>
<script>
  window.dataLayer = window.dataLayer || [];
  function gtag() { dataLayer.push(arguments); }
        gtag('js', new Date());

  gtag('config', 'UA-137823248-1');
</script>");

        public GoogleAnalyticsTagHelperComponent(ICookieSettingsProvider cookieSettingsProvider)
        {
            _cookieSettingsProvider = cookieSettingsProvider;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (_cookieSettingsProvider.GetPreferencesForCurrentUser()?.AllowAnalyticsCookies == true)
            {
                if (output.TagName.Equals("HEAD", StringComparison.OrdinalIgnoreCase))
                {
                    output.PreContent.AppendHtml(_headTrackingCode);
                }
                else if (output.TagName.Equals("BODY", StringComparison.OrdinalIgnoreCase))
                {
                    output.PostContent.AppendHtml(_bodyTrackingCode);
                }
            }
        }
    }
}
