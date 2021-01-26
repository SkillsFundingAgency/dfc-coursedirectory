using System.Linq;
using Dfc.CourseDirectory.Core;

namespace Dfc.CourseDirectory.Functions.FixVenues
{
    public class VenueCorrector
    {
        private readonly IClock _clock;

        public VenueCorrector(IClock clock)
        {
            _clock = clock;
        }

        /// <summary>
        /// Mutates the attached apprenticeship and its locations.
        /// </summary>
        /// <param name="apprenticeshipVenueCorrection">Changes to make plus apprenticeship to modify</param>
        /// <returns>true if any changes made</returns>
        public bool Apply(ApprenticeshipVenueCorrection apprenticeshipVenueCorrection)
        {
            bool dirty = false;

            foreach (var locationVenueCorrection in apprenticeshipVenueCorrection.ApprenticeshipLocationVenueCorrections
                .Where(c => c.VenueCorrection != null))
            {
                var locationToUpdate = apprenticeshipVenueCorrection.Apprenticeship.ApprenticeshipLocations
                    .Single(l => l.Id == locationVenueCorrection.LocationId);

                // mutating the in-memory copy of the apprenticeship directly is a bit nasty, but quicker to do for this throwaway code
                locationToUpdate.VenueId = locationVenueCorrection.VenueCorrection.Id;
                locationToUpdate.UpdatedDate = _clock.UtcNow;
                locationToUpdate.UpdatedBy = "VenueCorrector";
                apprenticeshipVenueCorrection.Apprenticeship.UpdatedDate = _clock.UtcNow;
                apprenticeshipVenueCorrection.Apprenticeship.UpdatedBy = "VenueCorrector";
                dirty = true;
            }

            return dirty;
        }
    }
}
