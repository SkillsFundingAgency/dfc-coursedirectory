using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewComponents
{
    public class GoogleAnalyticsOptions
    {
        public string TrackingCode { get; set; }
    }

    public class GoogleAnalyticsTagHelperComponent : TagHelperComponent
    {
        private readonly GoogleAnalyticsOptions _googleAnalyticsOptions;

        public GoogleAnalyticsTagHelperComponent(IOptions<GoogleAnalyticsOptions> googleAnalyticsOptions)
        {
            _googleAnalyticsOptions = googleAnalyticsOptions.Value;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            // Inject the code only in the head element
            if (string.Equals(output.TagName, "head", StringComparison.OrdinalIgnoreCase))
            {
                // Get the tracking code from the configuration
                //var trackingCode = "UA-137823248-1"; 
                //var trackingCode = _googleAnalyticsOptions.TrackingCode;
                //if (!string.IsNullOrEmpty(trackingCode))
                //// {
                // PostContent correspond to the text just before closing tag
                //output.PostContent
                //    .AppendHtml("<script async src='https://www.googletagmanager.com/gtag/js?id=")
                //    .AppendHtml(trackingCode)
                //    .AppendHtml("'></script><script>window.dataLayer=window.dataLayer||[];function gtag(){dataLayer.push(arguments)}gtag('js',new Date);gtag('config','")
                //    .AppendHtml(trackingCode)
                //    .AppendHtml("',{displayFeaturesTask:'null'});</script>");

                output.PostContent
                    .AppendHtml("<script>(function(w,d,s,l,i){w[l]=w[l]||[];w[l].push({'gtm.start':")
                    .AppendHtml("new Date().getTime(),event:'gtm.js'});var f=d.getElementsByTagName(s)[0],")
                    .AppendHtml("j=d.createElement(s),dl=l!='dataLayer'?'&l='+l:'';j.async=true;j.src=")
                    .AppendHtml("'https://www.googletagmanager.com/gtm.js?id='+i+dl;f.parentNode.insertBefore(j,f);")
                    .AppendHtml("})(window,document,'script','dataLayer','GTM-P9HWJ9L');</script>");

               // output.PostContent.AppendHtml("<script>(function(w,d,s,l,i){w[l]=w[l]||[];w[l].push({'gtm.start':new Date().getTime(),event:'gtm.js'});var f = d.getElementsByTagName(s)[0],j = d.createElement(s), dl = l != 'dataLayer' ? '&l=' + l : ''; j.async=true;j.src=https://www.googletagmanager.com/gtm.js?id='+i+dl;f.parentNode.insertBefore(j, f);})(window, document,'script','dataLayer','GTM-5R9DDR9');</script>");
              //  }
            }

            if (string.Equals(output.TagName, "body", StringComparison.OrdinalIgnoreCase))
            {

                output.PostContent
                    .AppendHtml("<noscript><iframe src='https://www.googletagmanager.com/ns.html?id=GTM-P9HWJ9L'")
                    .AppendHtml("height='0' width='0' style='display: none; visibility: hidden'></iframe></noscript>");
            }
        }
    }
}
