using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Filters;
using Dfc.CourseDirectory.WebV2.Security;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.WebUtilities;

namespace Dfc.CourseDirectory.WebV2.TagHelpers
{
    public class AppendProviderContextTagHelperComponent : TagHelperComponent
    {
        private const string AttributeName = "append-provider-context";

        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IProviderContextProvider _providerContextProvider;

        public AppendProviderContextTagHelperComponent(
            ICurrentUserProvider currentUserProvider,
            IProviderContextProvider providerContextProvider)
        {
            _currentUserProvider = currentUserProvider;
            _providerContextProvider = providerContextProvider;
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (!output.Attributes.ContainsName(AttributeName))
            {
                return;
            }

            var providerContext = await _providerContextProvider.GetProviderContext();
            if (providerContext == null)
            {
                throw new InvalidOperationException("No active provider context.");
            }

            var providerId = providerContext.ProviderInfo.ProviderId;

            // If the user is a provider user, no need to do anything since context is resolved from claims
            var currentUser = _currentUserProvider.GetCurrentUser();
            if (currentUser.IsProvider)
            {
                return;
            }

            if (output.Attributes.ContainsName("href"))
            {
                HandleHref();
            }
            else if (output.Attributes.ContainsName("action"))
            {
                HandleAction();
            }
            else
            {
                throw new NotSupportedException($"Cannot determine how to append provider context.");
            }

            output.Attributes.RemoveAll(AttributeName);

            void HandleAction()
            {
                var method = output.Attributes["action"]?.Value.ToString() ?? "post";

                if (method.Equals("GET", StringComparison.OrdinalIgnoreCase))
                {
                    var input = new TagBuilder("input");
                    input.Attributes.Add("type", "hidden");
                    input.Attributes.Add("name", ProviderContextResourceFilter.RouteValueKey);
                    input.Attributes.Add("value", providerId.ToString());
                    output.PreElement.AppendHtml(input);
                }
                else
                {
                    var currentAction = output.Attributes["action"].Value.ToString();

                    var newHref = QueryHelpers.AddQueryString(
                        currentAction,
                        ProviderContextResourceFilter.RouteValueKey,
                        providerId.ToString());

                    output.Attributes.SetAttribute("action", newHref);
                }
            }

            void HandleHref()
            {
                var currentHref = output.Attributes["href"].Value.ToString();

                var newHref = QueryHelpers.AddQueryString(
                    currentHref,
                    ProviderContextResourceFilter.RouteValueKey,
                    providerId.ToString());

                output.Attributes.SetAttribute("href", newHref);
            }
        }
    }
}
