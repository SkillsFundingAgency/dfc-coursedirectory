using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Services.Models;
using Dfc.CourseDirectory.Services.Models.Courses;
using Dfc.CourseDirectory.Web.Models.Apprenticeships;
using Dfc.CourseDirectory.Web.ViewModels.BulkUpload;
using Dfc.CourseDirectory.Web.ViewModels.PublishApprenticeships;
using Dfc.CourseDirectory.WebV2.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.Controllers.PublishApprenticeships
{
    [RestrictApprenticeshipQAStatus(ApprenticeshipQAStatus.Passed)]
    public class PublishApprenticeshipsController : Controller
    {
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;

        private ISession Session => HttpContext.Session;

        public PublishApprenticeshipsController(ICosmosDbQueryDispatcher cosmosDbQueryDispatcher)
        {
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int? UKPRN = Session.GetInt32("UKPRN");

            if (!UKPRN.HasValue)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            PublishApprenticeshipsViewModel vm = new PublishApprenticeshipsViewModel();

            var apprenticeships = await _cosmosDbQueryDispatcher.ExecuteQuery(new GetApprenticeships
            {
                Predicate = a =>
                    a.ProviderUKPRN == UKPRN
                    && (a.RecordStatus == (int)ApprenticeshipStatus.BulkUploadPending || a.RecordStatus == (int)ApprenticeshipStatus.BulkUploadReadyToGoLive)
            });

            var bulkUploadPendingApprenticeships = apprenticeships.Values.Where(a => a.RecordStatus == (int)ApprenticeshipStatus.BulkUploadPending).Select(Apprenticeship.FromCosmosDbModel).ToArray();
            var bulkUploadReadyToGoLiveApprenticeships = apprenticeships.Values.Where(a => a.RecordStatus == (int)ApprenticeshipStatus.BulkUploadReadyToGoLive).Select(Apprenticeship.FromCosmosDbModel).ToArray();

            vm.ListOfApprenticeships = await GetErrorMessages(bulkUploadPendingApprenticeships);

            if (!bulkUploadPendingApprenticeships.Any())
            {
                return RedirectToAction("PublishYourFile", "BulkUploadApprenticeships", new
                {
                    NumberOfApprenticeships = bulkUploadReadyToGoLiveApprenticeships.Sum(a => a.ApprenticeshipLocations.Count(al => al.RecordStatus == ApprenticeshipStatus.BulkUploadReadyToGoLive))
                });
            }

            vm.AreAllReadyToBePublished = true;

            return View("Index", vm);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Index(PublishApprenticeshipsViewModel vm)
        {
            PublishCompleteViewModel CompleteVM = new PublishCompleteViewModel();

            int? sUKPRN = Session.GetInt32("UKPRN");
            int UKPRN;
            if (!sUKPRN.HasValue)
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            else
                UKPRN = sUKPRN ?? 0;

            CompleteVM.NumberOfCoursesPublished = vm.NumberOfApprenticeships;

            await _cosmosDbQueryDispatcher.ExecuteQuery(new UpdateApprenticeshipStatusesByProviderUkprn
            {
                ProviderUkprn = UKPRN,
                CurrentStatus = ApprenticeshipStatus.MigrationPending | ApprenticeshipStatus.MigrationReadyToGoLive,
                NewStatus = ApprenticeshipStatus.Archived
            });

            //Archive any existing courses
            await _cosmosDbQueryDispatcher.ExecuteQuery(new UpdateApprenticeshipStatusesByProviderUkprn
            {
                ProviderUkprn = UKPRN,
                CurrentStatus = ApprenticeshipStatus.Live,
                NewStatus = ApprenticeshipStatus.Archived
            });

            // Publish courses
            await _cosmosDbQueryDispatcher.ExecuteQuery(new UpdateApprenticeshipStatusesByProviderUkprn
            {
                ProviderUkprn = UKPRN,
                CurrentStatus = ApprenticeshipStatus.BulkUploadReadyToGoLive,
                NewStatus = ApprenticeshipStatus.Live
            });

            CompleteVM.Mode = PublishMode.ApprenticeshipBulkUpload;

            return RedirectToAction("Complete", "Apprenticeships", new { CompleteVM });
        }

        private async Task<IEnumerable<Apprenticeship>> GetErrorMessages(IEnumerable<Apprenticeship> apprenticeships)
        {
            foreach (var apprenticeship in apprenticeships)
            {
                apprenticeship.ValidationErrors = ValidateApprenticeships(apprenticeship).Select(x => x.Value);

                apprenticeship.LocationValidationErrors = ValidateApprenticeshipLocations(apprenticeship).Select(x => x.Value);

                if (apprenticeship.BulkUploadErrors.Any() && !apprenticeship.ValidationErrors.Any())
                {
                    apprenticeship.BulkUploadErrors = new List<BulkUploadError>();
                    await _cosmosDbQueryDispatcher.ExecuteQuery(apprenticeship.ToUpdateApprenticeship());
                }
            }

            return apprenticeships;
        }

        public IList<KeyValuePair<string, string>> ValidateApprenticeships(Apprenticeship apprenticeship)
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
                if (apprenticeship.ContactTelephone.Length < 10)
                    validationMessages.Add(new KeyValuePair<string, string>("Telephone", $"Telephone should not be less than 10 characters"));
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

        public IList<KeyValuePair<string, string>> ValidateApprenticeshipLocations(Apprenticeship apprenticeship)
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
