using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    internal static class ApprenticeshipMappingHelper
    {
        public static async Task<IReadOnlyCollection<Apprenticeship>> MapApprenticeships(SqlMapper.GridReader reader)
        {
            var apprenticeships = reader.Read<ApprenticeshipHeaderResult, Standard, ApprenticeshipHeaderResult>(
                (header, standard) =>
                {
                    header.Standard = standard;
                    return header;
                },
                splitOn: "StandardCode");

            var locations = (reader.Read<ApprenticeshipLocationResult, Venue, ApprenticeshipLocationResult>(
                    (location, venue) =>
                    {
                        location.Venue = venue;
                        return location;
                    },
                    splitOn: "VenueId"))
                .GroupBy(al => al.ApprenticeshipId)
                .ToDictionary(g => g.Key, g => g.AsEnumerable());

            var apprenticeshipLocationSubRegions = (await reader.ReadAsync<ApprenticeshipLocationSubRegionResult>())
                .GroupBy(r => r.ApprenticeshipLocationId)
                .ToDictionary(g => g.Key, g => g.Select(r => r.RegionId).AsEnumerable());

            return apprenticeships
                .Select(a => new Apprenticeship()
                {
                    ApprenticeshipId = a.ApprenticeshipId,
                    CreatedOn = a.CreatedOn,
                    UpdatedOn = a.UpdatedOn,
                    ProviderId = a.ProviderId,
                    ProviderUkprn = a.ProviderUkprn,
                    Standard = a.Standard,
                    MarketingInformation = a.MarketingInformation,
                    ApprenticeshipWebsite = a.ApprenticeshipWebsite,
                    ContactEmail = a.ContactEmail,
                    ContactTelephone = a.ContactTelephone,
                    ContactWebsite = a.ContactWebsite,
                    ApprenticeshipLocations = locations.GetValueOrDefault(a.ApprenticeshipId, Enumerable.Empty<ApprenticeshipLocationResult>())
                        .Select(al =>
                        {
                            // Some bad Classroom-based data has a non-null National value; fix that up here
                            if (al.ApprenticeshipLocationType == ApprenticeshipLocationType.ClassroomBased)
                            {
                                al.National = null;
                            }

                            // If National is true then remove explicit Radius
                            if (al.National == true)
                            {
                                al.Radius = null;
                            }

                            return new ApprenticeshipLocation()
                            {
                                ApprenticeshipLocationId = al.ApprenticeshipLocationId,
                                ApprenticeshipLocationType = al.ApprenticeshipLocationType,
                                DeliveryModes = MapDeliveryModesFromSqlValue(al.DeliveryModes).ToArray(),
                                National = al.National,
                                Radius = al.Radius,
                                SubRegionIds = apprenticeshipLocationSubRegions.GetValueOrDefault(al.ApprenticeshipLocationId, Enumerable.Empty<string>()).ToArray(),
                                Telephone = al.Telephone,
                                Venue = al.Venue
                            };
                        })
                        .ToArray()
                })
                .ToArray();
        }

        public static IEnumerable<ApprenticeshipDeliveryMode> MapDeliveryModesFromSqlValue(byte value)
        {
            if ((value & 1) != 0)
            {
                yield return ApprenticeshipDeliveryMode.EmployerAddress;
            }

            if ((value & 2) != 0)
            {
                yield return ApprenticeshipDeliveryMode.DayRelease;
            }

            if ((value & 4) != 0)
            {
                yield return ApprenticeshipDeliveryMode.BlockRelease;
            }
        }

        public static byte MapDeliveryModesToSqlValue(IEnumerable<ApprenticeshipDeliveryMode> deliveryModes) =>
            deliveryModes
                .Select(v => v switch
                {
                    ApprenticeshipDeliveryMode.EmployerAddress => 1,
                    ApprenticeshipDeliveryMode.DayRelease => 2,
                    ApprenticeshipDeliveryMode.BlockRelease => 4,
                    (ApprenticeshipDeliveryMode)4 => 5,
                    _ => throw new ArgumentException($"Unknown delivery mode: '{v}'.")
                })
                .Aggregate((byte)0, (acc, v) => (byte)(acc | v));

        private class ApprenticeshipHeaderResult
        {
            public Guid ApprenticeshipId { get; set; }
            public DateTime CreatedOn { get; set; }
            public DateTime? UpdatedOn { get; set; }
            public Guid ProviderId { get; set; }
            public int ProviderUkprn { get; set; }
            public string MarketingInformation { get; set; }
            public string ApprenticeshipWebsite { get; set; }
            public string ContactTelephone { get; set; }
            public string ContactEmail { get; set; }
            public string ContactWebsite { get; set; }
            public Standard Standard { get; set; }
        }

        private class ApprenticeshipLocationResult : ApprenticeshipLocation
        {
            public Guid ApprenticeshipId { get; set; }
            public new byte DeliveryModes { get; set; }
        }

        private class ApprenticeshipLocationSubRegionResult
        {
            public Guid ApprenticeshipLocationId { get; set; }
            public string RegionId { get; set; }
        }
    }
}
