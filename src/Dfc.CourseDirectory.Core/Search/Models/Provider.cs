using System;

namespace Dfc.CourseDirectory.Core.Search.Models
{
    public class Provider
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Postcode { get; set; }

        public string Town { get; set; }

        public string Region { get; set; }

        public string UKPRN { get; set; }

        public int Status { get; set; }

        public string ProviderStatus { get; set; }

        public string CourseDirectoryName { get; set; }

        public string TradingName { get; set; }

        public string ProviderAlias { get; set; }
    }
}