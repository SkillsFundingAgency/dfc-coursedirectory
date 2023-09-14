using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class UpdateProviderFromUkrlpData : ISqlQuery<Success>
    {
        public Guid ProviderId { get; set; }
        public string ProviderName { get; set; }
        public string ProviderStatus { get; set; }
        public ProviderStatus Status { get; set; }
        public string Alias { get; set; }
        public DateTime DateUpdated { get; set; }
        public string UpdatedBy { get; set; }
        //public IEnumerable<ProviderAlias> Aliases { get; set; }
        public ProviderContact Contact { get; set; }
    }
}
