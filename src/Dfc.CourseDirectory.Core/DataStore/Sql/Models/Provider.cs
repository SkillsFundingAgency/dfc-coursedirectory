using System;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Models
{
    public class Provider
    {
        public Guid ProviderId { get; set; }
        public int Ukprn { get; set; }
        public string ProviderName { get; set; }
        public string Alias { get; set; }
        public ProviderDisplayNameSource DisplayNameSource { get; set; }
    }
}
