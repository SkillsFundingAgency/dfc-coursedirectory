using System;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class OnboardProviderUploadResult
    {
        public IReadOnlyCollection<Guid> OnboardedProviderIds { get; set; }
    }
}
