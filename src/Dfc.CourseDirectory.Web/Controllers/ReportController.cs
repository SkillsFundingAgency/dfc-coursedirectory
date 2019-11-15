﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Models.Courses;
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

            var reportResultsTask = _courseService.GetAllDfcReports();
            var totalCoursesLiveTask = _courseService.GetTotalLiveCourses();
            await Task.WhenAll(reportResultsTask, totalCoursesLiveTask);

            var reportResults = reportResultsTask.Result;
            var totalCoursesLive = totalCoursesLiveTask.Result;

            if (reportResults.IsFailure || totalCoursesLive.IsFailure) throw new Exception("Unable to generate migration reports");

            // @ToDo: refactor this business logic away from the presentation layer
            int feProvidersMigrated = reportResults.Value.Count;
            int feCoursesMigrated = (reportResults.Value.Sum(r => r.MigratedCount) ?? 0);
            int feCoursesMigratedWithErrors = reportResults.Value.Sum(r => r.MigrationPendingCount);

            MigrationReportViewModel model = new MigrationReportViewModel()
            {
                ProvidersMigrated = new MigrationReportDashboardPanelModel("FE providers migrated", value: feProvidersMigrated),
                 Migrated = new MigrationReportDashboardPanelModel("FE courses migrated", value: feCoursesMigrated),
                MigratedWithErrors = new MigrationReportDashboardPanelModel("FE courses with errors", value: feCoursesMigratedWithErrors),
                Live = new MigrationReportDashboardPanelModel("Total courses live", value: totalCoursesLive.Value),

                ReportResults = new MigrationReportResultsModel(reportResults.Value)
            };
            model.ReportType = ReportType.Fe;
            return View(model);
        }

        public async Task<IActionResult> Apprenticeship()
        {
            var ukPRN = _session.GetInt32("UKPRN");
            if (ukPRN != null)
            {
                _session.Remove("UKPRN");
            }

            var reportResultsTask = _apprenticeshipService.GetAllDfcReports();
            await Task.WhenAll(reportResultsTask);

            var reportResults = reportResultsTask.Result;

            if (reportResults.IsFailure) throw new Exception("Unable to generate migration reports");

            // @ToDo: refactor this business logic away from the presentation layer
            int feProvidersMigrated = reportResults.Value.Count;
            int feCoursesMigrated = (reportResults.Value.Sum(r => r.MigratedCount) ?? 0);
            int feCoursesMigratedWithErrors = reportResults.Value.Sum(r => r.MigrationPendingCount);

            MigrationReportViewModel model = new MigrationReportViewModel()
            {
                ProvidersMigrated = new MigrationReportDashboardPanelModel("Apprenticeship providers migrated", value: feProvidersMigrated),
                Migrated = new MigrationReportDashboardPanelModel("Apprenticeships migrated", value: feCoursesMigrated),
                MigratedWithErrors = new MigrationReportDashboardPanelModel("Apprenticeships with errors", value: feCoursesMigratedWithErrors),
                Live = new MigrationReportDashboardPanelModel("Total Apprenticeships live", 0), //todo,
                ReportResults = new MigrationReportResultsModel(reportResults.Value)
            };
            model.ReportType = ReportType.Apprenticeship;

            return View("Index", model);
        }

        public async Task<IActionResult> ReportCSV(ReportType type)
        {
            var reportResults = await _courseService.GetAllDfcReports();

            if (reportResults.IsFailure) throw new Exception("Unable to generate migration reports");

            var csvReports = new List<CsvDfcMigrationReport>();

            switch (type)
            {
                case ReportType.Apprenticeship:
                    csvReports = reportResults.Value.Select(x => new CsvDfcMigrationReport
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
                        UKPRN = x.ProviderUKPRN
                    }).ToList();
                    break;
                default:
                    csvReports = reportResults.Value.Select(x => new CsvDfcMigrationReport
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
                        UKPRN = x.ProviderUKPRN
                    }).ToList();
                    break;
            }



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
            DateTime d = DateTime.Now;
            result.FileDownloadName = $"Helpdesk_Migration _Report_{d.Day.TwoChars()}_{d.Month.TwoChars()}_{d.Year}_{d.Hour.TwoChars()}_{d.Minute.TwoChars()}.csv";
            return result;

        }
    }
}