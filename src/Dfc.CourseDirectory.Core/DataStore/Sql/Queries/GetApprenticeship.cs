using System;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetApprenticeship : ISqlQuery<Apprenticeship>
    {
        public Guid ApprenticeshipId { get; set; }
    }
}
