using System;
using Dfc.CourseDirectory.WebV2.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries
{
    public class GetLatestApprenticeshipQAUnableToCompleteInfoForProvider
        : ISqlQuery<OneOf<None, ApprenticeshipQAUnableToCompleteInfo>>
    {
        public Guid ProviderId { get; set; }
    }
}
