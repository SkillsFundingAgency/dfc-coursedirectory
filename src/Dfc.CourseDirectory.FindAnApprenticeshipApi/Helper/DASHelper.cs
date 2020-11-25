using System;
using System.Collections.Generic;
using System.Linq;
using Dfc.Providerportal.FindAnApprenticeship.Interfaces.Helper;
using Dfc.Providerportal.FindAnApprenticeship.Interfaces.Models.Regions;
using Dfc.Providerportal.FindAnApprenticeship.Models;
using Dfc.Providerportal.FindAnApprenticeship.Models.DAS;
using Dfc.Providerportal.FindAnApprenticeship.Models.Enums;
using Dfc.Providerportal.FindAnApprenticeship.Models.Providers;
using Dfc.ProviderPortal.Packages;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace Dfc.Providerportal.FindAnApprenticeship.Helper
{
    public class DASHelper : IDASHelper
    {
        private const int _intIdentifier = 300000;

        // TODO: Add to config
        private const double NationalLat = 52.564269;
        private const double NationalLon = -1.466056;
        private readonly TelemetryClient _telemetryClient;

        public DASHelper(TelemetryClient telemetryClient)
        {
            Throw.IfNull(telemetryClient, nameof(telemetryClient));

            _telemetryClient = telemetryClient;
        }

        [Obsolete("Please don't use this any more, instead replace with a mapper class using something like AutoMapper",
            false)]
        public DasProvider CreateDasProviderFromProvider(int exportKey, Provider provider, FeChoice feChoice)
        {
            if (!int.TryParse(provider.UnitedKingdomProviderReferenceNumber, out var ukprn))
                throw new InvalidUkprnException(provider.UnitedKingdomProviderReferenceNumber);

            if (!provider.ProviderContact.Any()) throw new MissingContactException();

            try
            {
                var contactDetails = provider.ProviderContact.FirstOrDefault();

                return new DasProvider
                {
                    Id = provider.ProviderId ?? _intIdentifier + exportKey,
                    Email = contactDetails?.ContactEmail,
                    EmployerSatisfaction = feChoice?.EmployerSatisfaction,
                    LearnerSatisfaction = feChoice?.LearnerSatisfaction,
                    MarketingInfo = provider.MarketingInformation,
                    Name = provider.Name,
                    TradingName = provider.Name,
                    NationalProvider = provider.NationalApprenticeshipProvider,
                    UKPRN = ukprn,
                    Website = contactDetails?.ContactWebsiteAddress,
                    Phone = !string.IsNullOrWhiteSpace(contactDetails?.ContactTelephone1)
                        ? contactDetails?.ContactTelephone1
                        : contactDetails?.ContactTelephone2
                };
            }

            catch (Exception e)
            {
                throw new ProviderExportException(ukprn, e);
            }
        }

        public List<DasLocation> ApprenticeshipLocationsToLocations(
            int exportKey,
            Dictionary<string, ApprenticeshipLocation> locations)
        {
            var DASLocations = new List<DasLocation>();
            if (locations.Any())
                foreach (var (key, currentLocation) in locations)
                    // Regions
                    if (currentLocation.Regions != null && currentLocation.Regions.Length > 0)
                        DASLocations.AddRange(RegionsToLocations(exportKey, currentLocation.Regions));
                    // Venues
                    else if (currentLocation.Address != null)
                        DASLocations.Add(new DasLocation
                        {
                            Id = int.Parse(key),
                            Address = new DasAddress
                            {
                                Address1 = currentLocation.Address?.Address1,
                                Address2 = currentLocation.Address?.Address2,
                                County = currentLocation.Address?.County,
                                Lat = currentLocation.Address?.Latitude,
                                Long = currentLocation.Address?.Longitude,
                                Postcode = currentLocation.Address?.Postcode,
                                Town = currentLocation.Address?.Town
                            },
                            Name = currentLocation.Name,
                            Email = currentLocation.Address?.Email,
                            Website = currentLocation.Address?.Website,
                            Phone = currentLocation.Phone ?? currentLocation.Address?.Phone
                        });
                    // National
                    else if (currentLocation.National != null && currentLocation.National.Value)
                        DASLocations.Add(new DasLocation
                        {
                            Id = int.Parse(key),
                            Address = new DasAddress
                            {
                                Lat = NationalLat,
                                Long = NationalLon
                            }
                        });

            return DASLocations.Distinct(new DasLocationComparer()).ToList();
        }

        public List<DasStandard> ApprenticeshipsToStandards(int exportKey, IEnumerable<Apprenticeship> apprenticeships,
            Dictionary<string, ApprenticeshipLocation> validLocations)
        {
            var standards = new List<DasStandard>();
            foreach (var apprenticeship in apprenticeships)
            {
                if (!apprenticeship.StandardCode.HasValue) continue;
                var apprenticeshipLocations =
                    LinkApprenticeshipLocationsToProvider(validLocations, apprenticeship);

                standards.Add(new DasStandard
                {
                    StandardCode = apprenticeship.StandardCode.Value,
                    MarketingInfo = apprenticeship.MarketingInformation,
                    StandardInfoUrl = apprenticeship.Url,
                    Contact = new DasContact
                    {
                        ContactUsUrl = apprenticeship.ContactWebsite,
                        Email = apprenticeship.ContactEmail,
                        Phone = apprenticeship.ContactTelephone
                    },
                    Locations = CreateLocationRef(exportKey, apprenticeshipLocations)
                });
            }

            return standards;
        }

        public List<DasFramework> ApprenticeshipsToFrameworks(int exportKey,
            IEnumerable<Apprenticeship> apprenticeships,
            Dictionary<string, ApprenticeshipLocation> validLocations)
        {
            var frameworks = new List<DasFramework>();

            foreach (var apprenticeship in apprenticeships)
            {
                if (!apprenticeship.FrameworkCode.HasValue) continue;

                var apprenticeshipLocations =
                    LinkApprenticeshipLocationsToProvider(validLocations, apprenticeship);

                frameworks.Add(new DasFramework
                {
                    FrameworkCode = apprenticeship.FrameworkCode.Value,
                    FrameworkInfoUrl = apprenticeship.Url,
                    MarketingInfo = HtmlHelper.StripHtmlTags(apprenticeship.MarketingInformation, true),
                    PathwayCode = apprenticeship.PathwayCode,
                    ProgType = apprenticeship.ProgType,
                    Contact = new DasContact
                    {
                        ContactUsUrl = apprenticeship.ContactWebsite,
                        Email = apprenticeship.ContactEmail,
                        Phone = apprenticeship.ContactTelephone
                    },
                    Locations = CreateLocationRef(exportKey, apprenticeshipLocations)
                });
            }

            return frameworks;
        }

        private List<DasLocationRef> CreateLocationRef(int exportKey,
            Dictionary<string, ApprenticeshipLocation> locations)
        {
            var locationRefs = new List<DasLocationRef>();
            var subRegionItemModels = new SelectRegionModel().RegionItems.SelectMany(x => x.SubRegion);
            foreach (var (key, currentLocation) in locations)
                // Regions
                if (currentLocation.Regions != null && currentLocation.Regions.Length > 0)
                {
                    foreach (var region in currentLocation.Regions)
                    {
                        var locationId = subRegionItemModels
                            .Where(x => x.Id == region)
                            .Select(y => $"{y.ApiLocationId.Value}")
                            .FirstOrDefault();

                        if (string.IsNullOrWhiteSpace(locationId)) continue;

                        var regionId = locationId.Substring(locationId.Length - 3, 3);

                        var regionIndex = $"{exportKey}2{regionId}";

                        locationRefs.Add(new DasLocationRef
                        {
                            Id = int.Parse(regionIndex),
                            DeliveryModes = ConvertToApprenticeshipDeliveryModes(currentLocation),
                            Radius = currentLocation.Radius ?? 50 // TODO: Add to config
                        });
                    }
                }

                else
                {
                    var isNational = currentLocation.National != null && currentLocation.National.Value;
                    var radius = isNational
                        ? 500 // National
                        : currentLocation.Radius ?? 30;

                    locationRefs.Add(new DasLocationRef
                    {
                        Id = int.Parse(key),
                        DeliveryModes = ConvertToApprenticeshipDeliveryModes(currentLocation),
                        Radius = radius
                    });
                }

            return locationRefs;
        }

        public List<string> ConvertToApprenticeshipDeliveryModes(ApprenticeshipLocation location)
        {
            var validDeliveryModes = location.DeliveryModes
                .Where(m => Enum.IsDefined(typeof(DeliveryMode), m))
                .Select(m => (DeliveryMode) m).ToList();

            var culledDeliveryModes = location.DeliveryModes.Count - validDeliveryModes.Count;

            if (culledDeliveryModes > 0)
            {
                var evt = new ExceptionTelemetry();

                var undefinedModes = string.Join(", ", location.DeliveryModes
                    .Where(m => !Enum.IsDefined(typeof(DeliveryMode), m)));

                var errorMessage = $"Could not map mode(s) \'{undefinedModes}\' to a matching {nameof(DeliveryMode)}";
                Console.WriteLine($"Culling {culledDeliveryModes} delivery modes: {errorMessage}");

                evt.Properties.TryAdd("LocationId", $"{location.ToAddressHash()}");
                evt.Properties.TryAdd("Message", errorMessage);

                _telemetryClient.TrackException(
                    new LocationExportException(
                        location.Id.ToString(),
                        new InvalidCastException(errorMessage)));
            }

            return validDeliveryModes
                .Select(m => m.ToDescription())
                .ToList();
        }


        private IEnumerable<DasLocation> RegionsToLocations(int exportKey, string[] regionCodes)
        {
            var apprenticeshipLocations = new List<DasLocation>();
            var regions =
                new SelectRegionModel().RegionItems.SelectMany(x => x.SubRegion.Where(y => regionCodes.Contains(y.Id)));

            foreach (var region in regions)
            {
                if (!region.ApiLocationId.HasValue) continue;

                var locationId = $"{region.ApiLocationId.Value}";
                if (!string.IsNullOrWhiteSpace(locationId))
                {
                    // get last three digits of region code
                    var regionId = locationId.Substring(locationId.Length - 3, 3);

                    var regionIndex = $"{exportKey}2{regionId}";
                    var dasLocation = new DasLocation
                    {
                        Id = int.Parse(regionIndex),
                        Name = region.SubRegionName,
                        Address = new DasAddress
                        {
                            Address1 = region.SubRegionName,
                            Lat = region.Latitude,
                            Long = region.Longitude
                        }
                    };
                    apprenticeshipLocations.Add(dasLocation);
                }
            }

            return apprenticeshipLocations;
        }

        /// <summary>
        ///     Link the apprenticeship location with a valid Provider location
        /// </summary>
        /// <param name="validLocations">A de-duped list of validated provider locations</param>
        /// <param name="apprenticeship">the apprenticeship to extract locations from</param>
        /// <returns></returns>
        private static Dictionary<string, ApprenticeshipLocation> LinkApprenticeshipLocationsToProvider(
            Dictionary<string, ApprenticeshipLocation> validLocations, Apprenticeship apprenticeship)
        {
            var linkedLocations = new Dictionary<string, ApprenticeshipLocation>();

            foreach (var currentLocation in apprenticeship.ApprenticeshipLocations)
            {
                var match = validLocations.SingleOrDefault(providerLocation =>
                    currentLocation.ToAddressHash() == providerLocation.Value.ToAddressHash());

                if (!match.Equals(default(KeyValuePair<string, ApprenticeshipLocation>)))
                    linkedLocations.TryAdd(match.Key, currentLocation);
            }

            return linkedLocations;
        }
    }
}