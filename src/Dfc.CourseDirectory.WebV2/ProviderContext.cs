using System;

namespace Dfc.CourseDirectory.WebV2
{
    public class ProviderContext
    {
        public ProviderContext(ProviderInfo providerInfo, bool strict = true)
        {
            ProviderInfo = providerInfo ?? throw new ArgumentNullException(nameof(providerInfo));
            Strict = strict;
        }

        public ProviderInfo ProviderInfo { get; }

        /// <summary>
        /// If <c>true</c> indicates that the context should not be changed once set for a request.
        /// </summary>
        public bool Strict { get; }
    }
}
