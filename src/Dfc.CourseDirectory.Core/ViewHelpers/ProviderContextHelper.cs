﻿using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.Extensions;
using Dfc.CourseDirectory.Core.Middleware;
using Flurl;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Dfc.CourseDirectory.Core.ViewHelpers
{
    public class ProviderContextHelper
    {
        private readonly IProviderContextProvider _providerContextProvider;

        public ProviderContextHelper(IProviderContextProvider providerContextProvider)
        {
            _providerContextProvider = providerContextProvider;
        }

        public ProviderInfo ProviderInfo => ProviderContext?.ProviderInfo;

        public IDictionary<string, string> RouteValues => new Dictionary<string, string>()
        {
            { ProviderContextMiddleware.RouteValueKey, ProviderInfo?.ProviderId.ToString() }
        };

        private ProviderContext ProviderContext =>
            _providerContextProvider.GetProviderContext(withLegacyFallback: true);

        public static implicit operator ProviderContext(ProviderContextHelper helper) =>
            helper.ProviderContext;

        public string AppendToUrl(string url)
        {
            if (url == null)
            {
                throw new ArgumentNullException(nameof(url));
            }

            return new Url(url).WithProviderContext(this);
        }

        public IHtmlContent CreateHiddenInput()
        {
            if (ProviderContext == null)
            {
                throw new InvalidOperationException("No provider context set.");
            }

            var tagBuilder = new TagBuilder("input");
            tagBuilder.Attributes.Add("type", "hidden");
            tagBuilder.Attributes.Add("name", ProviderContextMiddleware.RouteValueKey);
            tagBuilder.Attributes.Add("value", ProviderInfo.ProviderId.ToString());
            return tagBuilder;
        }
    }
}
