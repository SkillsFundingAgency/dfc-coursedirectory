using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.CourseTextService;
using Dfc.CourseDirectory.Services.Models;
using Dfc.CourseDirectory.Services.Models.Courses;
using Dfc.CourseDirectory.Services.Models.Regions;
using Dfc.CourseDirectory.Services.VenueService;
using Dfc.CourseDirectory.Web.Extensions;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.ChooseRegion;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.CourseFor;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.EntryRequirements;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.FundingOptions;
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
using Microsoft.Extensions.Options;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class AddCourseController : Controller
    {
        private readonly HtmlEncoder _htmlEncoder;
        private readonly ICourseService _courseService;

        private ISession Session => HttpContext.Session;
        private readonly IVenueSearchHelper _venueSearchHelper;
        private readonly IVenueService _venueService;
        private readonly ICourseTextService _courseTextService;

        private const string SessionVenues = "Venues";
        private const string SessionRegions = "Regions";
        private const string SessionAddCourseSection1 = "AddCourseSection1";
        private const string SessionAddCourseSection2 = "AddCourseSection2";
        private const string SessionLastAddCoursePage = "LastAddCoursePage";
        private const string SessionSummaryPageLoadedAtLeastOnce = "SummaryLoadedAtLeastOnce";
        private const string SessionPublishedCourse = "PublishedCourse";

        public AddCourseController(
            IOptions<CourseServiceSettings> courseSearchSettings,
            HtmlEncoder htmlEncoder,
            ICourseService courseService, IVenueSearchHelper venueSearchHelper, IVenueService venueService, ICourseTextService courseTextService)
        {
            if (courseSearchSettings == null)
            {
                throw new ArgumentNullException(nameof(courseSearchSettings));
            }

            if (courseService == null)
            {
                throw new ArgumentNullException(nameof(courseService));
            }

            if (venueService == null)
            {
                throw new ArgumentNullException(nameof(venueService));
            }

            if (courseTextService == null)
            {
                throw new ArgumentNullException(nameof(courseTextService));
            }

            _htmlEncoder = htmlEncoder;
            _courseService = courseService;
            _venueService = venueService;
            _venueSearchHelper = venueSearchHelper;
            _courseTextService = courseTextService;
        }

        [Authorize]
        [HttpGet]
        public IActionResult AddCourse(string learnAimRef, string notionalNVQLevelv2, string awardOrgCode, string learnAimRefTitle, string learnAimRefTypeDesc, Guid? courseId)
        {
            RemoveSessionVariables();

            Session.SetString("LearnAimRef", learnAimRef);
            Session.SetString("NotionalNVQLevelv2", notionalNVQLevelv2);
            Session.SetString("AwardOrgCode", awardOrgCode);
            Session.SetString("LearnAimRefTitle", learnAimRefTitle);
            Session.SetString("LearnAimRefTypeDesc", learnAimRefTypeDesc);

            Course course = null;
            CourseText defaultCourseText = null;

            if (courseId.HasValue)
            {
                course = _courseService.GetCourseByIdAsync(new GetCourseByIdCriteria(courseId.Value)).Result.Value;
            }
            else
            {
                defaultCourseText = _courseTextService.GetCourseTextByLARS(new CourseTextSearchCriteria(learnAimRef)).Result.Value;
            }

            AddCourseViewModel vm = new AddCourseViewModel
            {
                AwardOrgCode = awardOrgCode,
                LearnAimRef = learnAimRef,
                LearnAimRefTitle = learnAimRefTitle,
                NotionalNVQLevelv2 = notionalNVQLevelv2,
                CourseFor = new CourseForModel
                {
                    LabelText = "Who this course is for",
                    HintText =
                        "Information that will help the learner decide whether this course is suitable for them, the learning experience and opportunities they can expect from the course.",
                    AriaDescribedBy = "Please enter who this course is for.",
                    CourseFor = course?.CourseDescription ?? defaultCourseText?.CourseDescription
                },

                EntryRequirements = new EntryRequirementsModel
                {
                    LabelText = "Entry requirements",
                    HintText =
                        "Specific skills, licences, vocational or academic requirements. For example, DBS, driving licence, computer knowledge, literacy or numeracy requirements.",
                    AriaDescribedBy = "Please list entry requirements.",
                    EntryRequirements = course?.EntryRequirements ?? defaultCourseText?.EntryRequirements
                },
                WhatWillLearn = new WhatWillLearnModel()
                {
                    LabelText = "What you’ll learn",
                    HintText = "The main topics, units or modules of the course a learner can expect, include key features. For example, communication, team leadership and time management.",
                    AriaDescribedBy = "Please enter what will be learned",
                    WhatWillLearn = course?.WhatYoullLearn ?? defaultCourseText?.WhatYoullLearn
                },
                HowYouWillLearn = new HowYouWillLearnModel()
                {
                    LabelText = "How you’ll learn",
                    HintText = "The methods used to deliver the course. For example, classroom based exercises, a work environment or online study materials.",
                    AriaDescribedBy = "Please enter how you’ll learn",
                    HowYouWillLearn = course?.HowYoullLearn ?? defaultCourseText?.HowYoullLearn
                },
                WhatYouNeed = new WhatYouNeedModel()
                {
                    LabelText = "What you’ll need to bring",
                    HintText =
                        "What the learner will need to access or bring to the course. For example, personal protective clothing, tools, devices or internet access.",
                    AriaDescribedBy = "Please enter what you need",
                    WhatYouNeed = course?.WhatYoullNeed ?? defaultCourseText?.WhatYoullNeed
                },
                HowAssessed = new HowAssessedModel()
                {
                    LabelText = "How you'll be assessed",
                    HintText =
                        "The ways a learner will be assessed. For example, workplace assessment, written assignments, exams, group or individual project work or portfolio of evidence.",
                    AriaDescribedBy = "Please enter 'How you’ll be assessed'",
                    HowAssessed = course?.HowYoullBeAssessed ?? defaultCourseText?.HowYoullBeAssessed
                },
                WhereNext = new WhereNextModel()
                {
                    LabelText = "What you can do next",
                    HintText =
                        "The further opportunities a learner can expect after successfully completing the course. For example, a higher level course, apprenticeship or entry to employment.",
                    AriaDescribedBy = "Please enter 'What you can do next'",
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
            Session.SetObject("AddCourseViewModel", vm);




            if (courseId.HasValue)
            {
                vm.CourseId = courseId.Value;
            }
            var addcoursesteponevalues = Session.GetObject<AddCourseSection1RequestModel>(SessionAddCourseSection1);
            var DetailViewModel = Session.GetObject<AddCourseViewModel>("AddCourseViewModel");

            if (addcoursesteponevalues != null && DetailViewModel != null)
            {
                vm.EntryRequirements.EntryRequirements = addcoursesteponevalues.EntryRequirements;
                vm.CourseFor.CourseFor = addcoursesteponevalues.CourseFor;
                vm.WhatWillLearn.WhatWillLearn = addcoursesteponevalues.WhatWillLearn;
                vm.HowYouWillLearn.HowYouWillLearn = addcoursesteponevalues.HowYouWillLearn;
                vm.WhatYouNeed.WhatYouNeed = addcoursesteponevalues.WhatYouNeed;
                vm.HowAssessed.HowAssessed = addcoursesteponevalues.HowAssessed;
                vm.WhereNext.WhereNext = addcoursesteponevalues.WhereNext;

            }

            return View(vm);
        }

        [Authorize]
        [HttpPost]
        public IActionResult AddCourse(AddCourseSection1RequestModel model)
        {
            // to AddCourseRun or Summary

            Session.SetObject(SessionLastAddCoursePage, AddCoursePage.AddCourse);
            Session.SetObject(SessionAddCourseSection1, model);

            return RedirectToAction("AddCourseDetails");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> AddCourseDetails(AddCourseSection1RequestModel model)
        {
            var addCourseSection2Session = Session.GetObject<AddCourseRequestModel>(SessionAddCourseSection2);

            if (Session.GetInt32("UKPRN") == null)
            {
                return RedirectToAction("Index", "Home", new {errmsg = "Please select a Provider."});
            }
            int UKPRN = Session.GetInt32("UKPRN").Value;

            var viewModel = new AddCourseDetailsViewModel
            {
                LearnAimRef = Session.GetString("LearnAimRef"),
                LearnAimRefTitle = Session.GetString("LearnAimRefTitle"),
                AwardOrgCode = Session.GetString("AwardOrgCode"),
                NotionalNVQLevelv2 = Session.GetString("NotionalNVQLevelv2"),
                CourseName = Session.GetString("LearnAimRefTitle"),
                ProviderUKPRN = UKPRN,
                SelectVenue = await GetVenuesByUkprn(UKPRN),
                ChooseRegion = new ChooseRegionModel {
                    Regions = _courseService.GetRegions(),
                    National = null

                }
            };

            Session.SetObject(SessionVenues, viewModel.SelectVenue);
            Session.SetObject(SessionRegions, viewModel.ChooseRegion.Regions);

            if (addCourseSection2Session != null)
            {
                viewModel.CourseName = addCourseSection2Session.CourseName;
                viewModel.CourseProviderReference = addCourseSection2Session.CourseProviderReference;
                viewModel.DeliveryMode = addCourseSection2Session.DeliveryMode;

                if (!string.IsNullOrEmpty(addCourseSection2Session.StartDateType))
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
                viewModel.ChooseRegion.National = addCourseSection2Session.National;

                viewModel.DurationLength = addCourseSection2Session.DurationLength.ToString();

                viewModel.DurationUnit = addCourseSection2Session.DurationUnit;

                viewModel.StudyMode = addCourseSection2Session.StudyMode;
                viewModel.AttendanceMode = addCourseSection2Session.AttendanceMode;
                if (addCourseSection2Session.SelectedVenues != null) {
                    foreach (var selectedVenue in addCourseSection2Session.SelectedVenues) {
                        viewModel.SelectVenue.VenueItems.First(x => x.Id == selectedVenue.ToString()).Checked = true;
                    }
                }

                if (addCourseSection2Session.SelectedRegions != null) {
                    foreach (var selectedRegion in addCourseSection2Session.SelectedRegions) {

                        foreach(var region in viewModel.ChooseRegion.Regions.RegionItems)
                        {
                            foreach(var subregion in region.SubRegion)
                            {
                                if(subregion.Id == selectedRegion.ToString())
                                {
                                    subregion.Checked = true;
                                }
                            }
                        }
                    }
                }

            } else {
                viewModel.StudyMode = StudyMode.FullTime;
                viewModel.AttendanceMode = AttendancePattern.Daytime;
                viewModel.DurationUnit = DurationUnit.Months;
                viewModel.DeliveryMode = DeliveryMode.ClassroomBased;
                viewModel.StartDateType = StartDateType.SpecifiedStartDate;
            }

            Session.SetObject(SessionLastAddCoursePage, AddCoursePage.AddCourse);
            return View(viewModel);
        }


        [Authorize]
        //public IActionResult AddNewVenue(Guid[] projectId)
        public IActionResult AddNewVenue(AddCourseRequestModel model)
        {
           // var model = new AddCourseRequestModel();
            // AddCourseRun - going to Summary
            //Session.SetObject(SessionAddCourseSection2, model);
            //Session.SetObject(SessionLastAddCoursePage, AddCoursePage.AddCourseRun);
            Session.SetString("Option", "AddNewVenue");

            // AddCourseRun - going to Summary
            Session.SetObject(SessionAddCourseSection2, model);
            var addCourse = Session.GetObject<AddCourseSection1RequestModel>(SessionAddCourseSection1);
            var availableVenues = Session.GetObject<SelectVenueModel>(SessionVenues);
            var availableRegions = Session.GetObject<SelectRegionModel>(SessionRegions);

            var venues = new List<string>();
            var regions = new List<string>();

            // sort regions out
            model.SelectedRegions = availableRegions.SubRegionsDataCleanse(model.SelectedRegions?.ToList() ?? new List<string>());

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

                    if (model.SelectedVenues != null)
                    {
                        venues.AddRange(from summaryVenueVenueItem in availableVenues.VenueItems
                                        from modelSelectedVenue in model.SelectedVenues
                                        where modelSelectedVenue.ToString() == summaryVenueVenueItem.Id
                                        select summaryVenueVenueItem.VenueName);

                        summaryViewModel.Venues = venues;
                    }
                    break;
                case DeliveryMode.WorkBased:
                    regions.AddRange(from availableRegionsRegionItem in availableRegions.RegionItems
                                     from subRegionItemModel in availableRegionsRegionItem.SubRegion
                                     from modelSelectedRegion in model.SelectedRegions
                                     where modelSelectedRegion == subRegionItemModel.Id
                                     select subRegionItemModel.SubRegionName);


                    regions.AddRange(from availableRegionsRegionItem in availableRegions.RegionItems
                                     from modelSelectedRegion in model.SelectedRegions
                                     where modelSelectedRegion == availableRegionsRegionItem.Id
                                     select availableRegionsRegionItem.RegionName
                    );

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
            Session.SetObject(SessionLastAddCoursePage, AddCoursePage.AddCourse);


            return Json(Url.Action("AddVenue", "Venues"));

            //return RedirectToAction("AddVenue", "Venues");

        }

        [Authorize]
        [HttpGet]
        public IActionResult AddCourseRun()
        {

            return View();
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

            // sort regions out
            model.SelectedRegions = availableRegions.SubRegionsDataCleanse(model.SelectedRegions?.ToList() ?? new List<string>());

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
                National = model.National,
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

                    if(model.National)
                    {
                        regions.Add("National");
                    }
                    else
                    {
                        regions.AddRange(from availableRegionsRegionItem in availableRegions.RegionItems
                                         from subRegionItemModel in availableRegionsRegionItem.SubRegion
                                         from modelSelectedRegion in model.SelectedRegions
                                         where modelSelectedRegion == subRegionItemModel.Id
                                         select subRegionItemModel.SubRegionName);


                        regions.AddRange(from availableRegionsRegionItem in availableRegions.RegionItems
                                         from modelSelectedRegion in model.SelectedRegions
                                         where modelSelectedRegion == availableRegionsRegionItem.Id
                                         select availableRegionsRegionItem.RegionName
                        );
                    }
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
            Session.Remove("AddNewVenue");
            Session.Remove("Option");
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
                National = addCourseRun.National,
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
                    if(model.National)
                    {
                        regions.Add("National");
                    }
                    else
                    {
                        regions.AddRange(from region in availableRegions.RegionItems
                                         from modelSelectedRegion in addCourseRun.SelectedRegions
                                         where modelSelectedRegion == region.Id
                                         select region.RegionName);
                    }

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
            Session.Remove("NewAddedVenue");
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
        [ValidateAntiForgeryToken] //Harden for CSRF
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

                        CourseName = _htmlEncoder.Encode(addCourseSection2.CourseName),
                        ProviderCourseID = _htmlEncoder.Encode(addCourseSection2.CourseProviderReference??""),
                        DeliveryMode = addCourseSection2.DeliveryMode,
                        FlexibleStartDate = flexibleStartDate,
                        StartDate = specifiedStartDate,
                        CourseURL = addCourseSection2.Url,
                        Cost = addCourseSection2.Cost,
                        CostDescription = _htmlEncoder.Encode(addCourseSection2.CostDescription??""),
                        DurationUnit = addCourseSection2.DurationUnit,
                        DurationValue = addCourseSection2.DurationLength,
                        StudyMode = addCourseSection2.StudyMode,
                        AttendancePattern = addCourseSection2.AttendanceMode,
                        Regions = addCourseSection2.SelectedRegions,
                        CreatedDate = DateTime.Now,
                        CreatedBy = User.Claims.Where(c => c.Type == "email").Select(c => c.Value).SingleOrDefault(),
                        RecordStatus = RecordStatus.Live // TODO - To Be Decided
                    };

                    courseRuns.Add(courseRun);
                }
            }

            if (addCourseSection2.DeliveryMode == DeliveryMode.WorkBased)
            {
                var courseRun = new CourseRun
                {
                    id = Guid.NewGuid(),

                    CourseName = _htmlEncoder.Encode(addCourseSection2.CourseName),
                    ProviderCourseID = _htmlEncoder.Encode(addCourseSection2.CourseProviderReference??""),
                    DeliveryMode = addCourseSection2.DeliveryMode,
                    FlexibleStartDate = flexibleStartDate,
                    StartDate = specifiedStartDate,
                    CourseURL = addCourseSection2.Url,
                    Cost = addCourseSection2.Cost,
                    CostDescription = _htmlEncoder.Encode(addCourseSection2.CostDescription??""),
                    DurationUnit = addCourseSection2.DurationUnit,
                    DurationValue = addCourseSection2.DurationLength,
                    StudyMode = StudyMode.Undefined,
                    AttendancePattern = AttendancePattern.Undefined,
                    CreatedDate = DateTime.Now,
                    CreatedBy = User.Claims.Where(c => c.Type == "email").Select(c => c.Value).SingleOrDefault(),
                    RecordStatus = RecordStatus.Live, // TODO - To Be Decided
                };
                var availableRegions = new SelectRegionModel();

                if (addCourseSection2.National == false)
                {
                    if (addCourseSection2.SelectedRegions != null && addCourseSection2.SelectedRegions.Any())
                    {
                        courseRun.National = addCourseSection2.National;
                        courseRun.Regions = addCourseSection2.SelectedRegions;
                        string[] selectedRegions = availableRegions.SubRegionsDataCleanse(addCourseSection2.SelectedRegions.ToList());

                        courseRun.SubRegions = selectedRegions.Select(selectedRegion => availableRegions.GetSubRegionItemByRegionCode(selectedRegion)).ToList();
                    }
                }
                else
                {
                    courseRun.National = true;
                    //removed due to COUR-1552
                    //courseRun.Regions = availableRegions.RegionItems.Select(x => (string)x.Id).ToList();

                }

                courseRuns.Add(courseRun);

            }

            if (addCourseSection2.DeliveryMode == DeliveryMode.Online)
            {
                var courseRun = new CourseRun
                {
                    id = Guid.NewGuid(),

                    CourseName = _htmlEncoder.Encode(addCourseSection2.CourseName),
                    ProviderCourseID = _htmlEncoder.Encode(addCourseSection2.CourseProviderReference??""),
                    DeliveryMode = addCourseSection2.DeliveryMode,
                    FlexibleStartDate = flexibleStartDate,
                    StartDate = specifiedStartDate,
                    CourseURL = addCourseSection2.Url,
                    Cost = addCourseSection2.Cost,
                    CostDescription = _htmlEncoder.Encode(addCourseSection2.CostDescription??""),
                    DurationUnit = addCourseSection2.DurationUnit,
                    DurationValue = addCourseSection2.DurationLength,
                    StudyMode = StudyMode.Undefined,
                    AttendancePattern = AttendancePattern.Undefined,
                    Regions = _courseService.GetRegions().RegionItems.Select(x => x.Id),
                    CreatedDate = DateTime.Now,
                    CreatedBy = User.Claims.Where(c => c.Type == "email").Select(c => c.Value).SingleOrDefault(),
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
                CourseDescription = _htmlEncoder.Encode(courseFor??""),
                EntryRequirements = _htmlEncoder.Encode(entryRequirements??""),
                WhatYoullLearn = _htmlEncoder.Encode(whatWillLearn??""),
                HowYoullLearn = _htmlEncoder.Encode(howYouWillLearn??""),
                WhatYoullNeed = _htmlEncoder.Encode(whatYouNeed??""),
                HowYoullBeAssessed = _htmlEncoder.Encode(howAssessed??""),
                WhereNext = _htmlEncoder.Encode(whereNext??""),
                AdvancedLearnerLoan = advancedLearnerLoan,
                AdultEducationBudget = adultEducationBudget,
                IsValid = true,
                CourseRuns = courseRuns,

                CreatedDate = DateTime.Now,
                CreatedBy = User.Claims.Where(c => c.Type == "email").Select(c => c.Value).SingleOrDefault()

                //RecordStatus = RecordStatus.Live // TODO - To Be Decided
            };

            var result = await _courseService.AddCourseAsync(course);

            RemoveSessionVariables();

            if (result.IsSuccess)
            {
                Session.SetObject(SessionPublishedCourse, new PublishedCourseViewModel
                {
                    CourseId = course.id,
                    CourseRunId = courseRuns[0].id,
                    CourseName = courseRuns[0].CourseName
                });

                return RedirectToAction("Published");
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

            int UKPRN = 0;
            if (Session.GetInt32("UKPRN").HasValue)
            {
                UKPRN = Session.GetInt32("UKPRN").Value;
            }
            else
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            // set the last page
            Session.SetObject(SessionLastAddCoursePage, AddCoursePage.AddCourse);

            // old BackToAddCourseSection2
            return View("AddCourseDetails", addCourseRun);
        }

        [Authorize]
        [HttpGet]
        public IActionResult Published()
        {
            var publishedCourse = Session.GetObject<PublishedCourseViewModel>(SessionPublishedCourse);

            if (publishedCourse == null)
            {
                var ukprn = Session.GetInt32("UKPRN");
                return ukprn.HasValue
                    ? RedirectToAction("SearchProvider", "ProviderSearch", new { UKPRN = ukprn })
                    : RedirectToAction("Index", "SearchProvider");
            }

            Session.Remove(SessionPublishedCourse);

            return View(new PublishedCourseViewModel
            {
                CourseId = publishedCourse.CourseId,
                CourseRunId = publishedCourse.CourseRunId,
                CourseName = publishedCourse.CourseName
            });
        }

        #region Private methods
        private async Task<SelectVenueModel> GetVenuesByUkprn(int ukprn)
        {
            var selectVenue = new SelectVenueModel
            {
                LabelText = "Course location",
                AriaDescribedBy = "Select all that apply.",
                Ukprn = ukprn
            };
            var requestModel = new VenueSearchRequestModel { SearchTerm = ukprn.ToString() };
            var criteria = _venueSearchHelper.GetVenueSearchCriteria(requestModel);
            var result = await _venueService.SearchAsync(criteria);
            if (result.IsSuccess)
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


        internal void RemoveSessionVariables()
        {
            Session.Remove("LearnAimRef");
            Session.Remove("NotionalNVQLevelv2");
            Session.Remove("AwardOrgCode");
            Session.Remove("LearnAimRefTitle");
            Session.Remove("LearnAimRefTypeDesc");

          //  Session.Remove(SessionAddCourseSection1);
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
                    LabelText = "Who is the course for",
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
                    LabelText = "What you can do next",
                    HintText = "What are the opportunities beyond this course? Progression to a higher level course, apprenticeship or direct entry to employment?",
                    AriaDescribedBy = "Please enter 'What you can do next'"
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
                ProviderUKPRN = UKPRN,
                ChooseRegion = new ChooseRegionModel()
            };

            viewModel.SelectVenue = await GetVenuesByUkprn(UKPRN);
            viewModel.ChooseRegion.Regions = _courseService.GetRegions();

            Session.SetObject(SessionVenues, viewModel.SelectVenue);
            Session.SetObject(SessionRegions, viewModel.ChooseRegion.Regions);

            if (addCourseSection2Session != null)
            {
                viewModel.CourseName = addCourseSection2Session.CourseName;
                viewModel.CourseProviderReference = addCourseSection2Session.CourseProviderReference;
                viewModel.DeliveryMode = addCourseSection2Session.DeliveryMode;

                if (!string.IsNullOrEmpty(addCourseSection2Session.StartDateType))
                {
                    viewModel.StartDateType = (StartDateType)Enum.Parse(typeof(StartDateType), addCourseSection2Session.StartDateType);
                }

                viewModel.Day = addCourseSection2Session.Day;
                viewModel.Month = addCourseSection2Session.Month;
                viewModel.Year = addCourseSection2Session.Year;
                viewModel.ChooseRegion.National = addCourseSection2Session.National;
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
                        viewModel.SelectVenue.VenueItems.FirstOrDefault(x => x.Id == selectedVenue.ToString()).Checked = true;
                    }
                }
                if (addCourseSection2Session.SelectedRegions != null)
                {
                    foreach (var selectedRegion in addCourseSection2Session.SelectedRegions)
                    {
                        viewModel.ChooseRegion.Regions.RegionItems.SelectMany(x=>x.SubRegion)
                            .FirstOrDefault(sb=>sb.Id == selectedRegion).Checked = true;
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
