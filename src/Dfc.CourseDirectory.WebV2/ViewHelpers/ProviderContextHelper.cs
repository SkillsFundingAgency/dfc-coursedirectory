using System;
using Flurl;

namespace Dfc.CourseDirectory.WebV2.ViewHelpers
{
    public class ProviderContextHelper
    {
        private readonly IProviderContextProvider _providerContextProvider;

        public ProviderContextHelper(IProviderContextProvider providerContextProvider)
        {
            _providerContextProvider = providerContextProvider;
        }

        public ProviderInfo ProviderInfo => ProviderContext?.ProviderInfo;

        private ProviderContext ProviderContext => _providerContextProvider.GetProviderContext();

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
    }
}
