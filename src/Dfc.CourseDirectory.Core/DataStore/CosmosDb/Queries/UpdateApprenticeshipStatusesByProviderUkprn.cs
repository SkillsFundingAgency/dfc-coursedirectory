using Dfc.CourseDirectory.Core.Models;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries
{
    public class UpdateApprenticeshipStatusesByProviderUkprn : ICosmosDbQuery<Success>
    {
        public int ProviderUkprn { get; set; }
        public ApprenticeshipStatus CurrentStatus { get; set; }
        public ApprenticeshipStatus NewStatus { get; set; }
    }
}
