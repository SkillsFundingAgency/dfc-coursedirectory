using System;
using System.Collections.Generic;
using System.Diagnostics;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;

namespace Dfc.CourseDirectory.Functions.FixVenues
{
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "}")]
    public class ApprenticeshipLocationVenueCorrection
    {
        /* source data */

        public Guid LocationId { get; set; }
        public Guid? VenueIdOriginal { get; set; }
        public CorruptionType CorruptionType { get; set;}

        /* analysis */

        public UnfixableLocationVenueReasons? UnfixableLocationVenueReason { get; set; }

        public IReadOnlyList<Venue> DuplicateVenues { get; set; }

        /// <summary>
        /// Null if no fix available
        /// </summary>
        public Venue VenueCorrection { get; set; }

        /* debug */

        private string DebuggerDisplay =>
            UnfixableLocationVenueReason != null
                ? $"Location {LocationId} {CorruptionType} {UnfixableLocationVenueReason}"
                : $"Location {LocationId} {CorruptionType}, new id: {VenueCorrection?.Id}";
    }
}
