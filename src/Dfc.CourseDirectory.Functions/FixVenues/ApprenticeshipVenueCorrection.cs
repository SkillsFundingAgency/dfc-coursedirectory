using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;

namespace Dfc.CourseDirectory.Functions.FixVenues
{
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "}")]
    public class ApprenticeshipVenueCorrection
    {
        public Apprenticeship Apprenticeship { get; set; }
        public UnfixableApprenticeshipVenueReasons? UnfixableVenueReason { get; set; }
        public IList<ApprenticeshipLocationVenueCorrection> ApprenticeshipLocationVenueCorrections { get; set; } = new List<ApprenticeshipLocationVenueCorrection>();

        /// <summary>
        /// Reason for failure to update record in cosmos
        /// </summary>
        public string UpdateFailure { get; set; }

        /// <summary>
        ///  When the document was updated in cosmos with a correction
        /// </summary>
        public DateTime? UpdatedDate { get; set; }

        private string DebuggerDisplay => $"{Apprenticeship.Id}";
    }
}
