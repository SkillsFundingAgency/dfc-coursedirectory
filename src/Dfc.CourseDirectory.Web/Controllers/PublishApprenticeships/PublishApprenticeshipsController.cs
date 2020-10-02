using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Interfaces.Apprenticeships;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Services.Interfaces.ApprenticeshipService;
using Dfc.CourseDirectory.Web.ViewModels.BulkUpload;
using Dfc.CourseDirectory.Web.ViewModels.PublishApprenticeships;
using Dfc.CourseDirectory.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.Controllers.PublishApprenticeships
{
    [RestrictApprenticeshipQAStatus(ApprenticeshipQAStatus.Passed)]
    public class PublishApprenticeshipsController : Controller
    {
        private readonly ILogger<PublishApprenticeshipsController> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        private ISession _session => _contextAccessor.HttpContext.Session;
        private readonly IApprenticeshipService _apprenticeshipService;

        public PublishApprenticeshipsController(ILogger<PublishApprenticeshipsController> logger,
               IHttpContextAccessor contextAccessor, IApprenticeshipService apprenticeshipService)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(apprenticeshipService, nameof(apprenticeshipService));
            _logger = logger;
            _contextAccessor = contextAccessor;
            _apprenticeshipService = apprenticeshipService;
        }

        [Authorize]
        [HttpGet]
        public IActionResult Index()
        {
            PublishApprenticeshipsViewModel vm = new PublishApprenticeshipsViewModel();
            int? UKPRN = _session.GetInt32("UKPRN");
            if (!UKPRN.HasValue)
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            var apprenticeships = _apprenticeshipService.GetApprenticeshipByUKPRN(UKPRN.Value.ToString()).Result.Value.Where(x => x.RecordStatus == RecordStatus.BulkUploadPending);

            var getApprenticeships = _apprenticeshipService.GetApprenticeshipByUKPRN(UKPRN.Value.ToString()).Result.Value.Where(x => x.ProviderUKPRN == UKPRN);

            vm.ListOfApprenticeships = apprenticeships;
            vm.ListOfApprenticeships = GetErrorMessages(vm.ListOfApprenticeships);

            if (!getApprenticeships.Any(x => x.RecordStatus == RecordStatus.BulkUploadPending))
            {
                vm.AreAllReadyToBePublished = true;
            }

            if (vm.AreAllReadyToBePublished)
            {
                return RedirectToAction("PublishYourFile", "BulkUploadApprenticeships", new { NumberOfApprenticeships = apprenticeships.SelectMany(s => s.ApprenticeshipLocations.Where(cr => cr.RecordStatus == RecordStatus.BulkUploadReadyToGoLive)).Count() });
            }
            return View("Index", vm);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Index(PublishApprenticeshipsViewModel vm)
        {
            PublishCompleteViewModel CompleteVM = new PublishCompleteViewModel();

            int? sUKPRN = _session.GetInt32("UKPRN");
            int UKPRN;
            if (!sUKPRN.HasValue)
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            else
                UKPRN = sUKPRN ?? 0;

            CompleteVM.NumberOfCoursesPublished = vm.NumberOfApprenticeships;
            await _apprenticeshipService.ChangeApprenticeshipStatusesForUKPRNSelection(UKPRN, (int)RecordStatus.MigrationPending, (int)RecordStatus.Archived);
            await _apprenticeshipService.ChangeApprenticeshipStatusesForUKPRNSelection(UKPRN, (int)RecordStatus.MigrationReadyToGoLive, (int)RecordStatus.Archived);

            //Archive any existing courses
            var resultArchivingCourses = await _apprenticeshipService.ChangeApprenticeshipStatusesForUKPRNSelection(UKPRN, (int)RecordStatus.Live, (int)RecordStatus.Archived);
            if (resultArchivingCourses.IsSuccess)
            {
                // Publish courses
                var resultPublishBulkUploadedCourses = await _apprenticeshipService.ChangeApprenticeshipStatusesForUKPRNSelection(UKPRN, (int)RecordStatus.BulkUploadReadyToGoLive, (int)RecordStatus.Live);
                CompleteVM.Mode = PublishMode.BulkUpload;
                if (resultPublishBulkUploadedCourses.IsSuccess)
                    return RedirectToAction("Complete", "Apprenticeships", new { CompleteVM });
                else
                    return RedirectToAction("Index", "Home", new { errmsg = "Publish All BulkUpload-PublishCourses Error" });
            }
            else
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Publish All BulkUpload-ArchiveCourses Error" });
            }
        }

        internal IEnumerable<IApprenticeship> GetErrorMessages(IEnumerable<IApprenticeship> apprenticeships)
        {
            foreach (var apprentice in apprenticeships)
            {
                apprentice.ValidationErrors = ValidateApprenticeships(apprentice).Select(x => x.Value);

                apprentice.LocationValidationErrors = ValidateApprenticeshipLocations(apprentice).Select(x => x.Value);

                if (apprentice.BulkUploadErrors.Any() && !apprentice.ValidationErrors.Any())
                {
                    apprentice.BulkUploadErrors = new List<BulkUploadError> { };
                }               

                _apprenticeshipService.UpdateApprenticeshipAsync(apprentice);
            }


            return apprenticeships;
        }




        public IList<KeyValuePair<string, string>> ValidateApprenticeships(IApprenticeship apprenticeship)
        {
            List<KeyValuePair<string, string>> validationMessages = new List<KeyValuePair<string, string>>();

            // APPRENTICESHIP_INFORMATION
            if (string.IsNullOrEmpty(apprenticeship.MarketingInformation))
            {
                validationMessages.Add(new KeyValuePair<string, string>("APPRENTICESHIP_INFORMATION", "APPRENTICESHIP_INFORMATION is required"));
            }
            else
            {
                if (!HasOnlyFollowingValidCharacters(apprenticeship.MarketingInformation))
                    validationMessages.Add(new KeyValuePair<string, string>("APPRENTICESHIP_INFORMATION", "APPRENTICESHIP_INFORMATIONR contains invalid character"));
                if (apprenticeship.MarketingInformation.Length > 750)
                    validationMessages.Add(new KeyValuePair<string, string>("APPRENTICESHIP_INFORMATIONR", $"APPRENTICESHIP_INFORMATIONR must be 750 characters or less"));
            }

            //WebSite
            if (!string.IsNullOrEmpty(apprenticeship.ContactWebsite))
            {
                if (!IsValidWebSite(apprenticeship.ContactWebsite))
                {
                    validationMessages.Add(new KeyValuePair<string, string>("WebSite", "Enter a real web page, like http://www.provider.com/apprenticeship"));
                    if (apprenticeship.ContactWebsite.Length > 255)
                        validationMessages.Add(new KeyValuePair<string, string>("WebSite", $"WebSite must be 255 characters or less"));
                }
            }

            //Email
            if (string.IsNullOrEmpty(apprenticeship.ContactEmail))
            {
                validationMessages.Add(new KeyValuePair<string, string>("Email", "Email is required"));
            }
            else
            {
                if (!IsValidEmail(apprenticeship.ContactEmail))
                    validationMessages.Add(new KeyValuePair<string, string>("Email", "Enter a valid email"));
                if (apprenticeship.ContactEmail.Length > 255)
                    validationMessages.Add(new KeyValuePair<string, string>("Email", $"Email must be 255 characters or less"));
            }
            //Telephone
            if (string.IsNullOrEmpty(apprenticeship.ContactTelephone))
            {
                validationMessages.Add(new KeyValuePair<string, string>("Telephone", "Telephone is required"));
            }
            else
            {
                if (!IsValidTelephone(apprenticeship.ContactTelephone))
                    validationMessages.Add(new KeyValuePair<string, string>("Telephone", "Enter a valid Telephone"));
                if (apprenticeship.ContactTelephone.Length > 30)
                    validationMessages.Add(new KeyValuePair<string, string>("Telephone", $"Telephone should be no more than 30 characters"));
                if (apprenticeship.ContactTelephone.Length < 11)
                    validationMessages.Add(new KeyValuePair<string, string>("Telephone", $"Telephone should not be less than 11 characters"));
            }
            //contactUsPage
            if (!string.IsNullOrEmpty(apprenticeship.Url))
            {
                if (!IsValidWebSite(apprenticeship.Url))
                {
                    validationMessages.Add(new KeyValuePair<string, string>("Contact us page ", "Enter a real web page, like http://www.provider.com/apprenticeship"));
                    if (apprenticeship.Url.Length > 255)
                        validationMessages.Add(new KeyValuePair<string, string>("Contact us page ", $"Contact us page  must be 255 characters or less"));
                }
            }
            return validationMessages;
        }

        public IList<KeyValuePair<string, string>> ValidateApprenticeshipLocations(IApprenticeship apprenticeship)
        {
            List<KeyValuePair<string, string>> validationMessages = new List<KeyValuePair<string, string>>();

            var apprenticeshipsLocations = apprenticeship.ApprenticeshipLocations.Select(x => x.LocationId);
            foreach (var location in apprenticeshipsLocations)
            {
                // APPRENTICESHIP_Locations
                if (location == null)
                {
                    validationMessages.Add(new KeyValuePair<string, string>("APPRENTICESHIP_Location", "APPRENTICESHIP_Location is required"));
                }
            }
            return validationMessages;
        }

        public bool HasOnlyFollowingValidCharacters(string value)
        {
            string regex = @"^[a-zA-Z0-9 /\n/\r/\\u/\¬\!\£\$\%\^\&\*\\é\\è\\ﬁ\(\)_\+\-\=\{\}\[\]\;\:\@\'\#\~\,\<\>\.\?\/\|\`\•\·\●\\’\‘\“\”\—\-\–\‐\‐\…\:/\°\®\\â\\ç\\ñ\\ü\\ø\♦\™\\t/\s\¼\¾\½\" + "\"" + "\\\\]+$";
            var validUKPRN = Regex.Match(value, regex, RegexOptions.IgnoreCase);

            return validUKPRN.Success;
        }

        public bool IsValidUrl(string value)
        {
            string regex = @"^(http:\/\/www\.|https:\/\/www\.|http:\/\/|https:\/\/)?[a-z0-9]+([\-\.]{1}[a-z0-9]+)*\.[a-z]{2,5}(:[0-9]{1,5})?(\/.*)?$";
            var validUKPRN = Regex.Match(value, regex, RegexOptions.IgnoreCase);

            return validUKPRN.Success;
        }

        public bool IsCorrectCostFormatting(string value)
        {
            string regex = @"^[0-9]*(\.[0-9]{1,2})?$";
            var validUKPRN = Regex.Match(value, regex, RegexOptions.IgnoreCase);

            return validUKPRN.Success;
        }

        public bool ValidDurationValue(string value)
        {
            string regex = @"^([0-9]|[0-9][0-9]|[0-9][0-9][0-9])$";
            var validUKPRN = Regex.Match(value, regex, RegexOptions.IgnoreCase);

            return validUKPRN.Success;
        }

        public bool IsValidWebSite(string value)
        {
            string regex = @"^(?:http(s)?:\/\/)?[\w.-]+(?:\.[\w\.-]+)+[\w\-\._~:/?#[\]@!\$&'\(\)\*\+,;=.]+$";
            var validWebSite = Regex.Match(value, regex, RegexOptions.IgnoreCase);

            return validWebSite.Success;
        }

        public bool IsValidEmail(string value)
        {
            string regex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
            var validEmail = Regex.Match(value, regex, RegexOptions.IgnoreCase);

            return validEmail.Success;
        }

        public bool IsValidTelephone(string value)
        {
            string regex = @"^(((\+44)? ?(\(0\))? ?)|(0))( ?[0-9]{3,4}){3}?$";
            var validTelephone = Regex.Match(value, regex, RegexOptions.IgnoreCase);

            return validTelephone.Success;
        }
    }
}