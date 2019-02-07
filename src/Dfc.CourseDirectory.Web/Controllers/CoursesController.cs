using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Interfaces.Courses;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;
using Dfc.CourseDirectory.Services.VenueService;
using Dfc.CourseDirectory.Web.Extensions;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.ViewModels;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.CourseFor;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.CourseRun;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.EntryRequirements;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.HowAssessed;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.HowYouWillLearn;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.SelectVenue;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.WhatWillLearn;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.WhatYouNeed;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.WhereNext;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CourseRun = Dfc.CourseDirectory.Models.Models.Courses.CourseRun;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.SelectRegion;

namespace Dfc.CourseDirectory.Web.Controllers
{
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

        public CoursesController(
            ILogger<CoursesController> logger,
            IOptions<CourseServiceSettings> courseSearchSettings,
            IHttpContextAccessor contextAccessor,
            ICourseService courseService, IVenueSearchHelper venueSearchHelper, IVenueService venueService)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(courseSearchSettings, nameof(courseSearchSettings));
            Throw.IfNull(contextAccessor, nameof(contextAccessor));
            Throw.IfNull(courseService, nameof(courseService));
            Throw.IfNull(venueService, nameof(venueService));

            _logger = logger;
            _contextAccessor = contextAccessor;
            _courseService = courseService;
            _venueService = venueService;
            _venueSearchHelper = venueSearchHelper;
        }

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

            return RedirectToAction("Index", new { status = "update", learnAimRef = "", numberOfNewCourses = "", errmsg = "", updatedCourseId = model.CourseId });
        }




        //public async Task<JsonResult> Test1(int prn)
        //{
        //    _session.SetInt32("UKPRN", prn);
        //    IActionResult view = await GetYourCoursesViewModelAsync("", "", "", "", null);
        //    YourCoursesViewModel vm = (YourCoursesViewModel)(((ViewResult)view).Model);
        //    return new JsonResult(vm);
        //}

        //public async Task<JsonResult> Test2(int prn)
        //{
        //    _session.SetInt32("UKPRN", prn);
        //    IActionResult view = await GetYourCoursesViewModelAsync("", "", "", "", null);
        //    YourCoursesViewModel vm = (YourCoursesViewModel)(((ViewResult)view).Model);
        //    //vm.Courses.Value = vm.Courses.Value.Select(c => new CourseSearchOuterGrouping(c.Value, c.QualType)); //new CourseSearchResult(vm.Courses.Value);
        //    IEnumerable<ICourseSearchOuterGrouping> outers = vm.Courses.Value.Select(c => new CourseSearchOuterGrouping(c.Value, c.QualType));
        //    return new JsonResult(outers); //vm);
        //}

        //public async Task<JsonResult> Test3(int prn, string qualType)
        //{
        //    _session.SetInt32("UKPRN", prn);
        //    IActionResult view = await GetYourCoursesViewModelAsync("", "", "", "", null);
        //    YourCoursesViewModel vm = (YourCoursesViewModel)(((ViewResult)view).Model);
        //    var inners = vm.Courses.Value.FirstOrDefault(o => o.QualType == qualType)
        //                                                                     .Value
        //                                                                     .Select(t => new { Course = t.Value, Text = t.LARSRef });


        //    return new JsonResult(inners);
        //}

        //public async Task<JsonResult> Test4(int prn, string qualType, string larsRef)
        //{
        //    _session.SetInt32("UKPRN", prn);
        //    IActionResult view = await GetYourCoursesViewModelAsync("", "", "", "", null);
        //    YourCoursesViewModel vm = (YourCoursesViewModel)(((ViewResult)view).Model);
        //    IEnumerable<Course> courses = vm.Courses.Value.FirstOrDefault(o => o.QualType == qualType)
        //                                                  .Value
        //                                                  .FirstOrDefault(i => i.LARSRef == larsRef)
        //                                                  .Value
        //                                                  .Select(c => c.WithNoCourseRuns());
        //    return new JsonResult(courses);
        //}

        //public async Task<IActionResult> Level1(string status, string learnAimRef, string numberOfNewCourses, string errmsg, Guid? updatedCourseId)
        //{
        //    IActionResult view = await GetYourCoursesViewModelAsync(status, learnAimRef, numberOfNewCourses, errmsg, updatedCourseId);
        //    YourCoursesViewModel vm = (YourCoursesViewModel)(((ViewResult)view).Model);
        //    return View(new YourCoursesViewModel() {
        //        UpdatedCourseId = vm.UpdatedCourseId,
        //        UKPRN = vm.UKPRN,
        //        deliveryModes = vm.deliveryModes,
        //        durationUnits = vm.durationUnits,
        //        attendances = vm.attendances,
        //        modes = vm.modes,
        //        Venues = vm.Venues,
        //        Courses = new CourseSearchResult(vm.Courses.Value) //.Select(x => new CourseSearchOuterGrouping(x, false))
        //    });
        //}


        public async Task<IActionResult> Index(string status, string learnAimRef, string numberOfNewCourses, string errmsg, Guid? updatedCourseId)
        {
            IActionResult view = await GetYourCoursesViewModelAsync(status, learnAimRef, numberOfNewCourses, errmsg, updatedCourseId);
            return view;
        }

        private async Task<IActionResult> GetYourCoursesViewModelAsync(string status, string learnAimRef, string numberOfNewCourses, string errmsg, Guid? updatedCourseId)
        {

            var deliveryModes = new List<SelectListItem>();
            var durationUnits = new List<SelectListItem>();
            var attendances = new List<SelectListItem>();
            var modes = new List<SelectListItem>();

            if (!string.IsNullOrEmpty(status))
            {
                ViewData["Status"] = status;
                switch (status.ToUpper())
                {
                    case "GOOD":
                        ViewData["StatusMessage"] = string.Format("{0} New Course(s) created in Course Directory for LARS: {1}", numberOfNewCourses, learnAimRef);
                        break;
                    case "BAD":
                        ViewData["StatusMessage"] = errmsg;
                        break;
                    case "UPDATE":
                        ViewData["StatusMessage"] = string.Format("Course run updated in Course Directory");
                        break;
                    default:
                        break;
                }
            }

            List<SelectListItem> courseRunVenues = new List<SelectListItem>();
            int? UKPRN = _session.GetInt32("UKPRN");

            if (UKPRN.HasValue)
            {
                VenueSearchCriteria criteria = new VenueSearchCriteria(UKPRN.ToString(), null);
                var venues = await _venueService.SearchAsync(criteria);

                foreach (var venue in venues.Value.Value)
                {
                    var item = new SelectListItem { Text = venue.VenueName, Value = venue.ID };
                    courseRunVenues.Add(item);
                };
            }
            else
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            foreach (DeliveryMode eVal in DeliveryMode.GetValues(typeof(DeliveryMode)))
            {
                if (eVal.ToString().ToUpper() != "UNDEFINED")
                {
                    var item = new SelectListItem { Text = WebHelper.GetEnumDescription(eVal) };
                    deliveryModes.Add(item);
                }
            };

            foreach (DurationUnit eVal in DurationUnit.GetValues(typeof(DurationUnit)))
            {
                if (eVal.ToString().ToUpper() != "UNDEFINED")
                {
                    var item = new SelectListItem { Text = WebHelper.GetEnumDescription(eVal) };
                    durationUnits.Add(item);
                }
            };

            foreach (AttendancePattern eVal in AttendancePattern.GetValues(typeof(AttendancePattern)))
            {
                if (eVal.ToString().ToUpper() != "UNDEFINED")
                {
                    var item = new SelectListItem { Text = WebHelper.GetEnumDescription(eVal) };
                    attendances.Add(item);
                }
            };

            foreach (Dfc.CourseDirectory.Models.Models.Courses.StudyMode eVal in Enum.GetValues(typeof(Dfc.CourseDirectory.Models.Models.Courses.StudyMode)))
            {
                if (eVal.ToString().ToUpper() != "UNDEFINED")
                {
                    var item = new SelectListItem { Text = WebHelper.GetEnumDescription(eVal) };
                    modes.Add(item);
                }
            };

            // Get courses (and runs) for PRN, grouped by qualification type, then within that by LARS ref
            //int? ukprn = _session.GetInt32("UKPRN");
            if (UKPRN.HasValue)
            {
                ICourseSearchResult result = (!UKPRN.HasValue ? null :
                                              _courseService.GetYourCoursesByUKPRNAsync(new CourseSearchCriteria(UKPRN))
                                                            .Result.Value);


                YourCoursesViewModel vm = new YourCoursesViewModel
                {
                    UpdatedCourseId = updatedCourseId ?? null,
                    UKPRN = UKPRN,
                    Courses = result,
                    deliveryModes = deliveryModes,
                    durationUnits = durationUnits,
                    attendances = attendances,
                    modes = modes,
                    Venues = courseRunVenues
                };

                return View(vm);

            }
            else
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }
        }


        [HttpGet]
        public async Task<IActionResult> EditCourseSection2(string learnAimRef, string notionalNVQLevelv2,
            string awardOrgCode, string learnAimRefTitle, string learnAimRefTypeDesc, Guid? courseId, Guid? courseRunId)
        {
            _session.SetString("LearnAimRef", learnAimRef);
            _session.SetString("NotionalNVQLevelv2", notionalNVQLevelv2);
            _session.SetString("AwardOrgCode", awardOrgCode);
            _session.SetString("LearnAimRefTitle", learnAimRefTitle);
            _session.SetString("LearnAimRefTypeDesc", learnAimRefTypeDesc);

            ICourse course = new Course();
            if (courseId.HasValue)
            {
                course = _courseService.GetCourseByIdAsync(new GetCourseByIdCriteria(courseId.Value)).Result.Value;

                var courseRunForEdit =
                    course.CourseRuns.FirstOrDefault(x => x.id == courseRunId);

                if (courseRunForEdit != null)
                {
                    var venues = await GetVenuesByUkprn(course.ProviderUKPRN);
                    var regions = GetRegions();

                    var editCourseRunViewModel = new AddCourseDetailsViewModel
                    {
                        LearnAimRef = course.LearnAimRef,
                        LearnAimRefTitle = course.QualificationCourseTitle,
                        AwardOrgCode = course.AwardOrgCode,
                        NotionalNVQLevelv2 = course.NotionalNVQLevelv2,
                        SelectVenue = venues,
                        SelectRegion = regions,
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
                        CourseMode = CourseMode.EditCourseRun,
                        CourseId = course.id,
                        CourseRunId = courseRunForEdit.id
                    };

                    return View("AddCourseSection2", editCourseRunViewModel);
                }

            }

            return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });

        }

        [HttpGet]
        public async Task<IActionResult> AddCourseSection1(string learnAimRef, string notionalNVQLevelv2, string awardOrgCode, string learnAimRefTitle, string learnAimRefTypeDesc, Guid? courseId)
        {
            _session.SetString("LearnAimRef", learnAimRef);
            _session.SetString("NotionalNVQLevelv2", notionalNVQLevelv2);
            _session.SetString("AwardOrgCode", awardOrgCode);
            _session.SetString("LearnAimRefTitle", learnAimRefTitle);
            _session.SetString("LearnAimRefTypeDesc", learnAimRefTypeDesc);

            ICourse course = new Course();
            if (courseId.HasValue)
            {
                course = _courseService.GetCourseByIdAsync(new GetCourseByIdCriteria(courseId.Value)).Result.Value;
            }

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
                    AriaDescribedBy = "Please enter who this course is for.",
                    CourseFor = course?.CourseDescription
                },
                EntryRequirements = new EntryRequirementsModel()
                {
                    LabelText = "Entry requirements",
                    HintText = "Please provide details of specific academic or vocational entry qualification requirements. Also do learners need specific skills, attributes or evidence? e.g. DBS clearance, driving licence",
                    AriaDescribedBy = "Please list entry requirements.",
                    EntryRequirements = course?.EntryRequirements
                },
                WhatWillLearn = new WhatWillLearnModel()
                {
                    LabelText = "What you’ll learn",
                    HintText = "Give learners a taste of this course. What are the main topics covered?",
                    AriaDescribedBy = "Please enter what will be learned",
                    WhatWillLearn = course?.WhatYoullLearn
                },
                HowYouWillLearn = new HowYouWillLearnModel()
                {
                    LabelText = "How you’ll learn",
                    HintText = "Will it be classroom based exercises, practical on the job, practical but in a simulated work environment, online or a mixture of methods?",
                    AriaDescribedBy = "Please enter how you’ll learn",
                    HowYouWillLearn = course?.HowYoullLearn
                },
                WhatYouNeed = new WhatYouNeedModel()
                {
                    LabelText = "What you’ll need to bring",
                    HintText = "Please detail anything your learners will need to provide or pay for themselves such as uniform, personal protective clothing, tools or kit",
                    AriaDescribedBy = "Please enter what you need",
                    WhatYouNeed = course?.WhatYoullNeed

                },
                HowAssessed = new HowAssessedModel()
                {
                    LabelText = "How you’ll be assessed",
                    HintText = "Please provide details of all the ways your learners will be assessed for this course. E.g. assessment in the workplace, written assignments, group or individual project work, exam, portfolio of evidence, multiple choice tests.",
                    AriaDescribedBy = "Please enter 'How you’ll be assessed'",
                    HowAssessed = course?.HowYoullBeAssessed
                },
                WhereNext = new WhereNextModel()
                {
                    LabelText = "Where next?",
                    HintText = "What are the opportunities beyond this course? Progression to a higher level course, apprenticeship or direct entry to employment?",
                    AriaDescribedBy = "Please enter 'Where next?'",
                    WhereNext = course?.WhereNext
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

        [HttpPost]
        public async Task<IActionResult> AddCourseSection1(AddCourseSection1RequestModel model)
        {
            int UKPRN = 0;

            switch (model.CourseMode)
            {
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
                    viewModel.SelectRegion = GetRegions();

            _session.SetObject(SessionVenues, viewModel.SelectVenue);
            _session.SetObject(SessionRegions, viewModel.SelectRegion);

            if (addCourseSection2Session != null)
            {
                viewModel.CourseProviderReference = addCourseSection2Session.CourseProviderReference;
                viewModel.DeliveryMode = addCourseSection2Session.DeliveryMode;
                viewModel.StartDateType = (StartDateType)Enum.Parse(typeof(StartDateType), addCourseSection2Session.StartDateType);
                viewModel.Day = Convert.ToInt32(addCourseSection2Session.Day);
                viewModel.Month = Convert.ToInt32(addCourseSection2Session.Month);
                viewModel.Year = Convert.ToInt32(addCourseSection2Session.Year);
                viewModel.Url = addCourseSection2Session.Url;
                viewModel.Cost = addCourseSection2Session.Cost == 0 ? string.Empty : addCourseSection2Session.Cost.ToString(CultureInfo.InvariantCulture);
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

                    return View("AddCourseSection2", viewModel);
                case CourseMode.EditCourseRun:
                    return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
                case CourseMode.EditCourse:

                    ICourse courseForEdit = new Course();
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

                        return RedirectToAction("Courses", "Qualifications", new { qualificationType = courseForEdit.QualificationType });
                    }

                    return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
                default:
                    return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }



        }

        [HttpGet]
        public async Task<IActionResult> BackToAddCourseSection2()
        {
            var viewModel = await GetSection2ViewModel();
            if (viewModel == null)
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });

            return View("AddCourseSection2", viewModel);
        }

        [HttpPost]
        public IActionResult BackToAddCourseSection1(AddCourseRequestModel model)
        {
            _session.SetObject(SessionAddCourseSection2, model);
            var courseViewModel = GetSection1ViewModel();
            return View("AddCourseSection1", courseViewModel);
        }

        [HttpGet]
        public IActionResult BackToAddCourseSection1()
        {
            var courseViewModel = GetSection1ViewModel();
            return View("AddCourseSection1", courseViewModel);
        }

        [HttpPost]
        public IActionResult Preview(AddCourseRequestModel model)
        {
            // save page 2
            _session.SetObject(SessionAddCourseSection2, model);

            return new EmptyResult();
        }


        //public async Task<IActionResult> EditCourseRun(Guid? CourseId, Guid? CourseRunId)
        //{

        //    return View();
        //}

        //public async Task<IActionResult> EditCourse(Guid? CourseId)
        //{
        //    if (CourseId.HasValue)
        //    {
        //        var course = await _courseService.GetCourseByIdAsync(new GetCourseByIdCriteria(CourseId.Value));

        //        return RedirectToAction("AddCourseSection1", new { learnAimRef = course.Value.LearnAimRef, notionalNVQLevelv2 = course.Value.NotionalNVQLevelv2, awardOrgCode = course.Value.AwardOrgCode, learnAimRefTitle = course.Value.QualificationCourseTitle, learnAimRefTypeDesc = course.Value.QualificationType});
        //    }

        //    return View("../Qualifications/Courses");

        //}

        [HttpPost]
        public async Task<IActionResult> AddCourse(AddCourseRequestModel model)
        {

            var learnAimRef = _session.GetString("LearnAimRef");
            var notionalNVQLevelv2 = _session.GetString("NotionalNVQLevelv2");
            var awardOrgCode = _session.GetString("AwardOrgCode");
            var learnAimRefTitle = _session.GetString("LearnAimRefTitle");
            var learnAimRefTypeDesc = _session.GetString("LearnAimRefTypeDesc");

            var addCourseSection1 = _session.GetObject<AddCourseSection1RequestModel>(SessionAddCourseSection1);
            var courseFor = addCourseSection1.CourseFor;
            var entryRequirements = addCourseSection1.EntryRequirements;
            var whatWillLearn = addCourseSection1.WhatWillLearn;
            var howYouWillLearn = addCourseSection1.HowYouWillLearn;
            var whatYouNeed = addCourseSection1.WhatYouNeed;
            var howAssessed = addCourseSection1.HowAssessed;
            var whereNext = addCourseSection1.WhereNext;


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

            if (model.CourseMode == CourseMode.EditCourseRun)
            {
                ICourse courseForEdit = new Course();
                if (model.CourseId.HasValue)
                {
                    courseForEdit = _courseService.GetCourseByIdAsync(new GetCourseByIdCriteria(model.CourseId.Value))
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
                        if (model.StartDateType.Equals("SpecifiedStartDate", StringComparison.InvariantCultureIgnoreCase))
                        {
                            string day = model.Day.Length == 1 ? string.Concat("0", model.Day) : model.Day;
                            string month = model.Month.Length == 1 ? string.Concat("0", model.Month) : model.Month;
                            string startDate = string.Format("{0}-{1}-{2}", day, month, model.Year);
                            specifiedStartDate = DateTime.ParseExact(startDate, "dd-MM-yyyy",
                                System.Globalization.CultureInfo.InvariantCulture);
                        }
                        else if (model.StartDateType.Equals("FlexibleStartDate", StringComparison.InvariantCultureIgnoreCase))
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
            else
            {

                var addCourseSection1 = _session.GetObject<AddCourseSection1RequestModel>("AddCourseSection1");
                var courseFor = addCourseSection1.CourseFor;
                var entryRequirements = addCourseSection1.EntryRequirements;
                var whatWillLearn = addCourseSection1.WhatWillLearn;
                var howYouWillLearn = addCourseSection1.HowYouWillLearn;
                var whatYouNeed = addCourseSection1.WhatYouNeed;
                var howAssessed = addCourseSection1.HowAssessed;
                var whereNext = addCourseSection1.WhereNext;

                if (model.DeliveryMode == DeliveryMode.ClassroomBased)
                {
                    if (model.SelectedVenues == null || model.SelectedVenues.Count() < 1)
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
                if (model.StartDateType.Equals("SpecifiedStartDate", StringComparison.InvariantCultureIgnoreCase))
                {
                    string day = model.Day.Length == 1 ? string.Concat("0", model.Day) : model.Day;
                    string month = model.Month.Length == 1 ? string.Concat("0", model.Month) : model.Month;
                    string startDate = string.Format("{0}-{1}-{2}", day, month, model.Year);
                    specifiedStartDate = DateTime.ParseExact(startDate, "dd-MM-yyyy",
                        System.Globalization.CultureInfo.InvariantCulture);
                }
                else if (model.StartDateType.Equals("FlexibleStartDate", StringComparison.InvariantCultureIgnoreCase))
                {
                    flexibleStartDate = true;
                }
                //else
                //{
                //    // StartDateType not defined - log it.
                //    // specifiedStartDate will be DateTime.MinValue;
                //    // and flexibleStartDate = false
                //}

                if (model.DeliveryMode == DeliveryMode.ClassroomBased
                    && model.SelectedVenues != null
                    && model.SelectedVenues.Any())
                {
                    foreach (var venue in model.SelectedVenues)
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
                            CourseURL = model.Url?.ToLower(),
                            Cost = model.Cost,
                            CostDescription = model.CostDescription,
                            DurationUnit = model.DurationUnit,
                            DurationValue = model.DurationLength,
                            StudyMode = model.StudyMode,
                            AttendancePattern = model.AttendanceMode,
                            Regions = model.SelectedRegions,
                            CreatedDate = DateTime.Now,
                            CreatedBy = "ProviderPortal-AddCourse" // TODO - Change to the name of the logged person 
                        };

                        courseRuns.Add(courseRun);
                    }
                }

                if (model.DeliveryMode == DeliveryMode.WorkBased
                    && model.SelectedRegions != null
                    && model.SelectedRegions.Any())
                {
                    var courseRun = new CourseRun
                    {
                        id = Guid.NewGuid(),

                        CourseName = model.CourseName,
                        ProviderCourseID = model.CourseProviderReference,
                        DeliveryMode = model.DeliveryMode,
                        FlexibleStartDate = flexibleStartDate,
                        StartDate = specifiedStartDate,
                        CourseURL = model.Url,
                        Cost = model.Cost,
                        CostDescription = model.CostDescription,
                        DurationUnit = model.DurationUnit,
                        DurationValue = model.DurationLength,
                        StudyMode = model.StudyMode,
                        AttendancePattern = model.AttendanceMode,
                        Regions = model.SelectedRegions,
                        CreatedDate = DateTime.Now,
                        CreatedBy = "ProviderPortal-AddCourse"
                    };

                    courseRuns.Add(courseRun);
                }

                if (model.DeliveryMode == DeliveryMode.Online)
                {
                    var courseRun = new CourseRun
                    {
                        id = Guid.NewGuid(),

                        CourseName = model.CourseName,
                        ProviderCourseID = model.CourseProviderReference,
                        DeliveryMode = model.DeliveryMode,
                        FlexibleStartDate = flexibleStartDate,
                        StartDate = specifiedStartDate,
                        CourseURL = model.Url,
                        Cost = model.Cost,
                        CostDescription = model.CostDescription,
                        DurationUnit = model.DurationUnit,
                        DurationValue = model.DurationLength,
                        StudyMode = model.StudyMode,
                        AttendancePattern = model.AttendanceMode,
                        Regions = GetRegions().RegionItems.Select(x => x.Id),
                        CreatedDate = DateTime.Now,
                        CreatedBy = "ProviderPortal-AddCourse"
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
                    AdvancedLearnerLoan = model.AdvancedLearnerLoan,

                    CourseRuns = courseRuns,

                    CreatedDate = DateTime.Now,
                    CreatedBy = "ProviderPortal-AddCourse" // TODO - Change to the name of the logged person
                };

                var result = await _courseService.AddCourseAsync(course);

                RemoveSessionVariables();

                if (result.IsSuccess && result.HasValue)
                {
                    return RedirectToAction("Index",
                        new { status = "good", learnAimRef = learnAimRef, numberOfNewCourses = courseRuns?.Count });
                }

                return RedirectToAction("Index",
                    new { status = "bad", learnAimRef = learnAimRef, errmsg = result.Error });

            }
        }

        [HttpPost]
        public IActionResult Publish(AddCourseRequestModel model)
        {
            _session.SetObject(SessionAddCourseSection2, model);
            var section1 = _session.GetObject<AddCourseSection1RequestModel>(SessionAddCourseSection1);
            var availableVenues = _session.GetObject<SelectVenueModel>(SessionVenues);
            var availableRegions = _session.GetObject<SelectRegionModel>(SessionRegions);

            string startDate;
            if (model.StartDateType == "FlexibleStartDate")
            {
                startDate = "Flexible";
            }
            else
            {
                startDate = model.Day + "/" + model.Month + "/" + model.Year;
            }

            var venues = new List<string>();
            if (model.DeliveryMode == DeliveryMode.ClassroomBased)
            {
                venues.AddRange(from summaryVenueVenueItem in availableVenues.VenueItems
                    from modelSelectedVenue in model.SelectedVenues
                    where modelSelectedVenue.ToString() == summaryVenueVenueItem.Id
                    select summaryVenueVenueItem.VenueName);
            }

            var regions = new List<string>();
            if (model.DeliveryMode == DeliveryMode.WorkBased)
            {
                regions.AddRange(from region in availableRegions.RegionItems
                    from modelSelectedRegion in model.SelectedRegions
                    where modelSelectedRegion == region.Id
                    select region.RegionName);
            }

            var summaryViewModel = new AddCourseSummaryViewModel
            {
                LearnAimRef = section1.LearnAimRef,
                NotionalNVQLevelv2 = section1.NotionalNVQLevelv2,
                LearnAimRefTitle = section1.LearnAimRefTitle,
                CourseName = section1.LearnAimRefTitle,
                CourseId = model.CourseProviderReference,
                DeliveryMode = model.DeliveryMode.ToDescription(),
                DeliveryModeEnum = model.DeliveryMode,
                StartDate = startDate,

                Venues = venues,
                Regions = regions,

                Url = model.Url,
                Cost = model.Cost == 0 ? string.Empty : model.Cost.ToString(CultureInfo.InvariantCulture),
                CostDescription = model.CostDescription,
                AdvancedLearnerLoan = model.AdvancedLearnerLoan ? "Available" : "Unavailable",
                CourseLength = model.DurationLength + " " + model.DurationUnit,
                AttendancePattern = model.StudyMode.ToDescription(),
                AttendanceTime = model.AttendanceMode.ToDescription(),
                WhoIsThisCourseFor = section1.CourseFor,
                EntryRequirements = section1.EntryRequirements,
                WhatYouWillLearn = section1.WhatWillLearn,
                WhereNext = section1.WhereNext,
                WhatYouWillNeedToBring = section1.WhatYouNeed,
                HowAssessed = section1.HowAssessed,
                HowYouWillLearn = section1.HowYouWillLearn
            };

            return View("SummaryPage", summaryViewModel);
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
                        RegionName = "North East"
                    },
                    new RegionItemModel
                    {
                        Id = "E12000002",
                        Checked = false,
                        RegionName = "North West"
                    },
                    new RegionItemModel
                    {
                        Id = "E12000003",
                        Checked = false,
                        RegionName = "Yorkshire and The Humber"
                    },
                    new RegionItemModel
                    {
                        Id = "E12000004",
                        Checked = false,
                        RegionName = "East Midlands"
                    },
                    new RegionItemModel
                    {
                        Id = "E12000005",
                        Checked = false,
                        RegionName = "West Midlands"
                    },
                    new RegionItemModel
                    {
                        Id = "E12000006",
                        Checked = false,
                        RegionName = "East of England"
                    },
                    new RegionItemModel
                    {
                        Id = "E12000007",
                        Checked = false,
                        RegionName = "London"
                    },
                    new RegionItemModel
                    {
                        Id = "E12000008",
                        Checked = false,
                        RegionName = "South East"
                    },
                    new RegionItemModel
                    {
                        Id = "E12000009",
                        Checked = false,
                        RegionName = "South West"
                    }
                    //,
                    //new RegionItemModel
                    //{
                    //    Id = "L99999999",
                    //    Checked = false,
                    //    RegionName = "(pseudo) Channel Islands"
                    //},
                    //new RegionItemModel
                    //{
                    //    Id = "M99999999",
                    //    Checked = false,
                    //    RegionName = "(pseudo) Isle of Man"
                    //},
                    //new RegionItemModel
                    //{
                    //    Id = "N99999999",
                    //    Checked = false,
                    //    RegionName = "(pseudo) Northern Ireland"
                    //},
                    //new RegionItemModel
                    //{
                    //    Id = "S99999999",
                    //    Checked = false,
                    //    RegionName = "(pseudo) Scotland"
                    //},
                    //new RegionItemModel
                    //{
                    //    Id = "W99999999",
                    //    Checked = false,
                    //    RegionName = "(pseudo) Wales"
                    //}
                }
            };
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
            viewModel.SelectRegion = GetRegions();

            _session.SetObject(SessionVenues, viewModel.SelectVenue);
            _session.SetObject(SessionRegions, viewModel.SelectRegion);

            if (addCourseSection2Session != null)
            {
                viewModel.CourseProviderReference = addCourseSection2Session.CourseProviderReference;
                viewModel.DeliveryMode = addCourseSection2Session.DeliveryMode;
                viewModel.StartDateType = (StartDateType)Enum.Parse(typeof(StartDateType), addCourseSection2Session.StartDateType);
                viewModel.Day = Convert.ToInt32(addCourseSection2Session.Day);
                viewModel.Month = Convert.ToInt32(addCourseSection2Session.Month);
                viewModel.Year = Convert.ToInt32(addCourseSection2Session.Year);
                viewModel.Url = addCourseSection2Session.Url;
                viewModel.Cost = addCourseSection2Session.Cost == 0 ? string.Empty : addCourseSection2Session.Cost.ToString(CultureInfo.InvariantCulture);
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
                    LabelText = "Who is the course for?",
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
                    LabelText = "Where next?",
                    HintText =
                        "What are the opportunities beyond this course? Progression to a higher level course, apprenticeship or direct entry to employment?",
                    AriaDescribedBy = "Please enter 'Where next?'"
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
            }

            return courseViewModel;
        }
    }
}