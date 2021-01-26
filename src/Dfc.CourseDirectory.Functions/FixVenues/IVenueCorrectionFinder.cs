using System.Collections.Generic;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;

namespace Dfc.CourseDirectory.Functions.FixVenues
{
    public interface IVenueCorrectionFinder
    {
        /// <summary>
        /// If the venueId of the location needs changing this will return the venue that should be attached.
        /// If there are multiple matches they are passed on for calling code to decide what to do with them.
        /// </summary>
        IReadOnlyList<Venue> GetMatchingVenues(ApprenticeshipLocation location, IReadOnlyCollection<Venue> providersVenues);
    }
}
