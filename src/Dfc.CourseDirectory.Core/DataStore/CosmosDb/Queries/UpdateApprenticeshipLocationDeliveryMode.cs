using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries
{
    public class UpdateApprenticeshipLocationDeliveryMode : ICosmosDbQuery<OneOf<NotFound, Success>>
    {
        public Guid ApprenticeshipId { get; set; }
        public int ProviderUkprn { get; set; }
        public Guid ApprenticeshipLocationId { get; set; }
        public IReadOnlyCollection<ApprenticeshipDeliveryMode> DeliveryModes { get; set; }
    }
}
