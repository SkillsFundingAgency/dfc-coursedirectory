using System;
using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetLatestApprenticeshipQAUnableToCompleteInfoForProvider
        : ISqlQuery<OneOf<None, ApprenticeshipQAUnableToCompleteInfo>>
    {
        public Guid ProviderId { get; set; }
    }
}
