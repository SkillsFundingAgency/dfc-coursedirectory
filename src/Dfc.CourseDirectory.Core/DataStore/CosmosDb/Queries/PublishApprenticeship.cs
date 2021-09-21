using System;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries
{
    public class PublishApprenticeship : ISqlQuery<OneOf<NotFound, Success>>
    {
        public Guid ApprenticeshipId { get; set; }
        public UserInfo PublishedBy { get; set; }
        public DateTime PublishedOn { get; set; }
    }
}
