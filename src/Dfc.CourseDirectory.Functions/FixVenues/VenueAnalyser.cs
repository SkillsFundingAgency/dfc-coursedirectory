using System;
using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Functions.FixVenues
{
    public class VenueAnalyser
    {
        private readonly IVenueCorrectionFinder _venueCorrectionFinder;

        public VenueAnalyser(IVenueCorrectionFinder venueCorrectionFinder)
        {
            _venueCorrectionFinder = venueCorrectionFinder ?? throw new ArgumentNullException(nameof(venueCorrectionFinder));
        }

        public ApprenticeshipVenueCorrection AnalyseApprenticeship(Apprenticeship apprenticeship,IReadOnlyCollection<Venue> providerVenues)
        {
            if (!providerVenues.Any())
            {
                return new ApprenticeshipVenueCorrection
                {
                    Apprenticeship = apprenticeship,
                    UnfixableVenueReason = UnfixableApprenticeshipVenueReasons.ProviderHasNoLiveVenues
                };
            }

            return new ApprenticeshipVenueCorrection
            {
                Apprenticeship = apprenticeship,
                ApprenticeshipLocationVenueCorrections = RelevantApprenticeshipLocations(apprenticeship.ApprenticeshipLocations)
                    .Select(location => AnalyseLocation(location, providerVenues))
                    .Where(apprenticeshipLocationVenueCorrection => apprenticeshipLocationVenueCorrection != null)
                    .ToList()
            };
        }

        private ApprenticeshipLocationVenueCorrection AnalyseLocation(ApprenticeshipLocation location, IReadOnlyCollection<Venue> providersVenues)
        {
            CorruptionType corruptionType;
            if (providersVenues.Any(v => v.Id == location.VenueId))
            {
                return null; // valid record, nothing to do
            }

            if (location.VenueId == null)
            {
                corruptionType = CorruptionType.NullVenueId;
            }
            else if (location.VenueId == Guid.Empty)
            {
                corruptionType = CorruptionType.EmptyVenueId;
            }
            else
            {
                corruptionType = CorruptionType.VenueNotInProvidersLiveVenueList;
            }

            var matchingVenues = _venueCorrectionFinder.GetMatchingVenues(location, providersVenues);

            UnfixableLocationVenueReasons? unfixableLocationVenueReason = null;
            Venue matchingVenue = null;
            IReadOnlyList<Venue> duplicateVenues = null;
            if (matchingVenues == null || !matchingVenues.Any())
            {
                unfixableLocationVenueReason = UnfixableLocationVenueReasons.NoMatchingVenue;
            }
            else if (matchingVenues.Count() > 1)
            {
                unfixableLocationVenueReason = UnfixableLocationVenueReasons.DuplicateMatchingVenues;
                duplicateVenues = matchingVenues;
            }
            else
            {
                matchingVenue = matchingVenues.Single();
            }

            return new ApprenticeshipLocationVenueCorrection
            {
                LocationId = location.Id,
                VenueIdOriginal = location.VenueId,
                CorruptionType = corruptionType,
                VenueCorrection = matchingVenue,
                UnfixableLocationVenueReason = unfixableLocationVenueReason,
                DuplicateVenues = duplicateVenues,
            };
        }

        /// <summary>
        /// Find locations that are worth fixing
        /// </summary>
        public static IEnumerable<ApprenticeshipLocation> RelevantApprenticeshipLocations(IEnumerable<ApprenticeshipLocation> locations)
        {
            return locations
                .Where(l =>
                    l.RecordStatus == (int)ApprenticeshipStatus.Live
                    && l.LocationType == LocationType.Venue
                    && (l.ApprenticeshipLocationType == ApprenticeshipLocationType.ClassroomBased
                        || l.ApprenticeshipLocationType == ApprenticeshipLocationType.ClassroomBasedAndEmployerBased));
        }
    }
}
