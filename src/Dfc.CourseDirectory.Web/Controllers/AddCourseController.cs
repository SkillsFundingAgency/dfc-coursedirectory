using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Interfaces.Courses;
using Dfc.CourseDirectory.Models.Models;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.CourseTextService;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.CourseTextService;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;
using Dfc.CourseDirectory.Web.Extensions;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.CourseFor;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.EntryRequirements;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.FundingOptions;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.HowAssessed;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.HowYouWillLearn;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.SelectVenue;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.WhatWillLearn;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.WhatYouNeed;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.WhereNext;
using Dfc.CourseDirectory.Web.ViewComponents.Summary.SummaryComponent;
using Dfc.CourseDirectory.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class AddCourseController : Controller
    {
        private readonly ILogger<CoursesController> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ICourseService _courseService;
        private ISession Session => _contextAccessor.HttpContext.Session;
        private readonly IVenueSearchHelper _venueSearchHelper;
        private readonly IVenueService _venueService;
        private readonly ICourseTextService _courseTextService;

        private const string SessionVenues = "Venues";
        private const string SessionRegions = "Regions";
        private const string SessionAddCourseSection1 = "AddCourseSection1";
        private const string SessionAddCourseSection2 = "AddCourseSection2";
        private const string SessionLastAddCoursePage = "LastAddCoursePage";
        private const string SessionSummaryPageLoadedAtLeastOnce = "SummaryLoadedAtLeastOnce";


        public AddCourseController(
            ILogger<CoursesController> logger,
            IOptions<CourseServiceSettings> courseSearchSettings,
            IHttpContextAccessor contextAccessor,
            ICourseService courseService, IVenueSearchHelper venueSearchHelper, IVenueService venueService, ICourseTextService courseTextService)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(courseSearchSettings, nameof(courseSearchSettings));
            Throw.IfNull(contextAccessor, nameof(contextAccessor));
            Throw.IfNull(courseService, nameof(courseService));
            Throw.IfNull(venueService, nameof(venueService));
            Throw.IfNull(courseTextService, nameof(courseTextService));

            _logger = logger;
            _contextAccessor = contextAccessor;
            _courseService = courseService;
            _venueService = venueService;
            _venueSearchHelper = venueSearchHelper;
            _courseTextService = courseTextService;
        }

        [Authorize]
        [HttpGet]
        public IActionResult AddCourse(string learnAimRef, string notionalNVQLevelv2, string awardOrgCode, string learnAimRefTitle, string learnAimRefTypeDesc, Guid? courseId)
        {
            Session.SetString("LearnAimRef", learnAimRef);
            Session.SetString("NotionalNVQLevelv2", notionalNVQLevelv2);
            Session.SetString("AwardOrgCode", awardOrgCode);
            Session.SetString("LearnAimRefTitle", learnAimRefTitle);
            Session.SetString("LearnAimRefTypeDesc", learnAimRefTypeDesc);

            ICourse course = null;
            ICourseText defaultCourseText = null;

            if (courseId.HasValue)
            {
                course = _courseService.GetCourseByIdAsync(new GetCourseByIdCriteria(courseId.Value)).Result.Value;
            }
            else
            {
                defaultCourseText = _courseTextService.GetCourseTextByLARS(new CourseTextServiceCriteria(learnAimRef)).Result.Value;
            }

            AddCourseViewModel vm = new AddCourseViewModel
            {
                AwardOrgCode = awardOrgCode,
                LearnAimRef = learnAimRef,
                LearnAimRefTitle = learnAimRefTitle,
                NotionalNVQLevelv2 = notionalNVQLevelv2,
                CourseFor = new CourseForModel
                {
                    LabelText = "Who is the course for?",
                    HintText =
                        "Please provide useful information that helps a learner to make a decision about the suitability of this course. For example learners new to the subject / sector or those with some experience? Any age restrictions?",
                    AriaDescribedBy = "Please enter who this course is for.",
                    CourseFor = course?.CourseDescription ?? defaultCourseText?.CourseDescription
                },

                EntryRequirements = new EntryRequirementsModel
                {
                    LabelText = "Entry requirements",
                    HintText =
                        "Please provide details of specific academic or vocational entry qualification requirements. Also do learners need specific skills, attributes or evidence? e.g. DBS clearance, driving licence",
                    AriaDescribedBy = "Please list entry requirements.",
                    EntryRequirements = course?.EntryRequirements ?? defaultCourseText?.EntryRequirements
                },
                WhatWillLearn = new WhatWillLearnModel()
                {
                    LabelText = "What you’ll learn",
                    HintText = "Give learners a taste of this course. What are the main topics covered?",
                    AriaDescribedBy = "Please enter what will be learned",
                    WhatWillLearn = course?.WhatYoullLearn ?? defaultCourseText?.WhatYoullLearn
                },
                HowYouWillLearn = new HowYouWillLearnModel()
                {
                    LabelText = "How you’ll learn",
                    HintText =
                        "Will it be classroom based exercises, practical on the job, practical but in a simulated work environment, online or a mixture of methods?",
                    AriaDescribedBy = "Please enter how you’ll learn",
                    HowYouWillLearn = course?.HowYoullLearn ?? defaultCourseText?.HowYoullLearn
                },
                WhatYouNeed = new WhatYouNeedModel()
                {
                    LabelText = "What you’ll need to bring",
                    HintText =
                        "Please detail anything your learners will need to provide or pay for themselves such as uniform, personal protective clothing, tools or kit",
                    AriaDescribedBy = "Please enter what you need",
                    WhatYouNeed = course?.WhatYoullNeed ?? defaultCourseText?.WhatYoullNeed
                },
                HowAssessed = new HowAssessedModel()
                {
                    LabelText = "How you’ll be assessed",
                    HintText =
                        "Please provide details of all the ways your learners will be assessed for this course. E.g. assessment in the workplace, written assignments, group or individual project work, exam, portfolio of evidence, multiple choice tests.",
                    AriaDescribedBy = "Please enter 'How you’ll be assessed'",
                    HowAssessed = course?.HowYoullBeAssessed ?? defaultCourseText?.HowYoullBeAssessed
                },
                WhereNext = new WhereNextModel()
                {
                    LabelText = "Where next?",
                    HintText =
                        "What are the opportunities beyond this course? Progression to a higher level course, apprenticeship or direct entry to employment?",
                    AriaDescribedBy = "Please enter 'Where next?'",
                    WhereNext = course?.WhereNext ?? defaultCourseText?.WhereNext
                },
                FundingOptions = new FundingOptionsModel()
                {
                    FundingOptionsLabelText = "Funding options",
                    AdvancedLearnerLoan = false,
                    AdultEducationBudget = false
                }
            };

            Session.SetObject(SessionLastAddCoursePage, AddCoursePage.None);    // not come from another add course page
            Session.SetObject(SessionSummaryPageLoadedAtLeastOnce, false);      // not got to summary page yet

            if (courseId.HasValue)
            {
                vm.CourseId = courseId.Value;
            }

            return View(vm);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddCourse(AddCourseSection1RequestModel model)
        {
            // to AddCourseRun or Summary
            int UKPRN = 0;

            Session.SetObject(SessionLastAddCoursePage, AddCoursePage.AddCourse);
            Session.SetObject(SessionAddCourseSection1, model);

            var addCourseSection2Session = Session.GetObject<AddCourseRequestModel>(SessionAddCourseSection2);

            if (Session.GetInt32("UKPRN") != null)
            {
                UKPRN = Session.GetInt32("UKPRN").Value;
            }
            else
            {
                return RedirectToAction("Index", "Home", new {errmsg = "Please select a Provider."});
            }

            var viewModel = new AddCourseDetailsViewModel
            {
                LearnAimRef = Session.GetString("LearnAimRef"),
                LearnAimRefTitle = Session.GetString("LearnAimRefTitle"),
                AwardOrgCode = Session.GetString("AwardOrgCode"),
                NotionalNVQLevelv2 = Session.GetString("NotionalNVQLevelv2"),
                CourseName = Session.GetString("LearnAimRefTitle"),
                ProviderUKPRN = UKPRN,
                SelectVenue = await GetVenuesByUkprn(UKPRN),
                SelectRegion = GetRegions()
            };

            Session.SetObject(SessionVenues, viewModel.SelectVenue);
            Session.SetObject(SessionRegions, viewModel.SelectRegion);

            if (addCourseSection2Session != null)
            {
                viewModel.CourseName = addCourseSection2Session.CourseName;
                viewModel.CourseProviderReference = addCourseSection2Session.CourseProviderReference;
                viewModel.DeliveryMode = addCourseSection2Session.DeliveryMode;
                viewModel.StartDateType = (StartDateType) Enum.Parse(typeof(StartDateType),
                    addCourseSection2Session.StartDateType);
                viewModel.Day = addCourseSection2Session.Day;
                viewModel.Month = addCourseSection2Session.Month;
                viewModel.Year = addCourseSection2Session.Year;
                viewModel.Url = addCourseSection2Session.Url;
                viewModel.Cost = addCourseSection2Session.Cost == null
                    ? string.Empty
                    : addCourseSection2Session.Cost.ToString();
                viewModel.CostDescription = addCourseSection2Session.CostDescription;
                viewModel.DurationLength = addCourseSection2Session.DurationLength.ToString();

                viewModel.DurationUnit = addCourseSection2Session.DurationUnit;

                viewModel.StudyMode = addCourseSection2Session.StudyMode;
                viewModel.AttendanceMode = addCourseSection2Session.AttendanceMode;
                if (addCourseSection2Session.SelectedVenues != null)
                {
                    foreach (var selectedVenue in addCourseSection2Session.SelectedVenues)
                    {
                        viewModel.SelectVenue.VenueItems.First(x => x.Id == selectedVenue.ToString()).Checked =
                            true;
                    }
                }

                if (addCourseSection2Session.SelectedRegions != null)
                {
                    foreach (var selectedRegion in addCourseSection2Session.SelectedRegions)
                    {
                        viewModel.SelectRegion.RegionItems.First(x => x.Id == selectedRegion.ToString())
                            .Checked = true;
                    }
                }
            }
            else
            {
                viewModel.DurationUnit = DurationUnit.Months;
                viewModel.StudyMode = StudyMode.FullTime;
                viewModel.AttendanceMode = AttendancePattern.Daytime;
                viewModel.DeliveryMode = DeliveryMode.ClassroomBased;
                viewModel.StartDateType = StartDateType.SpecifiedStartDate;
            }

            Session.SetObject(SessionLastAddCoursePage, AddCoursePage.AddCourse);

            return View("AddCourseRun", viewModel);
        }
        
        [Authorize]
        [HttpPost]
        public IActionResult AddCourseRun(AddCourseRequestModel model)
        {
            // AddCourseRun - going to Summary
            Session.SetObject(SessionAddCourseSection2, model);
            var addCourse = Session.GetObject<AddCourseSection1RequestModel>(SessionAddCourseSection1);
            var availableVenues = Session.GetObject<SelectVenueModel>(SessionVenues);
            var availableRegions = Session.GetObject<SelectRegionModel>(SessionRegions);

            var venues = new List<string>();
            var regions = new List<string>();

            var summaryViewModel = new AddCourseSummaryViewModel
            {
                LearnAimRef = addCourse.LearnAimRef,
                NotionalNVQLevelv2 = addCourse.NotionalNVQLevelv2,
                LearnAimRefTitle = addCourse.LearnAimRefTitle,
                WhoIsThisCourseFor = addCourse.CourseFor,
                EntryRequirements = addCourse.EntryRequirements,
                WhatYouWillLearn = addCourse.WhatWillLearn,
                WhereNext = addCourse.WhereNext,
                WhatYouWillNeedToBring = addCourse.WhatYouNeed,
                HowAssessed = addCourse.HowAssessed,
                HowYouWillLearn = addCourse.HowYouWillLearn,
                CourseName = model.CourseName,
                CourseId = model.CourseProviderReference,
                DeliveryMode = model.DeliveryMode.ToDescription(),
                DeliveryModeEnum = model.DeliveryMode,
                Url = model.Url,
                Cost = model.Cost == null
                    ? string.Empty
                    : model.Cost.ToString(),
                CostDescription = model.CostDescription,
                CourseLength = model.DurationLength + " " + model.DurationUnit,
                AttendancePattern = model.StudyMode.ToDescription(),
                AttendanceTime = model.AttendanceMode.ToDescription(),
                StartDate = model.StartDateType == "FlexibleStartDate"
                    ? "Flexible"
                    : model.Day + "/" + model.Month + "/" + model.Year
            };

            switch (model.DeliveryMode)
            {
                case DeliveryMode.ClassroomBased:
                    venues.AddRange(from summaryVenueVenueItem in availableVenues.VenueItems
                        from modelSelectedVenue in model.SelectedVenues
                        where modelSelectedVenue.ToString() == summaryVenueVenueItem.Id
                        select summaryVenueVenueItem.VenueName);

                    summaryViewModel.Venues = venues;
                    break;
                case DeliveryMode.WorkBased:
                    regions.AddRange(from availableRegionsRegionItem in availableRegions.RegionItems
                        from subRegionItemModel in availableRegionsRegionItem.SubRegion
                        from modelSelectedRegion in model.SelectedRegions
                        where modelSelectedRegion == subRegionItemModel.Id
                        select subRegionItemModel.SubRegionName);
                    
                    summaryViewModel.Regions = regions;
                    break;
            }

            // funding options
            var fundingOptions = new List<string>();

            if (addCourse.AdultEducationBudget)
            {
                fundingOptions.Add("Adult education budget");
            }

            if (addCourse.AdvancedLearnerLoan)
            {
                fundingOptions.Add("Advanced learner loan");
            }

            summaryViewModel.FundingOptions = fundingOptions;
            Session.SetObject(SessionLastAddCoursePage, AddCoursePage.AddCourseRun);

            return View("Summary", summaryViewModel);
        }


        // Summary - can go to AddCourse, AddCourseRun or Edit screen
        [Authorize]
        [HttpPost]
        public IActionResult Summary(AddCourseSection1RequestModel model)
        {
            // from AddCourse to Summary

            // save model
            Session.SetObject(SessionAddCourseSection1, model);
            // load session1 / use model

            // load session2
            var addCourseRun = Session.GetObject<AddCourseRequestModel>(SessionAddCourseSection2);
            
            // cream scvm
            var summaryViewModel = new AddCourseSummaryViewModel()
            {
                // page 1
                LearnAimRef = model.LearnAimRef,
                NotionalNVQLevelv2 = model.NotionalNVQLevelv2,
                LearnAimRefTitle = model.LearnAimRefTitle,
                WhoIsThisCourseFor = model.CourseFor,
                EntryRequirements = model.EntryRequirements,
                WhatYouWillLearn = model.WhatWillLearn,
                WhereNext = model.WhereNext,
                WhatYouWillNeedToBring = model.WhatYouNeed,
                HowAssessed = model.HowAssessed,
                HowYouWillLearn = model.HowYouWillLearn,
                
                // page 2 
                CourseName = addCourseRun.CourseName,
                CourseId = addCourseRun.CourseProviderReference,
                DeliveryMode = addCourseRun.DeliveryMode.ToDescription(),
                DeliveryModeEnum = addCourseRun.DeliveryMode,
                Url = addCourseRun.Url,
                Cost = addCourseRun.Cost == null ? string.Empty : addCourseRun.Cost.ToString(),
                CostDescription = addCourseRun.CostDescription,
                CourseLength = addCourseRun.DurationLength + " " + addCourseRun.DurationUnit,
                AttendancePattern = addCourseRun.StudyMode.ToDescription(),
                AttendanceTime = addCourseRun.AttendanceMode.ToDescription(),
                StartDate = addCourseRun.StartDateType == "FlexibleStartDate"
                    ? "Flexible"
                    : addCourseRun.Day + "/" + addCourseRun.Month + "/" + addCourseRun.Year

            };
            
            // venues and regions
            var availableVenues = Session.GetObject<SelectVenueModel>(SessionVenues);
            var availableRegions = Session.GetObject<SelectRegionModel>(SessionRegions);

            var venues = new List<string>();
            var regions = new List<string>();

            switch (addCourseRun.DeliveryMode)
            {
                case DeliveryMode.ClassroomBased:
                    venues.AddRange(from summaryVenueVenueItem in availableVenues.VenueItems
                        from modelSelectedVenue in addCourseRun.SelectedVenues
                        where modelSelectedVenue.ToString() == summaryVenueVenueItem.Id
                        select summaryVenueVenueItem.VenueName);


                    break;
                case DeliveryMode.WorkBased:
                    regions.AddRange(from region in availableRegions.RegionItems
                        from modelSelectedRegion in addCourseRun.SelectedRegions
                        where modelSelectedRegion == region.Id
                        select region.RegionName);
                    break;
            }

            summaryViewModel.Venues = venues;
            summaryViewModel.Regions = regions;

            // funding options
            var fundingOptions = new List<string>();
            
            if (model.AdultEducationBudget)
            {
                fundingOptions.Add("Adult education budget");
            }

            if (model.AdvancedLearnerLoan)
            {
                fundingOptions.Add("Advanced learner loan");
            }
            summaryViewModel.FundingOptions = fundingOptions;

            // set the last page
            Session.SetObject(SessionLastAddCoursePage, AddCoursePage.AddCourse);

            // return view
            return View(summaryViewModel);
        }

        [Authorize]
        [HttpPost]
        public IActionResult AddCourseRunToAddCourse(AddCourseRequestModel model)
        {
            // save model
            Session.SetObject(SessionAddCourseSection2, model);

            // load page 1 from session
            var addCourse = GetSection1ViewModel();

            // set the last page
            Session.SetObject(SessionLastAddCoursePage, AddCoursePage.AddCourseRun);

            return View("AddCourse", addCourse);
        }

        // hitting the accept and publish button on summary page - ends in a save
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AcceptAndPublish()
        {
            var learnAimRef = Session.GetString("LearnAimRef");
            var notionalNvqLevelv2 = Session.GetString("NotionalNVQLevelv2");
            var awardOrgCode = Session.GetString("AwardOrgCode");
            var learnAimRefTitle = Session.GetString("LearnAimRefTitle");
            var learnAimRefTypeDesc = Session.GetString("LearnAimRefTypeDesc");

            var addCourseSection2 = Session.GetObject<AddCourseRequestModel>(SessionAddCourseSection2);

            // TODO - Add error message, if use this check
            if (string.IsNullOrEmpty(learnAimRef) ||
                string.IsNullOrEmpty(notionalNvqLevelv2) ||
                string.IsNullOrEmpty(awardOrgCode) ||
                string.IsNullOrEmpty(learnAimRefTitle) ||
                string.IsNullOrEmpty(learnAimRefTypeDesc)
            )
            {
                return RedirectToAction("AddCourse",
                    new
                    {
                        learnAimRef = learnAimRef,
                        notionalNVQLevelv2 = notionalNvqLevelv2,
                        awardOrgCode = awardOrgCode,
                        learnAimRefTitle = learnAimRefTitle,
                        errmsg = "Course data is missing."
                    });
            }

            var addCourseSection1 = Session.GetObject<AddCourseSection1RequestModel>("AddCourseSection1");
            var courseFor = addCourseSection1.CourseFor;
            var entryRequirements = addCourseSection1.EntryRequirements;
            var whatWillLearn = addCourseSection1.WhatWillLearn;
            var howYouWillLearn = addCourseSection1.HowYouWillLearn;
            var whatYouNeed = addCourseSection1.WhatYouNeed;
            var howAssessed = addCourseSection1.HowAssessed;
            var whereNext = addCourseSection1.WhereNext;
            var advancedLearnerLoan = addCourseSection1.AdvancedLearnerLoan;
            var adultEducationBudget = addCourseSection1.AdultEducationBudget;

            if (addCourseSection2.DeliveryMode == DeliveryMode.ClassroomBased)
            {
                if (addCourseSection2.SelectedVenues == null || addCourseSection2.SelectedVenues.Count() < 1)
                {
                    return RedirectToAction("AddCourse",
                        new
                        {
                            learnAimRef = learnAimRef,
                            notionalNVQLevelv2 = notionalNvqLevelv2,
                            awardOrgCode = awardOrgCode,
                            learnAimRefTitle = learnAimRefTitle,
                            errmsg = "No Venue Selected."
                        });
                }
            }

            // We will need to map the flat ModelView Structure to our hierarchical Course Model Structure
            // For each Venue => Course Run
            var courseRuns = new List<CourseRun>();

            bool flexibleStartDate = false;
            DateTime? specifiedStartDate = null;
            if (addCourseSection2.StartDateType.Equals("SpecifiedStartDate",
                StringComparison.InvariantCultureIgnoreCase))
            {
                string day = addCourseSection2.Day.Length == 1
                    ? string.Concat("0", addCourseSection2.Day)
                    : addCourseSection2.Day;
                string month = addCourseSection2.Month.Length == 1
                    ? string.Concat("0", addCourseSection2.Month)
                    : addCourseSection2.Month;
                string startDate = string.Format("{0}-{1}-{2}", day, month, addCourseSection2.Year);
                specifiedStartDate = DateTime.ParseExact(startDate, "dd-MM-yyyy",
                    System.Globalization.CultureInfo.InvariantCulture);
            }
            else if (addCourseSection2.StartDateType.Equals("FlexibleStartDate",
                StringComparison.InvariantCultureIgnoreCase))
            {
                flexibleStartDate = true;
            }

            if (addCourseSection2.DeliveryMode == DeliveryMode.ClassroomBased
                && addCourseSection2.SelectedVenues != null
                && addCourseSection2.SelectedVenues.Any())
            {
                foreach (var venue in addCourseSection2.SelectedVenues)
                {
                    var courseRun = new CourseRun
                    {
                        id = Guid.NewGuid(),
                        VenueId = venue,

                        CourseName = addCourseSection2.CourseName,
                        ProviderCourseID = addCourseSection2.CourseProviderReference,
                        DeliveryMode = addCourseSection2.DeliveryMode,
                        FlexibleStartDate = flexibleStartDate,
                        StartDate = specifiedStartDate,
                        CourseURL = addCourseSection2.Url?.ToLower(),
                        Cost = addCourseSection2.Cost,
                        CostDescription = addCourseSection2.CostDescription,
                        DurationUnit = addCourseSection2.DurationUnit,
                        DurationValue = addCourseSection2.DurationLength,
                        StudyMode = addCourseSection2.StudyMode,
                        AttendancePattern = addCourseSection2.AttendanceMode,
                        Regions = addCourseSection2.SelectedRegions,
                        CreatedDate = DateTime.Now,
                        CreatedBy = "ProviderPortal-AddCourse", // TODO - Change to the name of the logged person 

                        RecordStatus = RecordStatus.Live // TODO - To Be Decided
                    };

                    courseRuns.Add(courseRun);
                }
            }

            if (addCourseSection2.DeliveryMode == DeliveryMode.WorkBased
                && addCourseSection2.SelectedRegions != null
                && addCourseSection2.SelectedRegions.Any())
            {
                var courseRun = new CourseRun
                {
                    id = Guid.NewGuid(),

                    CourseName = addCourseSection2.CourseName,
                    ProviderCourseID = addCourseSection2.CourseProviderReference,
                    DeliveryMode = addCourseSection2.DeliveryMode,
                    FlexibleStartDate = flexibleStartDate,
                    StartDate = specifiedStartDate,
                    CourseURL = addCourseSection2.Url,
                    Cost = addCourseSection2.Cost,
                    CostDescription = addCourseSection2.CostDescription,
                    DurationUnit = addCourseSection2.DurationUnit,
                    DurationValue = addCourseSection2.DurationLength,
                    StudyMode = StudyMode.Undefined,
                    AttendancePattern = AttendancePattern.Undefined,
                    Regions = addCourseSection2.SelectedRegions,
                    CreatedDate = DateTime.Now,
                    CreatedBy = "ProviderPortal-AddCourse", // TODO - Change to the name of the logged person 
                    RecordStatus = RecordStatus.Live // TODO - To Be Decided
                };

                courseRuns.Add(courseRun);
            }

            if (addCourseSection2.DeliveryMode == DeliveryMode.Online)
            {
                var courseRun = new CourseRun
                {
                    id = Guid.NewGuid(),

                    CourseName = addCourseSection2.CourseName,
                    ProviderCourseID = addCourseSection2.CourseProviderReference,
                    DeliveryMode = addCourseSection2.DeliveryMode,
                    FlexibleStartDate = flexibleStartDate,
                    StartDate = specifiedStartDate,
                    CourseURL = addCourseSection2.Url,
                    Cost = addCourseSection2.Cost,
                    CostDescription = addCourseSection2.CostDescription,
                    DurationUnit = addCourseSection2.DurationUnit,
                    DurationValue = addCourseSection2.DurationLength,
                    StudyMode = StudyMode.Undefined,
                    AttendancePattern = AttendancePattern.Undefined,
                    Regions = GetRegions().RegionItems.Select(x => x.Id),
                    CreatedDate = DateTime.Now,
                    CreatedBy = "ProviderPortal-AddCourse",
                    RecordStatus = RecordStatus.Live // TODO - To Be Decided
                };

                courseRuns.Add(courseRun);
            }

            // TODO: To be modified once we implement user management (Assign ProviderUKPRN to user)
            int UKPRN = 0;
            if (Session.GetInt32("UKPRN").HasValue)
            {
                UKPRN = Session.GetInt32("UKPRN").Value;
            }
            else
            {
                return RedirectToAction("Index", "Home", new {errmsg = "Please select a Provider."});
            }

            var course = new Course
            {
                id = Guid.NewGuid(),

                QualificationCourseTitle = learnAimRefTitle,
                LearnAimRef = learnAimRef,
                NotionalNVQLevelv2 = notionalNvqLevelv2,
                AwardOrgCode = awardOrgCode,
                QualificationType = learnAimRefTypeDesc,
                ProviderUKPRN = UKPRN, // TODO: ToBeChanged
                CourseDescription = courseFor,
                EntryRequirements = entryRequirements,
                WhatYoullLearn = whatWillLearn,
                HowYoullLearn = howYouWillLearn,
                WhatYoullNeed = whatYouNeed,
                HowYoullBeAssessed = howAssessed,
                WhereNext = whereNext,
                AdvancedLearnerLoan = advancedLearnerLoan,
                AdultEducationBudget = adultEducationBudget,

                CourseRuns = courseRuns,

                CreatedDate = DateTime.Now,
                CreatedBy = "ProviderPortal-AddCourse", // TODO - Change to the name of the logged person

                //RecordStatus = RecordStatus.Live // TODO - To Be Decided
            };

            var result = await _courseService.AddCourseAsync(course);

            RemoveSessionVariables();

            if (result.IsSuccess && result.HasValue)
            {
                return RedirectToAction("Courses", "Provider",
                    new { level = notionalNvqLevelv2 });
            }

            return RedirectToAction("Index",
                new {status = "bad", learnAimRef = learnAimRef, errmsg = result.Error});

            //return RedirectToAction("Index", "Home");
        }


        [Authorize]
        [HttpGet]
        public IActionResult SummaryToAddCourse()
        {
            var addCourse = GetSection1ViewModel();

            // set the last page
            Session.SetObject(SessionLastAddCoursePage, AddCoursePage.Summary);

            // old BackToAddCourseSection1
            return View("AddCourse", addCourse);
        }

        [Authorize]
        public async Task<IActionResult> SummaryToAddCourseRun()
        {
            var addCourseRun = await GetSection2ViewModel();
            if (addCourseRun == null)
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });

            // set the last page
            Session.SetObject(SessionLastAddCoursePage, AddCoursePage.Summary);

            // old BackToAddCourseSection2
            return View("AddCourseRun", addCourseRun);
        }


        #region Private methods
        private async Task<SelectVenueModel> GetVenuesByUkprn(int ukprn)
        {
            var selectVenue = new SelectVenueModel
            {
                LabelText = "Select course venue",
                HintText = "Select all that apply.",
                AriaDescribedBy = "Select all that apply.",
                Ukprn = ukprn
            };
            var requestModel = new VenueSearchRequestModel { SearchTerm = ukprn.ToString() };
            var criteria = _venueSearchHelper.GetVenueSearchCriteria(requestModel);
            var result = await _venueService.SearchAsync(criteria);
            if (result.IsSuccess && result.HasValue)
            {
                var items = _venueSearchHelper.GetVenueSearchResultItemModels(result.Value.Value);
                var venueItems = new List<VenueItemModel>();

                foreach (var venueSearchResultItemModel in items)
                {
                    venueItems.Add(new VenueItemModel
                    {
                        Id = venueSearchResultItemModel.Id,
                        VenueName = venueSearchResultItemModel.VenueName
                    });
                }

                selectVenue.VenueItems = venueItems;
                if (venueItems.Count == 1)
                {
                    selectVenue.HintText = string.Empty;
                    selectVenue.AriaDescribedBy = string.Empty;
                }
            }

            return selectVenue;
        }

        private SelectRegionModel GetRegions()
        {
            return new SelectRegionModel
            {
                LabelText = "Select course region",
                HintText = "For example, South West",
                AriaDescribedBy = "Select all that apply.",
                RegionItems = new RegionItemModel[]
                {
                    new RegionItemModel
                    {
                        Id = "E12000001",
                        Checked = false,
                        RegionName = "North East",
                        SubRegion = new List<SubRegionItemModel>
                        {
                            new SubRegionItemModel{Id="E06000001", SubRegionName = "County Durham", Checked = false},
                            new SubRegionItemModel{Id="E06000002", SubRegionName = "Darlington", Checked = false},
                            new SubRegionItemModel{Id="E06000003", SubRegionName = "Gateshead", Checked = false},
                            new SubRegionItemModel{Id="E06000004", SubRegionName = "Hartlepool", Checked = false},
                            new SubRegionItemModel{Id="E06000005", SubRegionName = "Middlesbrough", Checked = false},
                            new SubRegionItemModel{Id="E06000047", SubRegionName = "Newcastle upon Tyne", Checked = false},
                            new SubRegionItemModel{Id="E06000057", SubRegionName = "North Tyneside", Checked = false},
                            new SubRegionItemModel{Id="E08000021", SubRegionName = "Northumberland", Checked = false},
                            new SubRegionItemModel{Id="E08000022", SubRegionName = "Redcar and Cleveland", Checked = false},
                            new SubRegionItemModel{Id="E08000023", SubRegionName = "South Tyneside", Checked = false},
                            new SubRegionItemModel{Id="E08000024", SubRegionName = "Stockton-on-Tees", Checked = false},
                            new SubRegionItemModel{Id="E08000037", SubRegionName = "Sunderland", Checked = false},
                        }
                    },
                    new RegionItemModel
                    {
                        Id = "E12000002",
                        Checked = false,
                        RegionName = "North West",
                        SubRegion = new List<SubRegionItemModel>
                        {
                            new SubRegionItemModel{Id="E06000006", SubRegionName  = "Halton", Checked = false},
                            new SubRegionItemModel{Id="E06000007", SubRegionName  = "Warrington", Checked = false},
                            new SubRegionItemModel{Id="E06000008", SubRegionName  = "Blackburn with Darwen", Checked = false},
                            new SubRegionItemModel{Id="E06000009", SubRegionName  = "Blackpool", Checked = false},
                            new SubRegionItemModel{Id="E06000049", SubRegionName  = "Cheshire East", Checked = false},
                            new SubRegionItemModel{Id="E06000050", SubRegionName  = "Cheshire West and Chester", Checked = false},
                            new SubRegionItemModel{Id="E08000001", SubRegionName  = "Bolton", Checked = false},
                            new SubRegionItemModel{Id="E08000002", SubRegionName  = "Bury", Checked = false},
                            new SubRegionItemModel{Id="E08000003", SubRegionName  = "Manchester", Checked = false},
                            new SubRegionItemModel{Id="E08000004", SubRegionName  = "Oldham", Checked = false},
                            new SubRegionItemModel{Id="E08000005", SubRegionName  = "Rochdale", Checked = false},
                            new SubRegionItemModel{Id="E08000006", SubRegionName  = "Salford", Checked = false},
                            new SubRegionItemModel{Id="E08000007", SubRegionName  = "Stockport", Checked = false},
                            new SubRegionItemModel{Id="E08000008", SubRegionName  = "Tameside", Checked = false},
                            new SubRegionItemModel{Id="E08000009", SubRegionName  = "Trafford", Checked = false},
                            new SubRegionItemModel{Id="E08000010", SubRegionName  = "Wigan", Checked = false},
                            new SubRegionItemModel{Id="E08000011", SubRegionName  = "Knowsley", Checked = false},
                            new SubRegionItemModel{Id="E08000012", SubRegionName  = "Liverpool", Checked = false},
                            new SubRegionItemModel{Id="E08000013", SubRegionName  = "St. Helens", Checked = false},
                            new SubRegionItemModel{Id="E08000014", SubRegionName  = "Sefton", Checked = false},
                            new SubRegionItemModel{Id="E08000015", SubRegionName  = "Wirral", Checked = false},
                            new SubRegionItemModel{Id="E10000006", SubRegionName  = "Cumbria", Checked = false},
                            new SubRegionItemModel{Id="E10000017", SubRegionName  = "Lancashire", Checked = false}
                        }
                    },
                    new RegionItemModel
                    {
                        Id = "E12000003",
                        Checked = false,
                        RegionName = "Yorkshire and The Humber",
                        SubRegion = new List<SubRegionItemModel>
                        {
                            new SubRegionItemModel{Id="E06000010", SubRegionName  = "Kingston upon Hull, City of", Checked = false},
                            new SubRegionItemModel{Id="E06000011", SubRegionName  = "East Riding of Yorkshire", Checked = false},
                            new SubRegionItemModel{Id="E06000012", SubRegionName  = "North East Lincolnshire", Checked = false},
                            new SubRegionItemModel{Id="E06000013", SubRegionName  = "North Lincolnshire", Checked = false},
                            new SubRegionItemModel{Id="E06000014", SubRegionName  = "York", Checked = false},
                            new SubRegionItemModel{Id="E08000016", SubRegionName  = "Barnsley", Checked = false},
                            new SubRegionItemModel{Id="E08000017", SubRegionName  = "Doncaster", Checked = false},
                            new SubRegionItemModel{Id="E08000018", SubRegionName  = "Rotherham", Checked = false},
                            new SubRegionItemModel{Id="E08000019", SubRegionName  = "Sheffield", Checked = false},
                            new SubRegionItemModel{Id="E08000032", SubRegionName  = "Bradford", Checked = false},
                            new SubRegionItemModel{Id="E08000033", SubRegionName  = "Calderdale", Checked = false},
                            new SubRegionItemModel{Id="E08000034", SubRegionName  = "Kirklees", Checked = false},
                            new SubRegionItemModel{Id="E08000035", SubRegionName  = "Leeds", Checked = false},
                            new SubRegionItemModel{Id="E08000036", SubRegionName  = "Wakefield", Checked = false},
                            new SubRegionItemModel{Id="E10000023", SubRegionName  = "North Yorkshire", Checked = false}
                        }
                    },
                    new RegionItemModel
                    {
                        Id = "E12000004",
                        Checked = false,
                        RegionName = "East Midlands",
                        SubRegion = new List<SubRegionItemModel>
                        {
                            new SubRegionItemModel{Id="E06000015", SubRegionName  = "Derby", Checked = false},
                            new SubRegionItemModel{Id="E10000007", SubRegionName  = "Derbyshire", Checked = false},
                            new SubRegionItemModel{Id="E06000016", SubRegionName  = "Leicester", Checked = false},
                            new SubRegionItemModel{Id="E10000018", SubRegionName  = "Leicestershire", Checked = false},
                            new SubRegionItemModel{Id="E10000019", SubRegionName  = "Lincolnshire", Checked = false},
                            new SubRegionItemModel{Id="E10000021", SubRegionName  = "Northamptonshire", Checked = false},
                            new SubRegionItemModel{Id="E06000018", SubRegionName  = "Nottingham", Checked = false},
                            new SubRegionItemModel{Id="E10000024", SubRegionName  = "Nottinghamshire", Checked = false},
                            new SubRegionItemModel{Id="E06000017", SubRegionName  = "Rutland", Checked = false}
                        }
                    },
                    new RegionItemModel
                    {
                        Id = "E12000005",
                        Checked = false,
                        RegionName = "West Midlands",
                        SubRegion = new List<SubRegionItemModel>
                        {
                            new SubRegionItemModel{Id="E06000019", SubRegionName  = "Herefordshire, County of", Checked = false},
                            new SubRegionItemModel{Id="E06000020", SubRegionName  = "Telford and Wrekin", Checked = false},
                            new SubRegionItemModel{Id="E06000021", SubRegionName  = "Stoke-on-Trent", Checked = false},
                            new SubRegionItemModel{Id="E06000051", SubRegionName  = "Shropshire", Checked = false},
                            new SubRegionItemModel{Id="E08000025", SubRegionName  = "Birmingham", Checked = false},
                            new SubRegionItemModel{Id="E08000026", SubRegionName  = "Coventry", Checked = false},
                            new SubRegionItemModel{Id="E08000027", SubRegionName  = "Dudley", Checked = false},
                            new SubRegionItemModel{Id="E08000028", SubRegionName  = "Sandwell", Checked = false},
                            new SubRegionItemModel{Id="E08000029", SubRegionName  = "Solihull", Checked = false},
                            new SubRegionItemModel{Id="E08000030", SubRegionName  = "Walsall", Checked = false},
                            new SubRegionItemModel{Id="E08000031", SubRegionName  = "Wolverhampton", Checked = false},
                            new SubRegionItemModel{Id="E10000028", SubRegionName  = "Staffordshire", Checked = false},
                            new SubRegionItemModel{Id="E10000031", SubRegionName  = "Warwickshire", Checked = false},
                            new SubRegionItemModel{Id="E10000034", SubRegionName  = "Worcestershire", Checked = false},
                        }
                    },
                    new RegionItemModel
                    {
                        Id = "E12000006",
                        Checked = false,
                        RegionName = "East of England",
                        SubRegion = new List<SubRegionItemModel>
                        {
                            new SubRegionItemModel{Id="E06000055", SubRegionName  = "Bedford", Checked = false},
                            new SubRegionItemModel{Id="E10000003", SubRegionName  = "Cambridgeshire", Checked = false},
                            new SubRegionItemModel{Id="E06000056", SubRegionName  = "Central Bedfordshire", Checked = false},
                            new SubRegionItemModel{Id="E10000012", SubRegionName  = "Essex", Checked = false},
                            new SubRegionItemModel{Id="E10000015", SubRegionName  = "Hertfordshire", Checked = false},
                            new SubRegionItemModel{Id="E06000032", SubRegionName  = "Luton", Checked = false},
                            new SubRegionItemModel{Id="E10000020", SubRegionName  = "Norfolk", Checked = false},
                            new SubRegionItemModel{Id="E06000031", SubRegionName  = "Peterborough", Checked = false},
                            new SubRegionItemModel{Id="E06000033", SubRegionName  = "Southend-on-Sea", Checked = false},
                            new SubRegionItemModel{Id="E10000029", SubRegionName  = "Suffolk", Checked = false},
                            new SubRegionItemModel{Id="E06000034", SubRegionName  = "Thurrock", Checked = false}
                        }
                    },
                    new RegionItemModel
                    {
                        Id = "E12000007",
                        Checked = false,
                        RegionName = "London",
                        SubRegion = new List<SubRegionItemModel>
                        {
                            new SubRegionItemModel{Id="E09000001", SubRegionName  = "City of London", Checked = false},
                            new SubRegionItemModel{Id="E09000002", SubRegionName  = "Barking and Dagenham", Checked = false},
                            new SubRegionItemModel{Id="E09000003", SubRegionName  = "Barnet", Checked = false},
                            new SubRegionItemModel{Id="E09000004", SubRegionName  = "Bexley,", Checked = false},
                            new SubRegionItemModel{Id="E09000005", SubRegionName  = "Brent", Checked = false},
                            new SubRegionItemModel{Id="E09000006", SubRegionName  = "Bromley", Checked = false},
                            new SubRegionItemModel{Id="E09000007", SubRegionName  = "Camden", Checked = false},
                            new SubRegionItemModel{Id="E09000008", SubRegionName  = "Croydon", Checked = false},
                            new SubRegionItemModel{Id="E09000009", SubRegionName  = "Ealing", Checked = false},
                            new SubRegionItemModel{Id="E09000010", SubRegionName  = "Enfield", Checked = false},
                            new SubRegionItemModel{Id="E09000011", SubRegionName  = "Greenwich", Checked = false},
                            new SubRegionItemModel{Id="E09000012", SubRegionName  = "Hackney", Checked = false},
                            new SubRegionItemModel{Id="E09000013", SubRegionName  = "Hammersmith and Fulham", Checked = false},
                            new SubRegionItemModel{Id="E09000014", SubRegionName  = "Haringey", Checked = false},
                            new SubRegionItemModel{Id="E09000015", SubRegionName  = "Harrow", Checked = false},
                            new SubRegionItemModel{Id="E09000016", SubRegionName  = "Havering", Checked = false},
                            new SubRegionItemModel{Id="E09000017", SubRegionName  = "Hillingdon", Checked = false},
                            new SubRegionItemModel{Id="E09000018", SubRegionName  = "Hounslow", Checked = false},
                            new SubRegionItemModel{Id="E09000019", SubRegionName  = "Islington", Checked = false},
                            new SubRegionItemModel{Id="E09000020", SubRegionName  = "Kensington and Chelsea", Checked = false},
                            new SubRegionItemModel{Id="E09000021", SubRegionName  = "Kingston upon Thames", Checked = false},
                            new SubRegionItemModel{Id="E09000022", SubRegionName  = "Lambeth", Checked = false},
                            new SubRegionItemModel{Id="E09000023", SubRegionName  = "Lewisham", Checked = false},
                            new SubRegionItemModel{Id="E09000024", SubRegionName  = "Merton", Checked = false},
                            new SubRegionItemModel{Id="E09000025", SubRegionName  = "Newham", Checked = false},
                            new SubRegionItemModel{Id="E09000026", SubRegionName  = "Redbridge", Checked = false},
                            new SubRegionItemModel{Id="E09000027", SubRegionName  = "Richmond upon Thames", Checked = false},
                            new SubRegionItemModel{Id="E09000028", SubRegionName  = "Southwark", Checked = false},
                            new SubRegionItemModel{Id="E09000029", SubRegionName  = "Sutton", Checked = false},
                            new SubRegionItemModel{Id="E09000030", SubRegionName  = "Tower Hamlets", Checked = false},
                            new SubRegionItemModel{Id="E09000031", SubRegionName  = "Waltham Forest", Checked = false},
                            new SubRegionItemModel{Id="E09000032", SubRegionName  = "Wandsworth", Checked = false},
                            new SubRegionItemModel{Id="E09000033", SubRegionName  = "Westminster", Checked = false}
                        }
                    },
                    new RegionItemModel
                    {
                        Id = "E12000008",
                        Checked = false,
                        RegionName = "South East",
                        SubRegion = new List<SubRegionItemModel>
                        {
                            new SubRegionItemModel{Id="E06000035", SubRegionName  = "Medway", Checked = false},
                            new SubRegionItemModel{Id="E06000036", SubRegionName  = "Bracknell Forest", Checked = false},
                            new SubRegionItemModel{Id="E06000037", SubRegionName  = "West Berkshire", Checked = false},
                            new SubRegionItemModel{Id="E06000038", SubRegionName  = "Reading", Checked = false},
                            new SubRegionItemModel{Id="E06000039", SubRegionName  = "Slough", Checked = false},
                            new SubRegionItemModel{Id="E06000040", SubRegionName  = "Windsor and Maidenhead", Checked = false},
                            new SubRegionItemModel{Id="E06000041", SubRegionName  = "Wokingham", Checked = false},
                            new SubRegionItemModel{Id="E06000042", SubRegionName  = "Milton Keynes", Checked = false},
                            new SubRegionItemModel{Id="E06000043", SubRegionName  = "Brighton and Hove", Checked = false},
                            new SubRegionItemModel{Id="E06000044", SubRegionName  = "Portsmouth", Checked = false},
                            new SubRegionItemModel{Id="E06000045", SubRegionName  = "Southampton", Checked = false},
                            new SubRegionItemModel{Id="E06000046", SubRegionName  = "Isle of Wight", Checked = false},
                            new SubRegionItemModel{Id="E10000002", SubRegionName  = "Buckinghamshire", Checked = false},
                            new SubRegionItemModel{Id="E10000011", SubRegionName  = "East Sussex", Checked = false},
                            new SubRegionItemModel{Id="E10000014", SubRegionName  = "Hampshire", Checked = false},
                            new SubRegionItemModel{Id="E10000016", SubRegionName  = "Kent", Checked = false},
                            new SubRegionItemModel{Id="E10000025", SubRegionName  = "Oxfordshire", Checked = false},
                            new SubRegionItemModel{Id="E10000030", SubRegionName  = "Surrey", Checked = false},
                            new SubRegionItemModel{Id="E10000032", SubRegionName  = "West Sussex",  Checked = false}
                        }
                    },
                    new RegionItemModel
                    {
                        Id = "E12000009",
                        Checked = false,
                        RegionName = "South West",
                        SubRegion = new List<SubRegionItemModel>
                        {
                            new SubRegionItemModel{Id="E06000022", SubRegionName  = "Bath and North East Somerset", Checked = false},
                            new SubRegionItemModel{Id="E06000023", SubRegionName  = "Bristol, City of", Checked = false},
                            new SubRegionItemModel{Id="E06000024", SubRegionName  = "North Somerset", Checked = false},
                            new SubRegionItemModel{Id="E06000025", SubRegionName  = "South Gloucestershire", Checked = false},
                            new SubRegionItemModel{Id="E06000026", SubRegionName  = "Plymouth", Checked = false},
                            new SubRegionItemModel{Id="E06000027", SubRegionName  = "Torbay", Checked = false},
                            new SubRegionItemModel{Id="E06000028", SubRegionName  = "Bournemouth", Checked = false},
                            new SubRegionItemModel{Id="E06000029", SubRegionName  = "Poole", Checked = false},
                            new SubRegionItemModel{Id="E06000030", SubRegionName  = "Swindon", Checked = false},
                            new SubRegionItemModel{Id="E06000052", SubRegionName  = "Cornwall", Checked = false},
                            new SubRegionItemModel{Id="E06000053", SubRegionName  = "Isles of Scilly", Checked = false},
                            new SubRegionItemModel{Id="E06000054", SubRegionName  = "Wiltshire", Checked = false},
                            new SubRegionItemModel{Id="E10000008", SubRegionName  = "Devon", Checked = false},
                            new SubRegionItemModel{Id="E10000009", SubRegionName  = "Dorset", Checked = false},
                            new SubRegionItemModel{Id="E10000013", SubRegionName  = "Gloucestershire", Checked = false},
                            new SubRegionItemModel{Id="E10000027", SubRegionName  = "Somerset", Checked = false},
                        }
                    }
                }.OrderBy(x => x.RegionName)
            };
        }

        internal void RemoveSessionVariables()
        {
            Session.Remove("LearnAimRef");
            Session.Remove("NotionalNVQLevelv2");
            Session.Remove("AwardOrgCode");
            Session.Remove("LearnAimRefTitle");
            Session.Remove("LearnAimRefTypeDesc");

            Session.Remove(SessionAddCourseSection1);
            Session.Remove(SessionAddCourseSection2);
            Session.Remove(SessionLastAddCoursePage);
        }

        private AddCourseViewModel GetSection1ViewModel()
        {
            var addCourseSection1 = Session.GetObject<AddCourseSection1RequestModel>(SessionAddCourseSection1);

            var courseViewModel = new AddCourseViewModel()
            {
                AwardOrgCode = Session.GetString("AwardOrgCode"),
                LearnAimRef = Session.GetString("LearnAimRef"),
                LearnAimRefTitle = Session.GetString("LearnAimRefTitle"),
                NotionalNVQLevelv2 = Session.GetString("NotionalNVQLevelv2"),
                CourseFor = new CourseForModel
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
                    HintText =
                        "Will it be classroom based exercises, practical on the job, practical but in a simulated work environment, online or a mixture of methods?",
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
                },
                FundingOptions = new FundingOptionsModel
                {
                    FundingOptionsLabelText = "Funding options"
                }

            };

            if (addCourseSection1 != null)
            {
                courseViewModel.CourseFor.CourseFor = addCourseSection1.CourseFor;
                courseViewModel.EntryRequirements.EntryRequirements = addCourseSection1.EntryRequirements;
                courseViewModel.WhatWillLearn.WhatWillLearn = addCourseSection1.WhatWillLearn;
                courseViewModel.HowYouWillLearn.HowYouWillLearn = addCourseSection1.HowYouWillLearn;
                courseViewModel.WhatYouNeed.WhatYouNeed = addCourseSection1.WhatYouNeed;
                courseViewModel.HowAssessed.HowAssessed = addCourseSection1.HowAssessed;
                courseViewModel.WhereNext.WhereNext = addCourseSection1.WhereNext;
                courseViewModel.courseMode = addCourseSection1.CourseMode;
                courseViewModel.FundingOptions.AdultEducationBudget = addCourseSection1.AdultEducationBudget;
                courseViewModel.FundingOptions.AdvancedLearnerLoan = addCourseSection1.AdvancedLearnerLoan;
            }

            return courseViewModel;
        }

        private async Task<AddCourseDetailsViewModel> GetSection2ViewModel()
        {
            var addCourseSection2Session = Session.GetObject<AddCourseRequestModel>(SessionAddCourseSection2);

            int UKPRN = 0;
            if (Session.GetInt32("UKPRN") != null)
            {
                UKPRN = Session.GetInt32("UKPRN").Value;
            }
            else
            {
                return null;
            }

            var viewModel = new AddCourseDetailsViewModel
            {
                LearnAimRef = Session.GetString("LearnAimRef"),
                LearnAimRefTitle = Session.GetString("LearnAimRefTitle"),
                AwardOrgCode = Session.GetString("AwardOrgCode"),
                NotionalNVQLevelv2 = Session.GetString("NotionalNVQLevelv2"),
                CourseName = Session.GetString("LearnAimRefTitle"),
                ProviderUKPRN = UKPRN
            };

            viewModel.SelectVenue = await GetVenuesByUkprn(UKPRN);
            viewModel.SelectRegion = GetRegions();

            Session.SetObject(SessionVenues, viewModel.SelectVenue);
            Session.SetObject(SessionRegions, viewModel.SelectRegion);

            if (addCourseSection2Session != null)
            {
                viewModel.CourseName = addCourseSection2Session.CourseName;
                viewModel.CourseProviderReference = addCourseSection2Session.CourseProviderReference;
                viewModel.DeliveryMode = addCourseSection2Session.DeliveryMode;
                viewModel.StartDateType = (StartDateType)Enum.Parse(typeof(StartDateType), addCourseSection2Session.StartDateType);
                viewModel.Day = addCourseSection2Session.Day;
                viewModel.Month = addCourseSection2Session.Month;
                viewModel.Year = addCourseSection2Session.Year;
                viewModel.Url = addCourseSection2Session.Url;
                viewModel.Cost = addCourseSection2Session.Cost == null ? string.Empty : addCourseSection2Session.Cost.ToString();
                viewModel.CostDescription = addCourseSection2Session.CostDescription;
                viewModel.DurationLength = addCourseSection2Session.DurationLength.ToString();

                viewModel.DurationUnit = addCourseSection2Session.DurationUnit;

                viewModel.StudyMode = addCourseSection2Session.StudyMode;
                viewModel.AttendanceMode = addCourseSection2Session.AttendanceMode;
                if (addCourseSection2Session.SelectedVenues != null)
                {
                    foreach (var selectedVenue in addCourseSection2Session.SelectedVenues)
                    {
                        viewModel.SelectVenue.VenueItems.First(x => x.Id == selectedVenue.ToString()).Checked = true;
                    }
                }
                if (addCourseSection2Session.SelectedRegions != null)
                {
                    foreach (var selectedRegion in addCourseSection2Session.SelectedRegions)
                    {
                        viewModel.SelectRegion.RegionItems.First(x => x.Id == selectedRegion.ToString()).Checked = true;
                    }
                }
            }
            else
            {
                viewModel.DurationUnit = DurationUnit.Months;
                viewModel.StudyMode = StudyMode.FullTime;
                viewModel.AttendanceMode = AttendancePattern.Daytime;
                viewModel.DeliveryMode = DeliveryMode.ClassroomBased;
                viewModel.StartDateType = StartDateType.SpecifiedStartDate;
            }

            return viewModel;
        }
        #endregion  

    }
}