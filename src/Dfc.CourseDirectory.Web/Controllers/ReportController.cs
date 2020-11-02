using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Services.Enums;
using Dfc.CourseDirectory.Services.Models.Courses;
using Dfc.CourseDirectory.Services.Interfaces.ApprenticeshipService;
using Dfc.CourseDirectory.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Dfc.CourseDirectory.Web.ViewModels.Report;
using Dfc.CourseDirectory.Web.ViewComponents.MigrationReportResults;
using Dfc.CourseDirectory.Web.ViewComponents.MigrationReportDashboardPanel;
using Microsoft.AspNetCore.Http;

namespace Dfc.CourseDirectory.Web.Controllers
{
    [Authorize("Admin")]
    public class ReportController : Controller
    {
        private readonly ILogger<ReportController> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ICourseService _courseService;
        private readonly IApprenticeshipService _apprenticeshipService;
        private ICSVHelper _CSVHelper;
        private ISession _session => _contextAccessor.HttpContext.Session;

        readonly string appName = "MigrationReport";

        public ReportController(ILogger<ReportController> logger, ICourseService courseService, ICSVHelper csvHelper,
            IHttpContextAccessor contextAccessor, IApprenticeshipService apprenticeshipService)
        {
            _logger = logger;
            _courseService = courseService;
            _CSVHelper = csvHelper;
            _contextAccessor = contextAccessor;
            _apprenticeshipService = apprenticeshipService;
        }

        public async Task<IActionResult> Index()
        {
            var ukPRN = _session.GetInt32("UKPRN");
            if (ukPRN != null)
            {
                _session.Remove("UKPRN");
            }

            string dateLastUpdate;
            MigrationReportViewModel2 model = new MigrationReportViewModel2();

            // Get courses data
            var couresesReportResultsTask = _courseService.GetAllDfcReports();
            var appsReportResultsTask = _apprenticeshipService.GetAllDfcReports();
            await Task.WhenAll(couresesReportResultsTask, appsReportResultsTask);

            var courseReportResults = couresesReportResultsTask.Result;
            var appsReportResults = appsReportResultsTask.Result;

            if (courseReportResults.IsFailure || appsReportResults.IsFailure) throw new Exception("Unable to generate migration reports");

            var courseReportData = courseReportResults.Value.Where(c => c.CreatedBy == appName); // Only show records that have been updated by report migrator
            var appReportData = appsReportResults.Value.Where(c => c.CreatedBy == appName); // Only show records that have been updated by report migrator

            // Get providers in both reports
            var providersInBoth = from c in courseReportData
                                  join a in appReportData
                                         on c.ProviderUKPRN equals a.ProviderUKPRN
                                  select c.ProviderUKPRN;

            // Get providers in  courses reports only
            var providersInCoursesOnly = from c in courseReportData
                                         where !providersInBoth.Contains(c.ProviderUKPRN)
                                         select c.ProviderUKPRN;

            // Get providers in  apps reports only
            var providersInAppsOnly = from a in appReportData
                                      where !providersInBoth.Contains(a.ProviderUKPRN)
                                      select a.ProviderUKPRN;


            int totalProvidersMigrated = providersInBoth.Count()
                                            + providersInCoursesOnly.Count()
                                            + providersInAppsOnly.Count();

            dateLastUpdate = appReportData.First().CreatedOn.ToString("dd/MM/yyyy H:mm");

            // Perform course calculations
            var feCoursesPendingCount = courseReportData.Sum(r => r.MigrationPendingCount);
            var feCoursesLiveCount = courseReportData.Sum(r => r.LiveCount);
            int feCoursesMigratedCount = courseReportData.Sum(r => r.MigratedCount) ?? 0;

            // Perform apps calculations
            int appsPendingCount = appReportData.Sum(r => r.MigrationPendingCount);
            int appsLiveCount = appReportData.Sum(r => r.LiveCount);
            int appsMigratedCount = appReportData.Sum(r => r.MigratedCount) ?? 0;

            model.TotalProvidersMigrated = new MigrationReportDashboardPanelModel("Total providers migrated", value: totalProvidersMigrated);
            model.DateLastUpdated = dateLastUpdate;

            model.CoursesMigrated = new MigrationReportDashboardPanelModel("Migrated", value: feCoursesMigratedCount);
            model.CoursesPending = new MigrationReportDashboardPanelModel("Pending", value: feCoursesPendingCount);
            model.CoursesLive = new MigrationReportDashboardPanelModel("Live", value: feCoursesLiveCount);
            model.CourseReportResults = new MigrationReportResultsModel(new List<DfcMigrationReport>());

            model.ApprenticeshipMigrated = new MigrationReportDashboardPanelModel("Migrated", value: appsMigratedCount);
            model.ApprenticeshipPending = new MigrationReportDashboardPanelModel("Pending", value: appsPendingCount);
            model.ApprenticeshipLive = new MigrationReportDashboardPanelModel("Live", value: appsLiveCount);
            model.ApprenticeshipReportResults = new MigrationReportResultsModel(new List<DfcMigrationReport>());

            return View(model);
        }

        public async Task<IActionResult> ReportCSVCourses()
        {
            var reportResults = await _courseService.GetAllDfcReports();
            if (reportResults.IsFailure) throw new Exception("Unable to generate migration reports");

            var result = GetCSVData(reportResults.Value.ToList());
            DateTime d = DateTime.Now;
            result.FileDownloadName = $"Migration_Report_Courses_{d.ToString("dd-MM-yyyy_hhmmss")}.csv";
            return result;
        }

        public async Task<IActionResult> ReportCSVApps()
        {
            var reportResults = await _apprenticeshipService.GetAllDfcReports();
            if (reportResults.IsFailure) throw new Exception("Unable to generate migration reports");

            var result = GetCSVData(reportResults.Value.ToList());
            DateTime d = DateTime.Now;
            result.FileDownloadName = $"Migration_Report_Apprenticeships_{d.ToString("dd-MM-yyyy_hhmmss")}.csv";
            return result;
        }

        public async Task<IActionResult> ReportCSVProviders()
        {
            // Get courses data
            var couresesReportResultsTask = _courseService.GetAllDfcReports();
            var appsReportResultsTask = _apprenticeshipService.GetAllDfcReports();
            await Task.WhenAll(couresesReportResultsTask, appsReportResultsTask);

            var courseReportResults = couresesReportResultsTask.Result;
            var appsReportResults = appsReportResultsTask.Result;

            if (courseReportResults.IsFailure || appsReportResults.IsFailure) throw new Exception("Unable to generate migration reports");

            var courseReportData = courseReportResults.Value.Where(c => c.CreatedBy == appName); // Only show records that have been updated by report migrator
            var appReportData = appsReportResults.Value.Where(c => c.CreatedBy == appName); // Only show records that have been updated by report migrator

            // Get providers in both reports
            var providersInBoth = from c in courseReportData
                                  join a in appReportData
                                         on c.ProviderUKPRN equals a.ProviderUKPRN
                                  select new CsvProvider()
                                  {
                                      ProviderUKPRN = c.ProviderUKPRN,
                                      ProviderName = c.ProviderName
                                  };

            // Get providers in  courses reports only
            var providersInCoursesOnly = from c in courseReportData
                                         where !providersInBoth.Select(p => p.ProviderUKPRN).Contains(c.ProviderUKPRN)
                                         select new CsvProvider()
                                         {
                                             ProviderUKPRN = c.ProviderUKPRN,
                                             ProviderName = c.ProviderName
                                         };

            // Get providers in  apps reports only
            var providersInAppsOnly = from a in appReportData
                                      where !providersInBoth.Select(p => p.ProviderUKPRN).Contains(a.ProviderUKPRN)
                                      select new CsvProvider()
                                      {
                                          ProviderUKPRN = a.ProviderUKPRN,
                                          ProviderName = a.ProviderName
                                      };

            List<CsvProvider> reportResults = new List<CsvProvider>();
            reportResults.AddRange(providersInBoth);
            reportResults.AddRange(providersInCoursesOnly);
            reportResults.AddRange(providersInAppsOnly);


            var result = GetCSVData(reportResults.Distinct().ToList());
            DateTime d = DateTime.Now;
            result.FileDownloadName = $"Migration_Report_Providers_{d.ToString("dd-MM-yyyy_hhmmss")}.csv";
            return result;

        }

        private FileStreamResult GetCSVData(List<DfcMigrationReport> dfcMigrationReports)
        {
            var csvReports = new List<CsvDfcMigrationReport>();

            csvReports = dfcMigrationReports.Select(x => new CsvDfcMigrationReport
            {
                Errors = x.MigrationPendingCount,
                FailedMigrationCount = x.FailedMigrationCount,
                LiveCount = x.LiveCount,
                MigratedCount = x.MigratedCount,
                MigrationDate = x.MigrationDate.HasValue ? x.MigrationDate.Value.ToString("dd/MM/yyyy") : string.Empty,
                MigrationPendingCount = x.MigrationPendingCount,
                MigrationRate = x.MigrationRate,
                ProviderName = x.ProviderName,
                ProviderType = x.ProviderType,
                UKPRN = x.ProviderUKPRN,
                CreatedBy = x.CreatedBy,
                CreatedOn = x.CreatedOn,
            }).ToList();

            List<string> csvLines = new List<string>();
            foreach (var line in _CSVHelper.ToCsv(csvReports))
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
            return result;
        }

        private FileStreamResult GetCSVData(List<CsvProvider> prvoiderUkprn)
        {
            List<string> csvLines = new List<string>();
            foreach (var line in _CSVHelper.ToCsv(prvoiderUkprn, ",", false))
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
            return result;
        }
    }
}