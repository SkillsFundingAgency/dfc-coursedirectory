using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Services.ApprenticeshipService;
using Dfc.CourseDirectory.Services.Models;
using Dfc.CourseDirectory.Services.Models.Apprenticeships;
using Dfc.CourseDirectory.Services.Models.Regions;
using Dfc.CourseDirectory.Services.VenueService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.Helpers
{
    public class ApprenticeshipProvisionHelper : IApprenticeshipProvisionHelper
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IApprenticeshipService _apprenticeshipService;
        private readonly IVenueService _venueService;
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly ICSVHelper _CSVHelper;

        private ISession _session => _contextAccessor.HttpContext.Session;

        public ApprenticeshipProvisionHelper(
            IHttpContextAccessor contextAccessor,
            IApprenticeshipService apprenticeshipService,
            IVenueService venueService,
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher,
            ICSVHelper CSVHelper)
        {
            _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
            _apprenticeshipService = apprenticeshipService ?? throw new ArgumentNullException(nameof(apprenticeshipService));
            _venueService = venueService ?? throw new ArgumentNullException(nameof(venueService));
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher ?? throw new ArgumentNullException(nameof(cosmosDbQueryDispatcher));
            _CSVHelper = CSVHelper ?? throw new ArgumentNullException(nameof(CSVHelper));
        }

        public async Task<FileStreamResult> DownloadCurrentApprenticeshipProvisions()
        {
            var UKPRN = _session.GetInt32("UKPRN");

            if (!UKPRN.HasValue)
            {
                return null;
            }

            var provider = await _cosmosDbQueryDispatcher.ExecuteQuery(new GetProviderByUkprn { Ukprn = UKPRN.Value });
            var providerName = provider?.ProviderName.Replace(" ", "");

            var apprenticeships = await _apprenticeshipService.GetApprenticeshipByUKPRN(UKPRN.ToString());
            var csvApprenticeships = await ApprenticeshipsToCsvApprenticeships(
                apprenticeships.Value.Where(y => y.RecordStatus == RecordStatus.Live));

            return CsvApprenticeshipsToFileStream(csvApprenticeships, providerName);
        }

        private async Task<IEnumerable<CsvApprenticeship>> ApprenticeshipsToCsvApprenticeships(IEnumerable<Apprenticeship> apprenticeships)
        {
            List<CsvApprenticeship> csvApprenticeships = new List<CsvApprenticeship>();

            foreach (var apprenticeship in apprenticeships)
            {
                foreach (var apprenticeshipLocation in apprenticeship.ApprenticeshipLocations)
                {
                    //Sanitise regions
                    if (apprenticeshipLocation.Regions != null)
                    {
                        if(apprenticeshipLocation.Regions.Any())
                            apprenticeshipLocation.Regions = _CSVHelper.SanitiseRegionTextForCSVOutput(apprenticeshipLocation.Regions);
                    }
                        
                    var csvApprenticeshipLocation = await MapCsvApprenticeship(apprenticeship, apprenticeshipLocation);

                    csvApprenticeships.Add(csvApprenticeshipLocation);
                }
            }
            return csvApprenticeships;
        }

        private async Task<CsvApprenticeship> MapCsvApprenticeship(Apprenticeship apprenticeship, ApprenticeshipLocation location)
        {
            var selectRegionModel = new SelectRegionModel();

            return new CsvApprenticeship
            {
                StandardCode = apprenticeship.StandardCode?.ToString(),
                Version = apprenticeship.Version?.ToString(),
                ApprenticeshipInformation = _CSVHelper.SanitiseTextForCSVOutput(apprenticeship.MarketingInformation),
                ApprenticeshipWebpage = apprenticeship.Url,
                ContactEmail = apprenticeship.ContactEmail,
                ContactPhone = apprenticeship.ContactTelephone,
                ContactURL = apprenticeship.ContactWebsite,
                DeliveryMethod = DeliveryMethodConvert(location.ApprenticeshipLocationType),
                Venue = location.VenueId.HasValue ? (await _venueService.GetVenueByIdAsync(new GetVenueByIdCriteria(location.VenueId.ToString()))).Value?.VenueName : string.Empty,
                Radius = location.Radius?.ToString(),
                DeliveryMode = DeliveryModeConvert(location.DeliveryModes),
                AcrossEngland = location.ApprenticeshipLocationType == ApprenticeshipLocationType.ClassroomBasedAndEmployerBased ?
                    AcrossEnglandConvert(location.Radius, location.National) : 
                    String.Empty,
                NationalDelivery = location.ApprenticeshipLocationType == ApprenticeshipLocationType.EmployerBased ?  BoolConvert(location.National) : string.Empty,
                Region = location.Regions != null ? _CSVHelper.SemiColonSplit(
                                                                    selectRegionModel.RegionItems
                                                                    .Where(x => location.Regions.Contains(x.Id))
                                                                    .Select(y => _CSVHelper.SanitiseTextForCSVOutput(y.RegionName).Replace(",", "")).ToList())
                                                                    : string.Empty,
                Subregion =  location.Regions != null ? _CSVHelper.SemiColonSplit(
                                                                    selectRegionModel.RegionItems.SelectMany(
                                                                        x => x.SubRegion.Where(
                                                                            y => location.Regions.Contains(y.Id)).Select(
                                                                                z => _CSVHelper.SanitiseTextForCSVOutput(z.SubRegionName).Replace(",", "")).ToList())) : string.Empty,
            };
        }

        private FileStreamResult CsvApprenticeshipsToFileStream(IEnumerable<CsvApprenticeship> csvApprenticeships, string providerName)
        {
            var report = string.Join(Environment.NewLine, _CSVHelper.ToCsv(csvApprenticeships));

            var ms = new MemoryStream(Encoding.ASCII.GetBytes(report))
            {
                Position = 0
            };

            var now = DateTime.Now;
            return new FileStreamResult(ms, MediaTypeNames.Text.Plain)
            {
                FileDownloadName = $"{providerName}_Apprenticeships_{now.Day:00}_{now.Month:00}_{now.Year}_{now.Hour:00}_{now.Minute:00}.csv"
            };
        }

        private string BoolConvert(bool? input) => input switch
        {
            true => "Yes",
            false => "No",
            _ => string.Empty,
        };

        private string DeliveryMethodConvert(ApprenticeshipLocationType input) => input switch
        {
            ApprenticeshipLocationType.ClassroomBased => "Classroom",
            ApprenticeshipLocationType.ClassroomBasedAndEmployerBased => "Both",
            ApprenticeshipLocationType.EmployerBased => "Employer",
            _ => string.Empty,
        };

        private string DeliveryModeConvert(List<int> modes)
        {
            return string.Join(";", modes.Select(m => m switch
            {
                (int)ApprenticeShipDeliveryLocation.DayRelease => "Day",
                (int)ApprenticeShipDeliveryLocation.BlockRelease => "Block",
                (int)ApprenticeShipDeliveryLocation.EmployerAddress => "Employer",
                _ => null,
            }).Where(m => m != null));
        }

        private string AcrossEnglandConvert(int? radius, bool? national)
        {
            if (!radius.HasValue)
            {
                return string.Empty;
            }

            if (!national.HasValue)
            {
                return string.Empty;
            }

            if (radius.Value == 600 && national.Value)
            {
                return BoolConvert(true);
            }

            return string.Empty;
        }
    }
}
