using System;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.WebV2.SharedViews.Components
{
    public class ProviderInfoPanelViewModel
    {
        public Guid ProviderId { get; set; }
        public bool GotContact { get; set; }
        public IReadOnlyCollection<string> AddressParts { get; set; }
        public string ContactName { get; set; }
        public string Website { get; set; }
        public string Email { get; set; }
        public string Telephone { get; set; }
    }
}
