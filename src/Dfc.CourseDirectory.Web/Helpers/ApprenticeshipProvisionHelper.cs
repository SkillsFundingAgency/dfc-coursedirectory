﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Interfaces.Apprenticeships;
using Dfc.CourseDirectory.Models.Models.Apprenticeships;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Models.Models.Regions;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.ApprenticeshipService;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.ProviderService;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;
using Dfc.CourseDirectory.Services.VenueService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.Web.Helpers
{
    public class ApprenticeshipProvisionHelper : IApprenticeshipProvisionHelper
    {
        private readonly ILogger<ApprenticeshipProvisionHelper> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IApprenticeshipService _apprenticeshipService;
        private readonly IVenueService _venueService;
        private readonly IProviderService _providerService;


        private ICSVHelper _CSVHelper;
        private ISession _session => _contextAccessor.HttpContext.Session;
        public ApprenticeshipProvisionHelper(
            ILogger<ApprenticeshipProvisionHelper> logger,
                IHttpContextAccessor contextAccessor,
                IApprenticeshipService apprenticeshipService,
                IVenueService venueService,
                IProviderService providerService,
                ICSVHelper CSVHelper)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(contextAccessor, nameof(contextAccessor));
            Throw.IfNull(apprenticeshipService, nameof(apprenticeshipService));
            Throw.IfNull(venueService, nameof(venueService));
            Throw.IfNull(providerService, nameof(providerService));

            _logger = logger;
            _contextAccessor = contextAccessor;
            _apprenticeshipService = apprenticeshipService;
            _venueService = venueService;
            _providerService = providerService;
            _CSVHelper = CSVHelper;
        }
        public FileStreamResult DownloadCurrentApprenticeshipProvisions()
        {
            int? UKPRN;
            IProviderSearchResult providerSearchResult = null;
            string providerName = String.Empty;
            if (_session.GetInt32("UKPRN").HasValue)
            {

                UKPRN = _session.GetInt32("UKPRN").Value;
                providerSearchResult = _providerService.GetProviderByPRNAsync(new Services.ProviderService.ProviderSearchCriteria(UKPRN.Value.ToString())).Result.Value;
                providerName = providerSearchResult.Value.FirstOrDefault()?.ProviderName.Replace(" ", "");
            }
            else
            {
                return null;
            }

            var apprenticeships = _apprenticeshipService.GetApprenticeshipByUKPRN(UKPRN.ToString())
                                      .Result
                                      .Value
                                      .Where((y => (int)y.RecordStatus == (int)RecordStatus.Live));

            var csvApprenticeships = ApprenticeshipsToCsvApprenticeships(apprenticeships);

            return CsvApprenticeshipsToFileStream(csvApprenticeships, providerName);
        }

        internal IEnumerable<CsvApprenticeship> ApprenticeshipsToCsvApprenticeships(IEnumerable<IApprenticeship> apprenticeships)
        {
            List<CsvApprenticeship> csvApprenticeships = new List<CsvApprenticeship>();

            foreach (var apprenticeship in apprenticeships)
            {
                foreach (var apprenticeshipLocation in apprenticeship.ApprenticeshipLocations)
                {
                    //Sanitise regions
                    if (apprenticeshipLocation.Regions != null)
                        apprenticeshipLocation.Regions = _CSVHelper.SanitiseRegionTextForCSVOutput(apprenticeshipLocation.Regions);
                    var csvApprenticeshipLocation = MapCsvApprenticeship(apprenticeship, apprenticeshipLocation);

                    csvApprenticeships.Add(csvApprenticeshipLocation);
                }
            }
            return csvApprenticeships;
        }

        internal CsvApprenticeship MapCsvApprenticeship(IApprenticeship apprenticeship, ApprenticeshipLocation location)
        {
            SelectRegionModel selectRegionModel = new SelectRegionModel();

            return new CsvApprenticeship
            {
                StandardCode = apprenticeship.StandardCode?.ToString(),
                Version = apprenticeship.Version?.ToString(),
                FrameworkCode = apprenticeship.FrameworkCode?.ToString(),
                ProgType =  apprenticeship.ProgType?.ToString(),
                PathwayCode = apprenticeship.PathwayCode?.ToString(),
                ApprenticeshipInformation = apprenticeship.ApprenticeshipTitle,
                ApprenticeshipWebpage = apprenticeship.Url,
                ContactEmail = apprenticeship.ContactEmail,
                ContactPhone = apprenticeship.ContactTelephone,
                ContactURL = apprenticeship.ContactWebsite,
                DeliveryMethod = DeliveryMethodConvert(location.ApprenticeshipLocationType),
                Venue = location.VenueId.HasValue ? _venueService.GetVenueByIdAsync(new GetVenueByIdCriteria
                    (location.VenueId.Value.ToString())).Result.Value?.VenueName : String.Empty,
                Radius = location.Radius?.ToString(),
                DeliveryMode = DeliveryModeConvert(location.DeliveryModes),
                AcrossEngland = location.ApprenticeshipLocationType == ApprenticeshipLocationType.ClassroomBasedAndEmployerBased ?
                    BoolConvert(location.National) : 
                    String.Empty,
                NationalDelivery = BoolConvert(location.National),
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
        internal FileStreamResult CsvApprenticeshipsToFileStream(IEnumerable<CsvApprenticeship> csvApprenticeships, string providerName)
        {
            List<string> csvLines = new List<string>();
            foreach (var line in _CSVHelper.ToCsv(csvApprenticeships))
            {
                csvLines.Add(line);
            }
            string report = string.Join(Environment.NewLine, csvLines);
            byte[] data = Encoding.ASCII.GetBytes(report);
            MemoryStream ms = new MemoryStream(data)
            {
                Position = 0
            };
            FileStreamResult result = new FileStreamResult(ms, MediaTypeNames.Text.Plain);
            DateTime d = DateTime.Now;
            result.FileDownloadName = $"{providerName}_Apprenticeships_{d.Day.TwoChars()}_{d.Month.TwoChars()}_{d.Year}_{d.Hour.TwoChars()}_{d.Minute.TwoChars()}.csv";
            return result;
        }

        internal string BoolConvert(bool? input)
        {
            switch (input)
            {
                case true:
                    return "Yes";
                case false:
                    return "No";
                default:
                    return String.Empty;
            }
        }

        internal string DeliveryMethodConvert(ApprenticeshipLocationType input)
        {
            switch (input)
            {
                case ApprenticeshipLocationType.ClassroomBased:
                    return "Classroom";
                case ApprenticeshipLocationType.ClassroomBasedAndEmployerBased:
                    return "Both";
                case ApprenticeshipLocationType.EmployerBased:
                    return "Employer";
                default:
                    return string.Empty;
            }
        }

        internal string DeliveryModeConvert(List<int> modes)
        {
            List<string> modeNames = new List<string>();
            foreach (var mode in modes)
            {
                switch (mode)
                {
                    case (int)ApprenticeShipDeliveryLocation.DayRelease:
                        modeNames.Add("Day");
                        break;
                    case (int)ApprenticeShipDeliveryLocation.BlockRelease:
                        modeNames.Add("Block");
                        break;
                    case (int)ApprenticeShipDeliveryLocation.EmployerAddress:
                        modeNames.Add("Employer");
                        break;
                }
            }

            return string.Join(";", modeNames);
        }
    }
}
