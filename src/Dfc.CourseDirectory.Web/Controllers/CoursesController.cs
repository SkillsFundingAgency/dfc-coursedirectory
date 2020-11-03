using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Services.Enums;
using Dfc.CourseDirectory.Services.Models.Courses;
using Dfc.CourseDirectory.Services.Models.Regions;
using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.CourseTextService;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.CourseTextService;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;
using Dfc.CourseDirectory.Web.Extensions;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.CourseFor;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.CourseRun;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.EntryRequirements;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.HowAssessed;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.HowYouWillLearn;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.SelectVenue;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.WhatWillLearn;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.WhatYouNeed;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.WhereNext;
using Dfc.CourseDirectory.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CourseRun = Dfc.CourseDirectory.Services.Models.Courses.CourseRun;

namespace Dfc.CourseDirectory.Web.Controllers
{
    [Authorize("Fe")]
    public class CoursesController : Controller
    {
        private readonly ILogger<CoursesController> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ICourseService _courseService;
        private ISession _session => _contextAccessor.HttpContext.Session;
        private readonly IVenueSearchHelper _venueSearchHelper;
        private readonly IVenueService _venueService;
        private const string SessionAddCourseSection1 = "AddCourseSection1";
        private const string SessionAddCourseSection2 = "AddCourseSection2";
        private const string SessionVenues = "Venues";
        private const string SessionRegions = "Regions";
        private readonly ICourseTextService _courseTextService;

        public CoursesController(
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
        public async Task<IActionResult> Archive(Guid courseId, Guid courseRunId, string level, string qualificationType, string courseName)
        {
            //archive call
            var result = await _courseService.UpdateStatus(courseId, courseRunId, (int)RecordStatus.Archived);

            if (result.IsSuccess)
            {
                //do something
            }
            else
            {
               //log goto error????? no journey
            }

            //may need changing, not sure what message if anything needs to be displayed
            return RedirectToAction("Index", "ProviderCourses",
                new
                {
                    notificationTitle = "Course deleted: " + courseName,
                    courseRunId = courseRunId
                });
        }



        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Index(CourseRunModel model)
        {
            var UKPRN = _session.GetInt32("UKPRN");
            if (UKPRN.HasValue)
            {
                ICourseSearchResult coursesByUKPRN = (!UKPRN.HasValue
                    ? null
                    : _courseService.GetYourCoursesByUKPRNAsync(new CourseSearchCriteria(UKPRN))
                        .Result.Value);

                var courses = coursesByUKPRN.Value.SelectMany(o => o.Value).SelectMany(i => i.Value).ToList();

                var course = courses.SingleOrDefault(x => x.id == model.CourseId);

                var courserun = course.CourseRuns.SingleOrDefault(x => x.id == model.courseRun.id);

                if (courserun != null)
                {
                    courserun.DurationUnit = model.courseRun.DurationUnit;
                    courserun.AttendancePattern = model.courseRun.AttendancePattern;
                    courserun.DeliveryMode = model.courseRun.DeliveryMode;
                    courserun.FlexibleStartDate = model.courseRun.FlexibleStartDate;
                    courserun.StudyMode = model.courseRun.StudyMode;
                    courserun.Cost = model.courseRun.Cost;
                    courserun.CostDescription = model.courseRun.CostDescription;
                    courserun.CourseName = model.courseRun.CourseName;
                    courserun.CourseURL = model.courseRun.CourseURL;
                    courserun.DurationValue = model.courseRun.DurationValue;
                    courserun.ProviderCourseID = model.courseRun.ProviderCourseID;
                    // courserun.StartDate = model.courseRun.StartDate;
                    courserun.VenueId = model.courseRun.VenueId;
                    courserun.UpdatedDate = DateTime.Now;


                    var updatedCourses = await _courseService.UpdateCourseAsync(course);

                }
                else
                {
                    return RedirectToAction("Index", new { status = "bad", learnAimRef = "", numberOfNewCourses = "", errmsg = "No course run" });
                }
            }
            else
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }


            return RedirectToAction("Index", new { status = "update", learnAimRef = "", numberOfNewCourses = "", errmsg = "", updatedCourseId = model.CourseId });
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> CourseSection2(string learnAimRef, string notionalNVQLevelv2,
            string awardOrgCode, string learnAimRefTitle, string learnAimRefTypeDesc, Guid? courseId, Guid? courseRunId, CourseMode courseMode)
        {
            _session.SetString("LearnAimRef", learnAimRef);
            _session.SetString("NotionalNVQLevelv2", notionalNVQLevelv2);
            _session.SetString("AwardOrgCode", awardOrgCode);
            _session.SetString("LearnAimRefTitle", learnAimRefTitle);
            _session.SetString("LearnAimRefTypeDesc", learnAimRefTypeDesc);

            var course = new Course();
            if (courseId.HasValue)
            {
                course = _courseService.GetCourseByIdAsync(new GetCourseByIdCriteria(courseId.Value)).Result.Value;

                var courseRunForEdit =
                    course.CourseRuns.FirstOrDefault(x => x.id == courseRunId);

                if (courseRunForEdit != null)
                {
                    var venues = await GetVenuesByUkprn(course.ProviderUKPRN);
                    //var regions = GetRegions();

                    var editCourseRunViewModel = new AddCourseDetailsViewModel
                    {
                        LearnAimRef = course.LearnAimRef,
                        LearnAimRefTitle = course.QualificationCourseTitle,
                        AwardOrgCode = course.AwardOrgCode,
                        NotionalNVQLevelv2 = course.NotionalNVQLevelv2,
                        SelectVenue = venues,
                        //SelectRegion = regions,
                        CourseProviderReference = courseRunForEdit?.ProviderCourseID,
                        CourseName = courseRunForEdit?.CourseName,
                        DeliveryMode = courseRunForEdit.DeliveryMode,
                        DurationUnit = courseRunForEdit.DurationUnit,
                        DurationLength = courseRunForEdit.DurationValue?.ToString(),
                        StartDateType = courseRunForEdit.FlexibleStartDate
                            ? StartDateType.FlexibleStartDate
                            : StartDateType.SpecifiedStartDate,
                        Day = courseRunForEdit.StartDate?.Day.ToString("00"),
                        Month = courseRunForEdit.StartDate?.Month.ToString("00"),
                        Year = courseRunForEdit.StartDate?.Year.ToString("0000"),
                        StudyMode = courseRunForEdit.StudyMode,
                        Url = courseRunForEdit.CourseURL,
                        Cost = courseRunForEdit.Cost?.ToString("F"),
                        CostDescription = courseRunForEdit.CostDescription,
                        AttendanceMode = courseRunForEdit.AttendancePattern,
                        CourseMode = courseMode,
                        CourseId = course.id,
                        CourseRunId = courseRunForEdit.id
                    };

                    return View("AddCourseSection2", editCourseRunViewModel);
                }

            }

            return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });

        }
        [Authorize]
        [HttpGet]
        public IActionResult AddCourseSection1(string learnAimRef, string notionalNVQLevelv2, string awardOrgCode, string learnAimRefTitle, string learnAimRefTypeDesc, Guid? courseId)
        {
            _session.SetString("LearnAimRef", learnAimRef);
            _session.SetString("NotionalNVQLevelv2", notionalNVQLevelv2);
            _session.SetString("AwardOrgCode", awardOrgCode);
            _session.SetString("LearnAimRefTitle", learnAimRefTitle);
            _session.SetString("LearnAimRefTypeDesc", learnAimRefTypeDesc);

            Course course = null;
            CourseText defaultCourseText = null;

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
                CourseFor = new CourseForModel()
                {
                    LabelText = "Who is the course for",
                    HintText = "Please provide useful information that helps a learner to make a decision about the suitability of this course. For example learners new to the subject / sector or those with some experience? Any age restrictions?",
                    AriaDescribedBy = "Please enter who this course is for.",
                    CourseFor = course?.CourseDescription ?? defaultCourseText?.CourseDescription
                },

                EntryRequirements = new EntryRequirementsModel()
                {
                    LabelText = "Entry requirements",
                    HintText = "Please provide details of specific academic or vocational entry qualification requirements. Also do learners need specific skills, attributes or evidence? e.g. DBS clearance, driving licence",
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
                    HintText = "Will it be classroom based exercises, practical on the job, practical but in a simulated work environment, online or a mixture of methods?",
                    AriaDescribedBy = "Please enter how you’ll learn",
                    HowYouWillLearn = course?.HowYoullLearn ?? defaultCourseText?.HowYoullLearn
                },
                WhatYouNeed = new WhatYouNeedModel()
                {
                    LabelText = "What you’ll need to bring",
                    HintText = "Please detail anything your learners will need to provide or pay for themselves such as uniform, personal protective clothing, tools or kit",
                    AriaDescribedBy = "Please enter what you need",
                    WhatYouNeed = course?.WhatYoullNeed ?? defaultCourseText?.WhatYoullNeed

                },
                HowAssessed = new HowAssessedModel()
                {
                    LabelText = "How you’ll be assessed",
                    HintText = "Please provide details of all the ways your learners will be assessed for this course. E.g. assessment in the workplace, written assignments, group or individual project work, exam, portfolio of evidence, multiple choice tests.",
                    AriaDescribedBy = "Please enter 'How you’ll be assessed'",
                    HowAssessed = course?.HowYoullBeAssessed ?? defaultCourseText?.HowYoullBeAssessed
                },
                WhereNext = new WhereNextModel()
                {
                    LabelText = "What you can do next",
                    HintText = "What are the opportunities beyond this course? Progression to a higher level course, apprenticeship or direct entry to employment?",
                    AriaDescribedBy = "Please enter 'What you can do next'",
                    WhereNext = course?.WhereNext ?? defaultCourseText?.WhereNext
                }
            };

            vm.courseMode = CourseMode.Add;

            if (courseId.HasValue)
            {
                vm.courseMode = CourseMode.EditCourse;
                vm.CourseId = courseId.Value;
            }

            return View(vm);
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddCourseSection1(AddCourseSection1RequestModel model)
        {
            int UKPRN = 0;

            switch (model.CourseMode)
            {
                case CourseMode.Review:
                case CourseMode.Add:

                    _session.SetObject("AddCourseSection1", model);
                    var addCourseSection2Session = _session.GetObject<AddCourseRequestModel>("AddCourseSection2");

                    if (_session.GetInt32("UKPRN") != null)
                    {
                        UKPRN = _session.GetInt32("UKPRN").Value;
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
                    }

                    var viewModel = new AddCourseDetailsViewModel
                    {
                        LearnAimRef = _session.GetString("LearnAimRef"),
                        LearnAimRefTitle = _session.GetString("LearnAimRefTitle"),
                        AwardOrgCode = _session.GetString("AwardOrgCode"),
                        NotionalNVQLevelv2 = _session.GetString("NotionalNVQLevelv2"),
                        CourseName = _session.GetString("LearnAimRefTitle"),
                        ProviderUKPRN = UKPRN
                    };

                    viewModel.SelectVenue = await GetVenuesByUkprn(UKPRN);
                    //viewModel.SelectRegion = GetRegions();

                    _session.SetObject(SessionVenues, viewModel.SelectVenue);
                    _session.SetObject(SessionRegions, viewModel.ChooseRegion.Regions);

                    if (addCourseSection2Session != null)
                    {
                        viewModel.CourseName = addCourseSection2Session.CourseName;
                        viewModel.CourseProviderReference = addCourseSection2Session.CourseProviderReference;
                        viewModel.DeliveryMode = addCourseSection2Session.DeliveryMode;
                        viewModel.StartDateType = (StartDateType)Enum.Parse(typeof(StartDateType),
                            addCourseSection2Session.StartDateType);
                        viewModel.Day = addCourseSection2Session.Day;
                        viewModel.Month = addCourseSection2Session.Month;
                        viewModel.Year = addCourseSection2Session.Year;
                        viewModel.Url = addCourseSection2Session.Url;
                        viewModel.Cost = addCourseSection2Session.Cost == null
                            ? string.Empty
                            : addCourseSection2Session.Cost.ToString();
                        viewModel.CostDescription = addCourseSection2Session.CostDescription;
                        viewModel.AdvancedLearnerLoan = addCourseSection2Session.AdvancedLearnerLoan;
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
                                viewModel.ChooseRegion.Regions.RegionItems.First(x => x.Id == selectedRegion.ToString())
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

                    return View("AddCourseSection2", viewModel);
                case CourseMode.EditCourseRun:
                    return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
                case CourseMode.EditCourse:

                    var courseForEdit = new Course();
                    if (model.CourseId.HasValue)
                    {
                        courseForEdit = _courseService.GetCourseByIdAsync(new GetCourseByIdCriteria(model.CourseId.Value)).Result.Value;

                        courseForEdit.CourseDescription = model.CourseFor;
                        courseForEdit.EntryRequirements = model.EntryRequirements;
                        courseForEdit.WhatYoullLearn = model.WhatWillLearn;
                        courseForEdit.HowYoullLearn = model.HowYouWillLearn;
                        courseForEdit.WhatYoullNeed = model.WhatYouNeed;
                        courseForEdit.HowYoullBeAssessed = model.HowYouWillLearn;
                        courseForEdit.WhereNext = model.WhereNext;
                        courseForEdit.UpdatedBy = "A User";
                        courseForEdit.UpdatedDate = DateTime.Now;

                        var updatedCourse = await _courseService.UpdateCourseAsync(courseForEdit);

                        return RedirectToAction("Courses", "Qualifications", new { qualificationType = courseForEdit.QualificationType, courseId = updatedCourse.Value.id, courseMode = CourseMode.EditCourse });
                    }

                    return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
                default:
                    return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }



        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> BackToAddCourseSection2()
        {
            // from summary page
            var viewModel = await GetSection2ViewModel();
            if (viewModel == null)
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });

            viewModel.CourseMode = CourseMode.Review;
            return View("AddCourseSection2", viewModel);
        }
        [Authorize]
        [HttpPost]
        public IActionResult BackToAddCourseSection1(AddCourseRequestModel model)
        {
            _session.SetObject(SessionAddCourseSection2, model);
            var courseViewModel = GetSection1ViewModel();
            return View("AddCourseSection1", courseViewModel);
        }
        [Authorize]
        [HttpGet]
        public IActionResult BackToAddCourseSection1()
        {
            // from summary page
            var courseViewModel = GetSection1ViewModel();
            courseViewModel.courseMode = CourseMode.Review;
            return View("AddCourseSection1", courseViewModel);
        }
        [Authorize]
        [HttpPost]
        public IActionResult Preview(AddCourseRequestModel model)
        {
            // save page 2
            _session.SetObject(SessionAddCourseSection2, model);

            return new EmptyResult();
        }


        public IActionResult LandingOptions(CoursesLandingViewModel model)
        {
            switch (model.CoursesLandingOptions)
            {
                case CoursesLandingOptions.Add:
                    return RedirectToAction("Index", "RegulatedQualification");
                case CoursesLandingOptions.Upload:
                    return RedirectToAction("Index","BulkUpload");
                case CoursesLandingOptions.View:
                    return RedirectToAction("Index","ProviderCourses");
                default:
                    return RedirectToAction("LandingOptions", "Qualifications");
            }

        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddCourse()
        {

            var learnAimRef = _session.GetString("LearnAimRef");
            var notionalNVQLevelv2 = _session.GetString("NotionalNVQLevelv2");
            var awardOrgCode = _session.GetString("AwardOrgCode");
            var learnAimRefTitle = _session.GetString("LearnAimRefTitle");
            var learnAimRefTypeDesc = _session.GetString("LearnAimRefTypeDesc");

            var addCourseSection2 = _session.GetObject<AddCourseRequestModel>(SessionAddCourseSection2);

            // TODO - Add error message, if use this check
            if (string.IsNullOrEmpty(learnAimRef) ||
                string.IsNullOrEmpty(notionalNVQLevelv2) ||
                string.IsNullOrEmpty(awardOrgCode) ||
                string.IsNullOrEmpty(learnAimRefTitle) ||
                string.IsNullOrEmpty(learnAimRefTypeDesc)
            )
            {
                return RedirectToAction("AddCourseSection1",
                    new
                    {
                        learnAimRef = learnAimRef,
                        notionalNVQLevelv2 = notionalNVQLevelv2,
                        awardOrgCode = awardOrgCode,
                        learnAimRefTitle = learnAimRefTitle,
                        errmsg = "Course data is missing."
                    });
            }

            if (addCourseSection2.CourseMode == CourseMode.Copy)
            {
                var courseForCopy = new Course();
                if (addCourseSection2.CourseId.HasValue)
                {
                    courseForCopy = _courseService.GetCourseByIdAsync(new GetCourseByIdCriteria(addCourseSection2.CourseId.Value))
                        .Result.Value;

                    var courseRunForCopy =
                        courseForCopy.CourseRuns.FirstOrDefault(x => x.id == addCourseSection2.CourseRunId);

                    if (courseRunForCopy != null)
                    {
                        CourseRun newCourseRun = courseRunForCopy;

                        newCourseRun.id = new Guid();
                        newCourseRun.CourseName = addCourseSection2.CourseName;
                        newCourseRun.CourseURL = addCourseSection2.Url;
                        newCourseRun.ProviderCourseID = addCourseSection2.CourseProviderReference;
                        newCourseRun.DeliveryMode = addCourseSection2.DeliveryMode;

                        bool flexibleStartDate = false;
                        DateTime? specifiedStartDate = null;
                        if (addCourseSection2.StartDateType.Equals("SpecifiedStartDate", StringComparison.InvariantCultureIgnoreCase))
                        {
                            string day = addCourseSection2.Day.Length == 1 ? string.Concat("0", addCourseSection2.Day) : addCourseSection2.Day;
                            string month = addCourseSection2.Month.Length == 1 ? string.Concat("0", addCourseSection2.Month) : addCourseSection2.Month;
                            string startDate = string.Format("{0}-{1}-{2}", day, month, addCourseSection2.Year);
                            specifiedStartDate = DateTime.ParseExact(startDate, "dd-MM-yyyy",
                                System.Globalization.CultureInfo.InvariantCulture);
                        }
                        else if (addCourseSection2.StartDateType.Equals("FlexibleStartDate", StringComparison.InvariantCultureIgnoreCase))
                        {
                            flexibleStartDate = true;
                        }

                        newCourseRun.StartDate = specifiedStartDate;
                        newCourseRun.FlexibleStartDate = flexibleStartDate;
                        newCourseRun.Cost = addCourseSection2.Cost;
                        newCourseRun.CostDescription = addCourseSection2.CostDescription;
                        newCourseRun.DurationUnit = addCourseSection2.DurationUnit;
                        newCourseRun.DurationValue = addCourseSection2.DurationLength;
                        newCourseRun.AttendancePattern = addCourseSection2.AttendanceMode;
                        newCourseRun.StudyMode = addCourseSection2.StudyMode;
                        newCourseRun.CreatedDate = DateTime.Now;
                        newCourseRun.CreatedBy = "A User"; // TODO - change to login user
                        newCourseRun.UpdatedBy = string.Empty;
                        newCourseRun.UpdatedDate = null;

                        newCourseRun.RecordStatus = RecordStatus.Live; // TODO - To Be Decided

                        courseForCopy.CourseRuns.ToList().Add(newCourseRun);

                        var updatedCourse = await _courseService.UpdateCourseAsync(courseForCopy);

                        if (updatedCourse.IsSuccess && updatedCourse.HasValue)
                        {
                            RemoveSessionVariables();
                            return RedirectToAction("Courses", "Qualifications",
                                new { qualificationType = courseForCopy.QualificationType });
                        }
                    }
                }
                return RedirectToAction("AddCourseSection1",
                    new
                    {
                        learnAimRef = learnAimRef,
                        notionalNVQLevelv2 = notionalNVQLevelv2,
                        awardOrgCode = awardOrgCode,
                        learnAimRefTitle = learnAimRefTitle,
                        errmsg = "Error - cannot update course run"
                    });
            }

            if (addCourseSection2.CourseMode == CourseMode.EditCourseRun)
            {
                var courseForEdit = new Course();
                if (addCourseSection2.CourseId.HasValue)
                {
                    courseForEdit = _courseService.GetCourseByIdAsync(new GetCourseByIdCriteria(addCourseSection2.CourseId.Value))
                        .Result.Value;

                    var courseRunForEdit =
                        courseForEdit.CourseRuns.FirstOrDefault(x => x.id == addCourseSection2.CourseRunId);

                    if (courseRunForEdit != null)
                    {
                        courseRunForEdit.CourseName = addCourseSection2.CourseName;
                        courseRunForEdit.CourseURL = addCourseSection2.Url;
                        courseRunForEdit.ProviderCourseID = addCourseSection2.CourseProviderReference;
                        courseRunForEdit.DeliveryMode = addCourseSection2.DeliveryMode;

                        bool flexibleStartDate = false;
                        DateTime? specifiedStartDate = null;
                        if (addCourseSection2.StartDateType.Equals("SpecifiedStartDate", StringComparison.InvariantCultureIgnoreCase))
                        {
                            string day = addCourseSection2.Day.Length == 1 ? string.Concat("0", addCourseSection2.Day) : addCourseSection2.Day;
                            string month = addCourseSection2.Month.Length == 1 ? string.Concat("0", addCourseSection2.Month) : addCourseSection2.Month;
                            string startDate = string.Format("{0}-{1}-{2}", day, month, addCourseSection2.Year);
                            specifiedStartDate = DateTime.ParseExact(startDate, "dd-MM-yyyy",
                                System.Globalization.CultureInfo.InvariantCulture);
                        }
                        else if (addCourseSection2.StartDateType.Equals("FlexibleStartDate", StringComparison.InvariantCultureIgnoreCase))
                        {
                            flexibleStartDate = true;
                        }

                        courseRunForEdit.StartDate = specifiedStartDate;
                        courseRunForEdit.FlexibleStartDate = flexibleStartDate;
                        courseRunForEdit.Cost = addCourseSection2.Cost;
                        courseRunForEdit.CostDescription = addCourseSection2.CostDescription;
                        courseRunForEdit.DurationUnit = addCourseSection2.DurationUnit;
                        courseRunForEdit.DurationValue = addCourseSection2.DurationLength;
                        courseRunForEdit.AttendancePattern = addCourseSection2.AttendanceMode;
                        courseRunForEdit.StudyMode = addCourseSection2.StudyMode;

                        courseRunForEdit.RecordStatus = RecordStatus.Live; // TODO - To Be Decided

                        var updatedCourse = await _courseService.UpdateCourseAsync(courseForEdit);

                        if (updatedCourse.IsSuccess && updatedCourse.HasValue)
                        {
                            RemoveSessionVariables();
                            return RedirectToAction("Courses", "Qualifications",
                                new { qualificationType = courseForEdit.QualificationType });
                        }
                    }
                }
                return RedirectToAction("AddCourseSection1",
                    new
                    {
                        learnAimRef = learnAimRef,
                        notionalNVQLevelv2 = notionalNVQLevelv2,
                        awardOrgCode = awardOrgCode,
                        learnAimRefTitle = learnAimRefTitle,
                        errmsg = "Error - cannot update course run"
                    });
            }



            if ((addCourseSection2.CourseMode != CourseMode.EditCourseRun) || (addCourseSection2.CourseMode != CourseMode.Copy))
            {

                var addCourseSection1 = _session.GetObject<AddCourseSection1RequestModel>("AddCourseSection1");
                var courseFor = addCourseSection1.CourseFor;
                var entryRequirements = addCourseSection1.EntryRequirements;
                var whatWillLearn = addCourseSection1.WhatWillLearn;
                var howYouWillLearn = addCourseSection1.HowYouWillLearn;
                var whatYouNeed = addCourseSection1.WhatYouNeed;
                var howAssessed = addCourseSection1.HowAssessed;
                var whereNext = addCourseSection1.WhereNext;

                if (addCourseSection2.DeliveryMode == DeliveryMode.ClassroomBased)
                {
                    if (addCourseSection2.SelectedVenues == null || addCourseSection2.SelectedVenues.Count() < 1)
                    {
                        return RedirectToAction("AddCourseSection1",
                            new
                            {
                                learnAimRef = learnAimRef,
                                notionalNVQLevelv2 = notionalNVQLevelv2,
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
                if (addCourseSection2.StartDateType.Equals("SpecifiedStartDate", StringComparison.InvariantCultureIgnoreCase))
                {
                    string day = addCourseSection2.Day.Length == 1 ? string.Concat("0", addCourseSection2.Day) : addCourseSection2.Day;
                    string month = addCourseSection2.Month.Length == 1 ? string.Concat("0", addCourseSection2.Month) : addCourseSection2.Month;
                    string startDate = string.Format("{0}-{1}-{2}", day, month, addCourseSection2.Year);
                    specifiedStartDate = DateTime.ParseExact(startDate, "dd-MM-yyyy",
                        System.Globalization.CultureInfo.InvariantCulture);
                }
                else if (addCourseSection2.StartDateType.Equals("FlexibleStartDate", StringComparison.InvariantCultureIgnoreCase))
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
                            CourseURL = addCourseSection2.Url,
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
                        StudyMode = addCourseSection2.StudyMode,
                        AttendancePattern = addCourseSection2.AttendanceMode,
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
                        StudyMode = addCourseSection2.StudyMode,
                        AttendancePattern = addCourseSection2.AttendanceMode,
                        //Regions = GetRegions().RegionItems.Select(x => x.Id),
                        CreatedDate = DateTime.Now,
                        CreatedBy = "ProviderPortal-AddCourse",
                        RecordStatus = RecordStatus.Live // TODO - To Be Decided
                    };

                    courseRuns.Add(courseRun);
                }

                // TODO: To be modified once we implement user management (Assign ProviderUKPRN to user)
                int UKPRN = 0;
                if (_session.GetInt32("UKPRN").HasValue)
                {
                    UKPRN = _session.GetInt32("UKPRN").Value;
                }
                else
                {
                    return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
                }

                var course = new Course
                {
                    id = Guid.NewGuid(),

                    QualificationCourseTitle = learnAimRefTitle,
                    LearnAimRef = learnAimRef,
                    NotionalNVQLevelv2 = notionalNVQLevelv2,
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
                    AdvancedLearnerLoan = addCourseSection2.AdvancedLearnerLoan,

                    CourseRuns = courseRuns,

                    CreatedDate = DateTime.Now,
                    CreatedBy = "ProviderPortal-AddCourse", // TODO - Change to the name of the logged person

                    //RecordStatus = RecordStatus.Live // TODO - To Be Decided
                };

                var result = await _courseService.AddCourseAsync(course);

                RemoveSessionVariables();

                if (result.IsSuccess && result.HasValue)
                {
                    return RedirectToAction("Courses", "Qualifications", new { qualificationType = learnAimRefTypeDesc });
                }

                return RedirectToAction("Index",
                    new { status = "bad", learnAimRef = learnAimRef, errmsg = result.Error });

            }

            return RedirectToAction("Index", "Home");
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Publish(AddCourseRequestModel model)
        {
            switch (model.CourseMode)
            {
                case CourseMode.EditCourseRun:
                    var courseForEdit = new Course();
                    if (model.CourseId.HasValue)
                    {
                        courseForEdit = _courseService
                            .GetCourseByIdAsync(new GetCourseByIdCriteria(model.CourseId.Value))
                            .Result.Value;

                        var courseRunForEdit =
                            courseForEdit.CourseRuns.FirstOrDefault(x => x.id == model.CourseRunId);

                        if (courseRunForEdit != null)
                        {
                            courseRunForEdit.CourseName = model.CourseName;
                            courseRunForEdit.CourseURL = model.Url;
                            courseRunForEdit.ProviderCourseID = model.CourseProviderReference;
                            courseRunForEdit.DeliveryMode = model.DeliveryMode;

                            bool flexibleStartDate = false;
                            DateTime? specifiedStartDate = null;
                            if (model.StartDateType.Equals("SpecifiedStartDate",
                                StringComparison.InvariantCultureIgnoreCase))
                            {
                                string day = model.Day.Length == 1 ? string.Concat("0", model.Day) : model.Day;
                                string month = model.Month.Length == 1 ? string.Concat("0", model.Month) : model.Month;
                                string courseRunstartDate = string.Format("{0}-{1}-{2}", day, month, model.Year);
                                specifiedStartDate = DateTime.ParseExact(courseRunstartDate, "dd-MM-yyyy",
                                    System.Globalization.CultureInfo.InvariantCulture);
                            }
                            else if (model.StartDateType.Equals("FlexibleStartDate",
                                StringComparison.InvariantCultureIgnoreCase))
                            {
                                flexibleStartDate = true;
                            }

                            courseRunForEdit.StartDate = specifiedStartDate;
                            courseRunForEdit.FlexibleStartDate = flexibleStartDate;
                            courseRunForEdit.Cost = model.Cost;
                            courseRunForEdit.CostDescription = model.CostDescription;
                            courseRunForEdit.DurationUnit = model.DurationUnit;
                            courseRunForEdit.DurationValue = model.DurationLength;
                            courseRunForEdit.AttendancePattern = model.AttendanceMode;
                            courseRunForEdit.StudyMode = model.StudyMode;
                            courseRunForEdit.UpdatedBy = "A User"; //TODO change to user
                            courseRunForEdit.UpdatedDate = DateTime.Now;

                            var updatedCourse = await _courseService.UpdateCourseAsync(courseForEdit);

                            if (updatedCourse.IsSuccess && updatedCourse.HasValue)
                            {
                                RemoveSessionVariables();
                                return RedirectToAction("Courses", "Qualifications",
                                    new
                                    {
                                        qualificationType = courseForEdit.QualificationType,
                                        courseId = updatedCourse.Value.id,
                                        courseRunId = model.CourseRunId,
                                        courseMode = CourseMode.EditCourseRun
                                    });
                            }
                        }
                    }

                    return View("Index", "Home");
                case CourseMode.Copy:
                    var courseForCopy = new Course();
                    if (model.CourseId.HasValue)
                    {
                        courseForCopy = _courseService
                            .GetCourseByIdAsync(new GetCourseByIdCriteria(model.CourseId.Value)).Result.Value;

                        var t = courseForCopy.CourseRuns.ToList();

                        var allCourseRuns = courseForCopy.CourseRuns.ToList();

                        var courseRunForCopy = courseForCopy.CourseRuns.FirstOrDefault(x => x.id == model.CourseRunId);

                        if (courseRunForCopy != null)
                        {
                            CourseRun newCourseRun = new CourseRun
                            {
                                id = Guid.NewGuid(),

                                CourseName = model.CourseName,
                                CourseURL = model.Url,
                                ProviderCourseID = model.CourseProviderReference,
                                DeliveryMode = model.DeliveryMode
                            };

                            bool flexibleStartDate = false;
                            DateTime? specifiedStartDate = null;
                            if (model.StartDateType.Equals("SpecifiedStartDate",
                                StringComparison.InvariantCultureIgnoreCase))
                            {
                                string day = model.Day.Length == 1 ? string.Concat("0", model.Day) : model.Day;
                                string month = model.Month.Length == 1 ? string.Concat("0", model.Month) : model.Month;
                                string courseRunstartDate = string.Format("{0}-{1}-{2}", day, month, model.Year);
                                specifiedStartDate = DateTime.ParseExact(courseRunstartDate, "dd-MM-yyyy",
                                    System.Globalization.CultureInfo.InvariantCulture);
                            }
                            else if (model.StartDateType.Equals("FlexibleStartDate",
                                StringComparison.InvariantCultureIgnoreCase))
                            {
                                flexibleStartDate = true;
                            }

                            newCourseRun.StartDate = specifiedStartDate;
                            newCourseRun.FlexibleStartDate = flexibleStartDate;
                            newCourseRun.Cost = model.Cost;
                            newCourseRun.CostDescription = model.CostDescription;
                            newCourseRun.DurationUnit = model.DurationUnit;
                            newCourseRun.DurationValue = model.DurationLength;
                            newCourseRun.AttendancePattern = model.AttendanceMode;
                            newCourseRun.StudyMode = model.StudyMode;
                            newCourseRun.CreatedDate = DateTime.Now;
                            newCourseRun.CreatedBy = "A User"; //TODO change to user

                            allCourseRuns.Add(newCourseRun);

                            courseForCopy.CourseRuns = allCourseRuns;

                            var updatedCourse = await _courseService.UpdateCourseAsync(courseForCopy);

                            if (updatedCourse.IsSuccess && updatedCourse.HasValue)
                            {
                                RemoveSessionVariables();
                                return RedirectToAction("Courses", "Qualifications",
                                    new
                                    {
                                        qualificationType = courseForCopy.QualificationType,
                                        courseId = updatedCourse.Value.id,
                                        courseRunId = model.CourseRunId,
                                        courseMode = CourseMode.Copy
                                    });
                            }
                        }
                    }

                    return View("Index", "Home");
                default:
                    _session.SetObject(SessionAddCourseSection2, model);
                    var section1 = _session.GetObject<AddCourseSection1RequestModel>(SessionAddCourseSection1);
                    var availableVenues = _session.GetObject<SelectVenueModel>(SessionVenues);
                    var availableRegions = _session.GetObject<SelectRegionModel>(SessionRegions);

                    var venues = new List<string>();
                    var regions = new List<string>();

                    var summaryViewModel = new AddCourseSummaryViewModel
                    {
                        LearnAimRef = section1.LearnAimRef,
                        NotionalNVQLevelv2 = section1.NotionalNVQLevelv2,
                        LearnAimRefTitle = section1.LearnAimRefTitle,
                        WhoIsThisCourseFor = section1.CourseFor,
                        EntryRequirements = section1.EntryRequirements,
                        WhatYouWillLearn = section1.WhatWillLearn,
                        WhereNext = section1.WhereNext,
                        WhatYouWillNeedToBring = section1.WhatYouNeed,
                        HowAssessed = section1.HowAssessed,
                        HowYouWillLearn = section1.HowYouWillLearn
                    };

                    if (model.CourseMode == CourseMode.Review)
                    {
                        var section2 = _session.GetObject<AddCourseRequestModel>(SessionAddCourseSection2);
                        summaryViewModel.CourseName = section2.CourseName;
                        summaryViewModel.CourseId = section2.CourseProviderReference;
                        summaryViewModel.DeliveryMode = section2.DeliveryMode.ToDescription();
                        summaryViewModel.DeliveryModeEnum = section2.DeliveryMode;
                        summaryViewModel.Url = section2.Url;
                        summaryViewModel.Cost = section2.Cost == null
                            ? string.Empty
                            : section2.Cost.ToString();
                        summaryViewModel.CostDescription = section2.CostDescription;
                        summaryViewModel.AdvancedLearnerLoan =
                            section2.AdvancedLearnerLoan ? "Available" : "Unavailable";
                        summaryViewModel.CourseLength = section2.DurationLength + " " + section2.DurationUnit;
                        summaryViewModel.AttendancePattern = section2.StudyMode.ToDescription();
                        summaryViewModel.AttendanceTime = section2.AttendanceMode.ToDescription();
                        summaryViewModel.StartDate = section2.StartDateType == "FlexibleStartDate"
                            ? "Flexible"
                            : section2.Day + "/" + section2.Month + "/" + section2.Year;
                        switch (section2.DeliveryMode)
                        {
                            case DeliveryMode.ClassroomBased:
                                venues.AddRange(from summaryVenueVenueItem in availableVenues.VenueItems
                                                from modelSelectedVenue in section2.SelectedVenues
                                                where modelSelectedVenue.ToString() == summaryVenueVenueItem.Id
                                                select summaryVenueVenueItem.VenueName);

                                summaryViewModel.Venues = venues;
                                break;
                            case DeliveryMode.WorkBased:
                                regions.AddRange(from region in availableRegions.RegionItems
                                                 from modelSelectedRegion in section2.SelectedRegions
                                                 where modelSelectedRegion == region.Id
                                                 select region.RegionName);

                                summaryViewModel.Regions = regions;
                                break;
                        }
                    }
                    else
                    {
                        _session.SetObject(SessionAddCourseSection2, model);
                        summaryViewModel.CourseName = model.CourseName;
                        summaryViewModel.CourseId = model.CourseProviderReference;
                        summaryViewModel.DeliveryMode = model.DeliveryMode.ToDescription();
                        summaryViewModel.DeliveryModeEnum = model.DeliveryMode;
                        summaryViewModel.Url = model.Url;
                        summaryViewModel.Cost = model.Cost == null
                            ? string.Empty
                            : model.Cost.ToString();
                        summaryViewModel.CostDescription = model.CostDescription;
                        summaryViewModel.AdvancedLearnerLoan = model.AdvancedLearnerLoan ? "Available" : "Unavailable";
                        summaryViewModel.CourseLength = model.DurationLength + " " + model.DurationUnit;
                        summaryViewModel.AttendancePattern = model.StudyMode.ToDescription();
                        summaryViewModel.AttendanceTime = model.AttendanceMode.ToDescription();
                        summaryViewModel.StartDate = model.StartDateType == "FlexibleStartDate"
                            ? "Flexible"
                            : model.Day + "/" + model.Month + "/" + model.Year;

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
                                regions.AddRange(from region in availableRegions.RegionItems
                                                 from modelSelectedRegion in model.SelectedRegions
                                                 where modelSelectedRegion == region.Id
                                                 select region.RegionName);

                                summaryViewModel.Regions = regions;
                                break;
                        }
                    }
                    return View("SummaryPage", summaryViewModel);


            }
        }

        internal void RemoveSessionVariables()
        {
            _session.Remove("LearnAimRef");
            _session.Remove("NotionalNVQLevelv2");
            _session.Remove("AwardOrgCode");
            _session.Remove("LearnAimRefTitle");
            _session.Remove("LearnAimRefTypeDesc");

            _session.Remove(SessionAddCourseSection1);
            _session.Remove(SessionAddCourseSection2);
        }

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



        private async Task<AddCourseDetailsViewModel> GetSection2ViewModel()
        {
            var addCourseSection2Session = _session.GetObject<AddCourseRequestModel>(SessionAddCourseSection2);

            int UKPRN = 0;
            if (_session.GetInt32("UKPRN") != null)
            {
                UKPRN = _session.GetInt32("UKPRN").Value;
            }
            else
            {
                return null;
            }

            var viewModel = new AddCourseDetailsViewModel
            {
                LearnAimRef = _session.GetString("LearnAimRef"),
                LearnAimRefTitle = _session.GetString("LearnAimRefTitle"),
                AwardOrgCode = _session.GetString("AwardOrgCode"),
                NotionalNVQLevelv2 = _session.GetString("NotionalNVQLevelv2"),
                CourseName = _session.GetString("LearnAimRefTitle"),
                ProviderUKPRN = UKPRN
            };

            viewModel.SelectVenue = await GetVenuesByUkprn(UKPRN);
           // viewModel.SelectRegion = GetRegions();

            _session.SetObject(SessionVenues, viewModel.SelectVenue);
            _session.SetObject(SessionRegions, viewModel.ChooseRegion.Regions);

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
                viewModel.AdvancedLearnerLoan = addCourseSection2Session.AdvancedLearnerLoan;
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
                        viewModel.ChooseRegion.Regions.RegionItems.First(x => x.Id == selectedRegion.ToString()).Checked = true;
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

        private AddCourseViewModel GetSection1ViewModel()
        {
            var addCourseSection1 = _session.GetObject<AddCourseSection1RequestModel>(SessionAddCourseSection1);

            var courseViewModel = new AddCourseViewModel()
            {
                AwardOrgCode = _session.GetString("AwardOrgCode"),
                LearnAimRef = _session.GetString("LearnAimRef"),
                LearnAimRefTitle = _session.GetString("LearnAimRefTitle"),
                NotionalNVQLevelv2 = _session.GetString("NotionalNVQLevelv2"),
                CourseFor = new CourseForModel
                {
                    LabelText = "Who is the course for",
                    HintText =
                        "Please provide useful information that helps a learner to make a decision about the suitability of this course. For example learners new to the subject / sector or those with some experience? Any age restrictions?",
                    AriaDescribedBy = "Please enter who this course is for."
                },
                EntryRequirements = new EntryRequirementsModel()
                {
                    LabelText = "Entry requirements",
                    HintText =
                        "Please provide details of specific academic or vocational entry qualification requirements. Also do learners need specific skills, attributes or evidence? e.g. DBS clearance, driving licence",
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
                    HintText =
                        "Please detail anything your learners will need to provide or pay for themselves such as uniform, personal protective clothing, tools or kit",
                    AriaDescribedBy = "Please enter what you need"
                },
                HowAssessed = new HowAssessedModel()
                {
                    LabelText = "How you’ll be assessed",
                    HintText =
                        "Please provide details of all the ways your learners will be assessed for this course. E.g. assessment in the workplace, written assignments, group or individual project work, exam, portfolio of evidence, multiple choice tests.",
                    AriaDescribedBy = "Please enter 'How you’ll be assessed'"
                },
                WhereNext = new WhereNextModel()
                {
                    LabelText = "What you can do next",
                    HintText =
                        "What are the opportunities beyond this course? Progression to a higher level course, apprenticeship or direct entry to employment?",
                    AriaDescribedBy = "Please enter 'What you can do next'"
                },

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
            }

            return courseViewModel;
        }
    }
}
