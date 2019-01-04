
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.ViewModels;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.CourseFor;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.EntryRequirements;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.HowAssessed;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.HowYouWillLearn;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.WhatWillLearn;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.WhatYouNeed;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.WhereNext;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace Dfc.CourseDirectory.Web.Controllers
{
    public class CoursesController : Controller
    {
        private readonly ILogger<CoursesController> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ICourseService _courseService;
        private ISession _session => _contextAccessor.HttpContext.Session;

        public CoursesController(
            ILogger<CoursesController> logger,
            IOptions<CourseServiceSettings> courseSearchSettings,
            IHttpContextAccessor contextAccessor,
            ICourseService courseService)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(courseSearchSettings, nameof(courseSearchSettings));
            Throw.IfNull(contextAccessor, nameof(contextAccessor));
            Throw.IfNull(courseService, nameof(courseService));

            _logger = logger;
            _contextAccessor = contextAccessor;
            _courseService = courseService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AddCourseSection1(string learnAimRef, string notionalNVQLevelv2, string awardOrgCode, string learnAimRefTitle)
        {
            _session.SetString("LearnAimRef", learnAimRef);
            _session.SetString("NotionalNVQLevelv2", notionalNVQLevelv2);
            _session.SetString("AwardOrgCode", awardOrgCode);
            _session.SetString("LearnAimRefTitle", learnAimRefTitle);

            AddCourseViewModel vm = new AddCourseViewModel
            {
                AwardOrgCode = awardOrgCode,
                LearnAimRef = learnAimRef,
                LearnAimRefTitle = learnAimRefTitle,
                NotionalNVQLevelv2 = notionalNVQLevelv2,
                CourseFor = new CourseForModel()
                {
                    LabelText = "Who is the course for?",
                    HintText = "Please provide useful information that helps a learner to make a decision about the suitability of this course. For example learners new to the subject / sector or those with some experience? Any age restrictions?",
                    AriaDescribedBy = "Please enter who this course is for."
                },
                EntryRequirements = new EntryRequirementsModel()
                {
                    LabelText = "Entry requirements",
                    HintText = "Please provide details of specific academic or vocational entry qualification requirements. Also do learners need specific skills, attributes or evidence? e.g. DBS clearance, driving licence",
                    AriaDescribedBy = "Please list entry requirements."
                },
                WhatWillLearn = new WhatWillLearnModel()
                {
                    LabelText = "What you’ll learn",
                    HintText = "Give learners a taste of this course. What are the main topics covered?",
                    AriaDescribedBy = "Please enter what will be learned"
                },
                HowYouWillLearn = new HowYouWillLearnModel()
                {
                    LabelText = "How you’ll learn",
                    HintText = "Will it be classroom based exercises, practical on the job, practical but in a simulated work environment, online or a mixture of methods?",
                    AriaDescribedBy = "Please enter how you’ll learn"
                },
                WhatYouNeed = new WhatYouNeedModel()
                {
                    LabelText = "What you’ll need to bring",
                    HintText = "Please detail anything your learners will need to provide or pay for themselves such as uniform, personal protective clothing, tools or kit",
                    AriaDescribedBy = "Please enter what you need"
                },
                HowAssessed = new HowAssessedModel()
                {
                    LabelText = "How you’ll be assessed",
                    HintText = "Please provide details of all the ways your learners will be assessed for this course. E.g. assessment in the workplace, written assignments, group or individual project work, exam, portfolio of evidence, multiple choice tests.",
                    AriaDescribedBy = "Please enter 'How you’ll be assessed'"
                },
                WhereNext = new WhereNextModel()
                {
                    LabelText = "Where next?",
                    HintText = "What are the opportunities beyond this course? Progression to a higher level course, apprenticeship or direct entry to employment?",
                    AriaDescribedBy = "Please enter 'Where next?'"
                }
            };
            return View(vm);
        }

        [HttpPost]
        public IActionResult AddCourseSection1(AddCourseSection1RequestModel model)
        {
            _session.SetString("CourseFor", model?.CourseFor);
            _session.SetString("EntryRequirements", model?.EntryRequirements ?? string.Empty);
            _session.SetString("WhatWillLearn", model?.WhatWillLearn ?? string.Empty);
            _session.SetString("HowYouWillLearn", model?.HowYouWillLearn ?? string.Empty);
            _session.SetString("WhatYouNeed", model?.WhatYouNeed ?? string.Empty);
            _session.SetString("HowAssessed", model?.HowAssessed ?? string.Empty);
            _session.SetString("WhereNext", model?.WhereNext ?? string.Empty);

            var viewModel = new AddCourseDetailsViewModel()
            {
                LearnAimRef = _session.GetString("LearnAimRef"),
                LearnAimRefTitle = _session.GetString("LearnAimRefTitle"),
                AwardOrgCode = _session.GetString("AwardOrgCode"),
                NotionalNVQLevelv2 = _session.GetString("NotionalNVQLevelv2"),
                CourseName = _session.GetString("LearnAimRefTitle")
            };
            return View("AddCourseSection2", viewModel);
        }

        public IActionResult AddCourseSection2(AddCourseRequestModel requestModel)
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddCourse(AddCoursePublishModel model)
        {
            var learnAimRef = _session.GetString("LearnAimRef");
            var notionalNVQLevelv2 = _session.GetString("NotionalNVQLevelv2");
            var awardOrgCode = _session.GetString("AwardOrgCode");
            var learnAimRefTitle = _session.GetString("LearnAimRefTitle");

            var courseFor = _session.GetString("CourseFor");
            var entryRequirements = _session.GetString("EntryRequirements");
            var whatWillLearn = _session.GetString("WhatWillLearn");
            var howYouWillLearn = _session.GetString("HowYouWillLearn");
            var whatYouNeed = _session.GetString("WhatYouNeed");
            var howAssessed = _session.GetString("HowAssessed");
            var whereNext = _session.GetString("WhereNext");


            // TODO - Add error message, if use this check
            if (string.IsNullOrEmpty(learnAimRef) ||
                string.IsNullOrEmpty(notionalNVQLevelv2) ||
                string.IsNullOrEmpty(awardOrgCode) ||
                string.IsNullOrEmpty(learnAimRefTitle) ||
                string.IsNullOrEmpty(courseFor)
              )
            {
                return RedirectToAction("AddCourseSection1", new { learnAimRef = learnAimRef, notionalNVQLevelv2 = notionalNVQLevelv2, awardOrgCode = awardOrgCode, learnAimRefTitle = learnAimRefTitle });
            }

            // We will need to map the flat ModelView Structure to our hierarchical Course Model Structure

            // For each Venue => Course Run
            var courseRuns = new List<CourseRun>();
            // It will come from Venue selection
            var venueSelection = new Guid[] { new Guid("86748623-93f0-4ebd-8a37-0f0754822b7e"), new Guid("96748623-93f0-4ebd-8a37-0f0754822b7e") };

            bool flexibleStartDate = false;
            DateTime specifiedStartDate = DateTime.MinValue;
            if (model.StartDateType.Equals("SpecifiedStartDate", StringComparison.InvariantCultureIgnoreCase))
            {
                string day = model.Day.Length == 1 ? string.Concat("0", model.Day) : model.Day;
                string month = model.Month.Length == 1 ? string.Concat("0", model.Month) : model.Month;
                string startDate = string.Format("{0}-{1}-{2}", day, month, model.Year);
                specifiedStartDate = DateTime.ParseExact(startDate, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture);
            }
            else if (model.StartDateType.Equals("FlexibleStartDate", StringComparison.InvariantCultureIgnoreCase))
            {
                flexibleStartDate = true;
            }
            else
            {
                // StartDateType not defined - log it.
            }

            foreach (var venue in venueSelection)
            {
                var courseRun = new CourseRun
                {
                    id = Guid.NewGuid(),
                    VenueId = venue,

                    CourseName = model.CourseName,
                    ProviderCourseID = model.CourseProviderReference,
                    DeliveryMode = model.DeliveryMode,
                    FlexibleStartDate = flexibleStartDate,
                    StartDate = specifiedStartDate,
                    CourseURL = model.Url,
                    Cost = model.Cost,
                    CostDescription = model.CostDescription,
                    DurationUnit = model.Id,
                    DurationValue = model.DurationLength,
                    StudyMode = model.StudyMode,
                    AttendancePattern = model.AttendanceMode,

                    CreatedDate = DateTime.Now,
                    CreatedBy = "ProviderPortal-AddCourse" // TODO - Change to the name of the logged person 
                };

                courseRuns.Add(courseRun);
            }

            // TODO: To be modified once we implement user management (Assign ProviderUKPRN to user)
            int UKPRN = 0;
            if (_session.GetInt32("UKPRN") != null)
            {
                UKPRN = _session.GetInt32("UKPRN").Value;
            }
            else
            {
                return RedirectToAction("Index", "Venues", new { errmsg = "No-UKPRN" });
            }

            var course = new Course
            {
                id = Guid.NewGuid(),

                QualificationCourseTitle = learnAimRefTitle,
                LearnAimRef = learnAimRef,
                NotionalNVQLevelv2 = notionalNVQLevelv2,
                AwardOrgCode = awardOrgCode,
                QualificationType = "Diploma", // ??? QualificationTypes => Diploma, Cerificate or EACH courserun

                ProviderUKPRN = UKPRN, // TODO: ToBeChanged

                CourseDescription = courseFor,
                EntryRequirments = entryRequirements,
                WhatYoullLearn = whatWillLearn,
                HowYoullLearn = howYouWillLearn,
                WhatYoullNeed = whatYouNeed,
                HowYoullBeAssessed = howAssessed,
                WhereNext = whereNext,
                AdvancedLearnerLoan = model.AdvancedLearnerLoan,

                CourseRuns = courseRuns
            };

            var result = await _courseService.AddCourseAsync(course);

            RemoveSessionVariables();

            if (result.IsSuccess && result.HasValue)
            {
                return RedirectToAction("Index", new { status = "good", learnAimRef = learnAimRef, numberOfNewCourses = courseRuns?.Count });
            }
            else
            {
                return RedirectToAction("Index", new { status = "bad", learnAimRef = learnAimRef, errmsg = result.Error });
            }
        }

        internal void RemoveSessionVariables()
        {
            _session.Remove("LearnAimRef");
            _session.Remove("NotionalNVQLevelv2");
            _session.Remove("AwardOrgCode");
            _session.Remove("LearnAimRefTitle");

            _session.Remove("CourseFor");
            _session.Remove("EntryRequirements");
            _session.Remove("WhatWillLearn");
            _session.Remove("HowYouWillLearn");
            _session.Remove("WhatYouNeed");
            _session.Remove("HowAssessed");
            _session.Remove("WhereNext");
        }
    }
}