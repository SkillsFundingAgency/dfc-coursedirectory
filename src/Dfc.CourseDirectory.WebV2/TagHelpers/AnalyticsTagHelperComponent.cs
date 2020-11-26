using System;
using Dfc.CourseDirectory.WebV2.Cookies;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;

namespace Dfc.CourseDirectory.WebV2.TagHelpers
{
    public class AnalyticsTagHelperComponent : TagHelperComponent
    {
        private readonly ICookieSettingsProvider _cookieSettingsProvider;
        private readonly GoogleAnalyticsOptions _analyticsSettings;
        private readonly GoogleTagManagerOptions _tagManagerSettings;

        public AnalyticsTagHelperComponent(
            ICookieSettingsProvider cookieSettingsProvider,
            IOptions<GoogleAnalyticsOptions> analyticsSettings,
            IOptions<GoogleTagManagerOptions> tagManagerSettings)
        {
            _cookieSettingsProvider = cookieSettingsProvider;
            _analyticsSettings = analyticsSettings.Value;
            _tagManagerSettings = tagManagerSettings.Value;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (_cookieSettingsProvider.GetPreferencesForCurrentUser()?.AllowAnalyticsCookies == true)
            {
                if (output.TagName.Equals("HEAD", StringComparison.OrdinalIgnoreCase))
                {
                    output.PreContent.AppendHtml(GetAnalyticsScripts());
                    output.PreContent.AppendHtml(GetTagManagerScripts());
                }
                else if (output.TagName.Equals("BODY", StringComparison.OrdinalIgnoreCase))
                {
                    output.PreContent.AppendHtml(GetTagManagerNoScript());
                }
            }
        }

        private IHtmlContent GetAnalyticsScripts()
        {
            return new HtmlString($@"
<!-- Google Analytics -->
<script>
window.ga=window.ga||function(){{(ga.q=ga.q||[]).push(arguments)}};ga.l=+new Date;
ga('create', '{_analyticsSettings.ClientId}', 'auto');
ga('send', 'pageview');
</script>
<script async src='https://www.google-analytics.com/analytics.js'></script>
<!-- End Google Analytics -->");
        }

        private IHtmlContent GetTagManagerScripts()
        {
            return new HtmlString($@"
<!-- Google Tag Manager -->
<script>(function(w,d,s,l,i){{w[l] = w[l] ||[];w[l].push({{'gtm.start':
new Date().getTime(),event:'gtm.js'}});var f=d.getElementsByTagName(s)[0],
j=d.createElement(s),dl=l!='dataLayer'?'&l='+l:'';j.async=true;j.src=
'https://www.googletagmanager.com/gtm.js?id='+i+dl;f.parentNode.insertBefore(j,f);
}})(window,document,'script','dataLayer','{_tagManagerSettings.ContainerId}');</script>
<!-- End Google Tag Manager -->");
        }

        private IHtmlContent GetTagManagerNoScript()
        {
            return new HtmlString($@"
<!-- Google Tag Manager (noscript) -->
<noscript><iframe src=""https://www.googletagmanager.com/ns.html?id={_tagManagerSettings.ContainerId}""
height=""0"" width=""0"" style=""display:none;visibility:hidden""></iframe></noscript>
<!-- End Google Tag Manager (noscript) -->");
        }
    }
}
