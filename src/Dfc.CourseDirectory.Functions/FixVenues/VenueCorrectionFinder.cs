using System;
using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;

namespace Dfc.CourseDirectory.Functions.FixVenues
{
    public class VenueCorrectionFinder : IVenueCorrectionFinder
    {
        /// <inheritdoc/>
        public IReadOnlyList<Venue> GetMatchingVenues(ApprenticeshipLocation location, IReadOnlyCollection<Venue> providersVenues)
        {
            // limit fixes to:
            // * null/empty -- i.e. missing FKs (39k at last count)
            // * not in provider's venue list -- i.e. belonging to another provider or missing entirely (9 at last count)
            if (location.VenueId != null && location.VenueId != Guid.Empty && providersVenues.Any(v => v.Id == location.VenueId))
            {
                return new Venue[]{};
            }

            return FindMatchingVenues(providersVenues, location);
        }

        private static IReadOnlyList<Venue> FindMatchingVenues(IEnumerable<Venue> venues, ApprenticeshipLocation location)
        {
            // Below 10 matching fields we start to get false-positives in production data
            const int matchThreshold = 10;

            var matches = venues.Where(v => (
                    VenueTightMatch(v, location)
                    || VenueNameMatches(v, location)
                    || VenueAddressMatches(v, location)
                ) && MatchedFieldsCount(v, location) >= matchThreshold
            ).ToList();

            return matches;
        }

        private static int MatchedFieldsCount(Venue venue, ApprenticeshipLocation location)
        {
            int matchCounter = 0;

            FieldMatchCounter(venue.VenueName, location.Name, ref matchCounter);
            FieldMatchCounter(venue.AddressLine1, location.Address?.Address1, ref matchCounter);
            FieldMatchCounter(venue.AddressLine2, location.Address?.Address2, ref matchCounter);
            FieldMatchCounter(venue.Town, location.Address?.Town, ref matchCounter);
            FieldMatchCounter(venue.County, location.Address?.County, ref matchCounter);
            FieldMatchCounter(venue.Postcode, location.Address?.Postcode, ref matchCounter);
            FieldMatchCounter(venue.Latitude, location.Address?.Latitude, ref matchCounter);
            FieldMatchCounter(venue.Longitude, location.Address?.Longitude, ref matchCounter);
            FieldMatchCounter(venue.PHONE, location.Address?.Phone, ref matchCounter);
            FieldMatchCounter(venue.Email, location.Address?.Email, ref matchCounter);
            FieldMatchCounter(venue.Website, location.Address?.Website, ref matchCounter);

            return matchCounter;
        }

        private static void FieldMatchCounter(string a, string b, ref int matchCounter)
        {
            if (!string.IsNullOrWhiteSpace(a) && a == b)
            {
                matchCounter++;
            }
        }

        private static void FieldMatchCounter(decimal a, decimal? b, ref int matchCounter)
        {
            if (a != default && a == b)
            {
                matchCounter++;
            }
        }

        private static bool VenueNameMatches(Venue venue, ApprenticeshipLocation location)
        {
            return !string.IsNullOrWhiteSpace(venue.VenueName) && venue.VenueName == location.Name;
        }

        private static bool VenueAddressMatches(Venue venue, ApprenticeshipLocation location)
        {
            return (!string.IsNullOrWhiteSpace(venue.AddressLine1) && venue.AddressLine1 == location.Address?.Address1) &&
                   (!string.IsNullOrWhiteSpace(venue.Postcode) && venue.Postcode == location.Address?.Postcode);
        }

        private static bool VenueTightMatch(Venue venue, ApprenticeshipLocation location)
        {
            return VenueNameMatches(venue, location) && VenueAddressMatches(venue, location);
        }

    }
}
