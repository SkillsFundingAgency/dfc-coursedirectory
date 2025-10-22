using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Extensions;
using Dfc.CourseDirectory.Core.Helpers;
using Dfc.CourseDirectory.Core.Middleware;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Security;
using Dfc.CourseDirectory.Core.Services;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Models.Courses;
using Dfc.CourseDirectory.Services.Models.Regions;
using Dfc.CourseDirectory.WebV2.Extensions;
using Dfc.CourseDirectory.WebV2.ViewComponents.Courses.ChooseRegion;
using Dfc.CourseDirectory.WebV2.ViewComponents.Courses.CourseFor;
using Dfc.CourseDirectory.WebV2.ViewComponents.Courses.EntryRequirements;
using Dfc.CourseDirectory.WebV2.ViewComponents.Courses.FundingOptions;
using Dfc.CourseDirectory.WebV2.ViewComponents.Courses.HowAssessed;
using Dfc.CourseDirectory.WebV2.ViewComponents.Courses.HowYouWillLearn;
using Dfc.CourseDirectory.WebV2.ViewComponents.Courses.SelectVenue;
using Dfc.CourseDirectory.WebV2.ViewComponents.Courses.WhatWillLearn;
using Dfc.CourseDirectory.WebV2.ViewComponents.Courses.WhatYouNeed;
using Dfc.CourseDirectory.WebV2.ViewComponents.Courses.WhereNext;
using Dfc.CourseDirectory.WebV2.ViewComponents.RequestModels;
using Dfc.CourseDirectory.WebV2.ViewModels;
using Flurl;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Dfc.CourseDirectory.WebV2.Controllers
{
    public class AddCourseController : BaseController
    {
        private const string FindACourseUrlConfigName = "FindACourse:Url";

        private readonly ICourseService _courseService;
        private ISession Session => HttpContext.Session;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IProviderContextProvider _providerContextProvider;
        private readonly ICourseTypeService _courseTypeService;
        private readonly IWebRiskService _webRiskService;
        private readonly IConfiguration _configuration;

        public AddCourseController(
            ICourseService courseService,
            ISqlQueryDispatcher sqlQueryDispatcher,
            ICurrentUserProvider currentUserProvider,
            IProviderContextProvider providerContextProvider,
            ICourseTypeService courseTypeService,
            IWebRiskService webRiskService,
            IConfiguration configuration) : base(sqlQueryDispatcher)

        {
            _courseService = courseService ?? throw new ArgumentNullException(nameof(courseService));
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _currentUserProvider = currentUserProvider ?? throw new ArgumentNullException(nameof(currentUserProvider));
            _providerContextProvider = providerContextProvider;
            _courseTypeService = courseTypeService;
            _webRiskService = webRiskService;
            _configuration = configuration;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> AddCourse(string learnAimRef, string notionalNVQLevelv2, string awardOrgCode, string learnAimRefTitle, string learnAimRefTypeDesc, Guid? courseId)
        {
            RemoveSessionVariables();
            Session.Remove(SessionPublishedCourse);

            var nonLarsCourse = string.IsNullOrWhiteSpace(learnAimRef);

            if (nonLarsCourse)
            {
                Session.SetString(SessionNonLarsCourse, "true");
            }
            else
            {
                Session.SetString(SessionLearnAimRef, learnAimRef);
                Session.SetString(SessionNotionalNvqLevelV2, notionalNVQLevelv2 ?? string.Empty);
                Session.SetString(SessionAwardOrgCode, awardOrgCode ?? string.Empty);
                Session.SetString(SessionLearnAimRefTitle, learnAimRefTitle ?? string.Empty);
                Session.SetString(SessionLearnAimRefTypeDesc, learnAimRefTypeDesc ?? string.Empty);
                Session.SetString(SessionNonLarsCourse, "false");
            }

            AddCourseViewModel vm = await GetCourseViewModel(learnAimRef, notionalNVQLevelv2, awardOrgCode, learnAimRefTitle, courseId, nonLarsCourse);

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
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            int UKPRN = Session.GetInt32("UKPRN").Value;
            bool nonLarsCourse = IsCourseNonLars();            

            var viewModel = new AddCourseDetailsViewModel
            {
                ProviderUKPRN = UKPRN,
                SelectVenue = await GetVenuesForProvider(),
                ChooseRegion = new ChooseRegionModel
                {
                    Regions = _courseService.GetRegions(),
                    National = null

                },
                NonLarsCourse = nonLarsCourse,
                Sectors = await GetSectors()
            };            

            if (!nonLarsCourse)
            {
                viewModel.LearnAimRef = Session.GetString(SessionLearnAimRef);
                viewModel.LearnAimRefTitle = Session.GetString(SessionLearnAimRefTitle);
                viewModel.AwardOrgCode = Session.GetString(SessionAwardOrgCode);
                viewModel.NotionalNVQLevelv2 = Session.GetString(SessionNotionalNvqLevelV2);
                viewModel.CourseName = Session.GetString(SessionLearnAimRefTitle);
            }

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

                        foreach (var region in viewModel.ChooseRegion.Regions.RegionItems)
                        {
                            foreach (var subregion in region.SubRegion)
                            {
                                if (subregion.Id == selectedRegion.ToString())
                                {
                                    subregion.Checked = true;
                                }
                            }
                        }
                    }
                }

                if (nonLarsCourse)
                {
                    viewModel.CourseType = addCourseSection2Session.CourseType;
                    viewModel.SectorId = addCourseSection2Session.SectorId;
                    viewModel.SectorDescription = addCourseSection2Session.SectorDescription;
                    viewModel.EducationLevel = addCourseSection2Session.EducationLevel;
                    viewModel.AwardingBody = addCourseSection2Session.AwardingBody;
                }
            }
            else
            {
                viewModel.StudyMode = CourseStudyMode.FullTime;
                viewModel.AttendanceMode = CourseAttendancePattern.Daytime;
                viewModel.DurationUnit = CourseDurationUnit.Months;
                viewModel.DeliveryMode = CourseDeliveryMode.ClassroomBased;
                viewModel.StartDateType = StartDateType.SpecifiedStartDate;

                if (nonLarsCourse)
                {
                    viewModel.CourseType = CourseType.SkillsBootcamp;
                    viewModel.SectorId = DefaultSectorId;
                    viewModel.EducationLevel = EducationLevel.EntryLevel;
                }
            }

            Session.SetObject(SessionLastAddCoursePage, AddCoursePage.AddCourse);

            //Generate Live service URL accordingly based on current host
            string host = HttpContext.Request.Host.ToString();
            ViewBag.LiveServiceURL = LiveServiceURLHelper.GetLiveServiceURLFromHost(host) + "find-a-course/search";

            return View(viewModel);
        }

        [Authorize]        
        public IActionResult AddNewVenue(AddCourseRequestModel model)
        {
            // var model = new AddCourseRequestModel();
            // AddCourseRun - going to Summary
            //Session.SetObject(SessionAddCourseSection2, model);
            //Session.SetObject(SessionLastAddCoursePage, AddCoursePage.AddCourseRun);

            // AddCourseRun - going to Summary
            model.Url = HttpUtility.UrlDecode(model.Url);
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
                case CourseDeliveryMode.ClassroomBased:
                case CourseDeliveryMode.BlendedLearning:
                    if (model.SelectedVenues != null)
                    {
                        venues.AddRange(from summaryVenueVenueItem in availableVenues.VenueItems
                                        from modelSelectedVenue in model.SelectedVenues
                                        where modelSelectedVenue.ToString() == summaryVenueVenueItem.Id
                                        select summaryVenueVenueItem.VenueName);

                        summaryViewModel.Venues = venues;
                    }
                    break;
                case CourseDeliveryMode.WorkBased:
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


            return Json(new Url(Url.Action("Index", "AddVenue", new { returnUrl = Url.Action("SummaryToAddCourseRun", "AddCourse") }))
                .WithProviderContext(_providerContextProvider.GetProviderContext(withLegacyFallback: true))
                .ToString());

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
        public async Task<IActionResult> AddCourseRun(AddCourseRequestModel model)
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
                    : model.Day + "/" + model.Month + "/" + model.Year,
                CourseType = model.CourseType.ToDescription(),
                Sector = await GetSectorDescription(model.SectorId),
                EducationLevel = model.EducationLevel.ToDescription(),
                AwardingBody = model.AwardingBody,
                NonLarsCourse = IsCourseNonLars()
            };

            switch (model.DeliveryMode)
            {
                case CourseDeliveryMode.ClassroomBased:
                case CourseDeliveryMode.BlendedLearning:
                    venues.AddRange(from summaryVenueVenueItem in availableVenues.VenueItems
                                    from modelSelectedVenue in model.SelectedVenues
                                    where modelSelectedVenue.ToString() == summaryVenueVenueItem.Id
                                    select summaryVenueVenueItem.VenueName);

                    summaryViewModel.Venues = venues;
                    break;
                case CourseDeliveryMode.WorkBased:

                    if (model.National)
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
                    : addCourseRun.Day + "/" + addCourseRun.Month + "/" + addCourseRun.Year,
                NonLarsCourse = IsCourseNonLars()
            };

            // venues and regions
            var availableVenues = Session.GetObject<SelectVenueModel>(SessionVenues);
            var availableRegions = Session.GetObject<SelectRegionModel>(SessionRegions);

            var venues = new List<string>();
            var regions = new List<string>();

            switch (addCourseRun.DeliveryMode)
            {
                case CourseDeliveryMode.ClassroomBased:
                case CourseDeliveryMode.BlendedLearning:
                    venues.AddRange(from summaryVenueVenueItem in availableVenues.VenueItems
                                    from modelSelectedVenue in addCourseRun.SelectedVenues
                                    where modelSelectedVenue.ToString() == summaryVenueVenueItem.Id
                                    select summaryVenueVenueItem.VenueName);


                    break;
                case CourseDeliveryMode.WorkBased:
                    if (model.National)
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
            string learnAimRef = null;
            var notionalNvqLevelv2 = string.Empty;
            var awardOrgCode = string.Empty;
            var learnAimRefTitle = string.Empty;            

            var nonLarsCourse = IsCourseNonLars();
            if (!nonLarsCourse)
            {
                learnAimRef = Session.GetString(SessionLearnAimRef);
                notionalNvqLevelv2 = Session.GetString(SessionNotionalNvqLevelV2);
                awardOrgCode = Session.GetString(SessionAwardOrgCode);
                learnAimRefTitle = Session.GetString(SessionLearnAimRefTitle);                

                // TODO - Add error message, if use this check
                if (string.IsNullOrEmpty(learnAimRef) ||
                    string.IsNullOrEmpty(notionalNvqLevelv2) ||
                    string.IsNullOrEmpty(awardOrgCode) ||
                    string.IsNullOrEmpty(learnAimRefTitle)
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
            }

            var addCourseSection2 = Session.GetObject<AddCourseRequestModel>(SessionAddCourseSection2);
            var addCourseSection1 = Session.GetObject<AddCourseSection1RequestModel>(SessionAddCourseSection1);
            var courseFor = ASCIICodeHelper.ReplaceHexCodes(addCourseSection1.CourseFor);
            var entryRequirements = ASCIICodeHelper.ReplaceHexCodes(addCourseSection1.EntryRequirements);
            var whatWillLearn = ASCIICodeHelper.ReplaceHexCodes(addCourseSection1.WhatWillLearn);
            var howYouWillLearn = ASCIICodeHelper.ReplaceHexCodes(addCourseSection1.HowYouWillLearn);
            var whatYouNeed = ASCIICodeHelper.ReplaceHexCodes(addCourseSection1.WhatYouNeed);
            var howAssessed = ASCIICodeHelper.ReplaceHexCodes(addCourseSection1.HowAssessed);
            var whereNext = ASCIICodeHelper.ReplaceHexCodes(addCourseSection1.WhereNext);

            if (addCourseSection2.DeliveryMode == CourseDeliveryMode.ClassroomBased || addCourseSection2.DeliveryMode == CourseDeliveryMode.BlendedLearning)
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
            var courseRuns = new List<CreateCourseCourseRun>();

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

            if ((addCourseSection2.DeliveryMode == CourseDeliveryMode.ClassroomBased || addCourseSection2.DeliveryMode == CourseDeliveryMode.BlendedLearning)
                && addCourseSection2.SelectedVenues != null
                && addCourseSection2.SelectedVenues.Any())
            {
                foreach (var venue in addCourseSection2.SelectedVenues)
                {
                    var courseRun = new CreateCourseCourseRun
                    {
                        CourseRunId = Guid.NewGuid(),
                        VenueId = venue,
                        CourseName = addCourseSection2.CourseName,
                        ProviderCourseId = addCourseSection2.CourseProviderReference ?? "",
                        DeliveryMode = addCourseSection2.DeliveryMode,
                        FlexibleStartDate = flexibleStartDate,
                        StartDate = specifiedStartDate,
                        CourseUrl = addCourseSection2.Url,
                        Cost = addCourseSection2.Cost,
                        CostDescription = ASCIICodeHelper.ReplaceHexCodes(addCourseSection2.CostDescription) ?? "",
                        DurationUnit = addCourseSection2.DurationUnit.Value,
                        DurationValue = addCourseSection2.DurationLength,
                        StudyMode = addCourseSection2.StudyMode,
                        AttendancePattern = addCourseSection2.AttendanceMode
                    };

                    courseRuns.Add(courseRun);
                }
            }

            if (addCourseSection2.DeliveryMode == CourseDeliveryMode.WorkBased)
            {
                var courseRun = new CreateCourseCourseRun
                {
                    CourseRunId = Guid.NewGuid(),
                    CourseName = addCourseSection2.CourseName,
                    ProviderCourseId = addCourseSection2.CourseProviderReference ?? "",
                    DeliveryMode = CourseDeliveryMode.WorkBased,
                    FlexibleStartDate = flexibleStartDate,
                    StartDate = specifiedStartDate,
                    CourseUrl = addCourseSection2.Url,
                    Cost = addCourseSection2.Cost,
                    CostDescription = ASCIICodeHelper.ReplaceHexCodes(addCourseSection2.CostDescription) ?? "",
                    DurationUnit = addCourseSection2.DurationUnit.Value,
                    DurationValue = addCourseSection2.DurationLength
                };                

                if (addCourseSection2.National == false)
                {
                    if (addCourseSection2.SelectedRegions != null && addCourseSection2.SelectedRegions.Any())
                    {
                        courseRun.National = false;
                        courseRun.SubRegionIds = addCourseSection2.SelectedRegions;
                    }
                }
                else
                {
                    courseRun.National = true;
                }

                courseRuns.Add(courseRun);

            }

            if (addCourseSection2.DeliveryMode == CourseDeliveryMode.Online)
            {
                var courseRun = new CreateCourseCourseRun
                {
                    CourseRunId = Guid.NewGuid(),
                    CourseName = addCourseSection2.CourseName,
                    ProviderCourseId = addCourseSection2.CourseProviderReference ?? "",
                    DeliveryMode = addCourseSection2.DeliveryMode,
                    FlexibleStartDate = flexibleStartDate,
                    StartDate = specifiedStartDate,
                    CourseUrl = addCourseSection2.Url,
                    Cost = addCourseSection2.Cost,
                    CostDescription = ASCIICodeHelper.ReplaceHexCodes(addCourseSection2.CostDescription) ?? "",
                    DurationUnit = addCourseSection2.DurationUnit.Value,
                    DurationValue = addCourseSection2.DurationLength
                };

                courseRuns.Add(courseRun);
            }

            var courseId = Guid.NewGuid();
            var providerId = _providerContextProvider.GetProviderId(withLegacyFallback: true);

            var courseType = nonLarsCourse ? addCourseSection2.CourseType : await _courseTypeService.GetCourseType(learnAimRef, providerId);            

            await _sqlQueryDispatcher.ExecuteQuery(new CreateCourse()
            {
                CourseId = courseId,
                ProviderId = providerId,
                LearnAimRef = learnAimRef,
                WhoThisCourseIsFor = courseFor ?? "",
                EntryRequirements = entryRequirements ?? "",
                WhatYoullLearn = whatWillLearn ?? "",
                HowYoullLearn = howYouWillLearn ?? "",
                WhatYoullNeed = whatYouNeed ?? "",
                HowYoullBeAssessed = howAssessed ?? "",
                WhereNext = whereNext ?? "",
                CourseRuns = courseRuns,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = _currentUserProvider.GetCurrentUser(),
                CourseType = courseType,
                SectorId = addCourseSection2.SectorId,
                EducationLevel = addCourseSection2.EducationLevel,
                AwardingBody = addCourseSection2.AwardingBody
            });

            Session.SetObject(SessionPublishedCourse, new PublishedCourseViewModel
            {
                CourseId = courseId,
                CourseRunId = courseRuns[0].CourseRunId,
                CourseName = courseRuns[0].CourseName,
                NonLarsCourse = IsCourseNonLars()
            });

            RemoveSessionVariables();            

            return RedirectToAction("Published");
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
                return Redirect("/provider-search");
            }

            //Extract Live service URL from the environment variable
            ViewBag.LiveServiceURL = string.Format(_configuration[FindACourseUrlConfigName], publishedCourse.CourseId, publishedCourse.CourseRunId);

            return View(publishedCourse);
        }

        #region Private methods
        private async Task<SelectVenueModel> GetVenuesForProvider()
        {
            var providerContext = _providerContextProvider.GetProviderContext(withLegacyFallback: true);

            var selectVenue = new SelectVenueModel
            {
                LabelText = "Venue",
                AriaDescribedBy = "Select all that apply.",
                Ukprn = providerContext.ProviderInfo.Ukprn
            };

            var venues = await _sqlQueryDispatcher.ExecuteQuery(new GetVenuesByProvider() { ProviderId = providerContext.ProviderInfo.ProviderId });

            selectVenue.VenueItems = venues.Select(v => new VenueItemModel()
            {
                Id = v.VenueId.ToString(),
                VenueName = v.VenueName
            }).ToList();

            if (selectVenue.VenueItems.Count == 1)
            {
                selectVenue.HintText = string.Empty;
                selectVenue.AriaDescribedBy = string.Empty;
            }

            return selectVenue;
        }

        internal void RemoveSessionVariables()
        {
            Session.Remove(SessionLearnAimRef);
            Session.Remove(SessionNotionalNvqLevelV2);
            Session.Remove(SessionAwardOrgCode);
            Session.Remove(SessionLearnAimRefTitle);
            Session.Remove(SessionLearnAimRefTypeDesc);

            Session.Remove(SessionAddCourseSection2);
            Session.Remove(SessionLastAddCoursePage);
            Session.Remove(SessionNonLarsCourse);
        }

        private async Task<AddCourseViewModel> GetCourseViewModel(string learnAimRef, string notionalNVQLevelv2, string awardOrgCode, string learnAimRefTitle, Guid? courseId, bool nonLarsCourse = false)
        {
            Course course = null;
            CourseText defaultCourseText = null;

            if (courseId.HasValue)
            {
                course = await GetCourse(courseId, nonLarsCourse);
            }
            else if (!string.IsNullOrWhiteSpace(learnAimRef))
            {
                defaultCourseText = await _sqlQueryDispatcher.ExecuteQuery(new GetCourseTextByLearnAimRef { LearnAimRef = learnAimRef });
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
                        "The further opportunities a learner can expect after successfully completing the course. For example, a higher level course or entry to employment.",
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

            //Generate Live service URL accordingly based on current host
            string host = HttpContext.Request.Host.ToString();
            ViewBag.LiveServiceURL = LiveServiceURLHelper.GetLiveServiceURLFromHost(host) + "find-a-course/search";
            return vm;
        }

        private AddCourseViewModel GetSection1ViewModel()
        {
            var addCourseSection1 = Session.GetObject<AddCourseSection1RequestModel>(SessionAddCourseSection1);

            var courseViewModel = new AddCourseViewModel()
            {
                AwardOrgCode = Session.GetString(SessionAwardOrgCode),
                LearnAimRef = Session.GetString(SessionLearnAimRef),
                LearnAimRefTitle = Session.GetString(SessionLearnAimRefTitle),
                NotionalNVQLevelv2 = Session.GetString(SessionNotionalNvqLevelV2),
                CourseFor = new CourseForModel
                {
                    LabelText = "Who this course is for",
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
                    HintText = "What are the opportunities beyond this course? Progression to a higher level course or direct entry to employment?",
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

            bool nonLarsCourse = IsCourseNonLars();

            var viewModel = new AddCourseDetailsViewModel
            {
                ProviderUKPRN = UKPRN,
                SelectVenue = await GetVenuesForProvider(),
                ChooseRegion = new ChooseRegionModel
                {
                    Regions = _courseService.GetRegions(),
                    National = null

                },
                NonLarsCourse = nonLarsCourse,
                Sectors = await GetSectors()
            };

            if (!nonLarsCourse)
            {
                viewModel.LearnAimRef = Session.GetString(SessionLearnAimRef);
                viewModel.LearnAimRefTitle = Session.GetString(SessionLearnAimRefTitle);
                viewModel.AwardOrgCode = Session.GetString(SessionAwardOrgCode);
                viewModel.NotionalNVQLevelv2 = Session.GetString(SessionNotionalNvqLevelV2);
                viewModel.CourseName = Session.GetString(SessionLearnAimRefTitle);
            }

            viewModel.SelectVenue = await GetVenuesForProvider();
            viewModel.ChooseRegion.Regions = _courseService.GetRegions();

            Session.SetObject(SessionVenues, viewModel.SelectVenue);
            Session.SetObject(SessionRegions, viewModel.ChooseRegion.Regions);

            if (addCourseSection2Session != null)
            {
                viewModel.CourseName = addCourseSection2Session.CourseName;
                viewModel.CourseProviderReference = addCourseSection2Session.CourseProviderReference;
                viewModel.CourseType = nonLarsCourse ? addCourseSection2Session.CourseType : default;
                viewModel.SectorId = nonLarsCourse ? addCourseSection2Session.SectorId : null;
                viewModel.SectorDescription = nonLarsCourse ? addCourseSection2Session.SectorDescription : null;
                viewModel.EducationLevel = nonLarsCourse ? addCourseSection2Session.EducationLevel : default;
                viewModel.AwardingBody = nonLarsCourse ? addCourseSection2Session.AwardingBody : null;
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
                        viewModel.ChooseRegion.Regions.RegionItems.SelectMany(x => x.SubRegion)
                            .FirstOrDefault(sb => sb.Id == selectedRegion).Checked = true;
                    }
                }
            }
            else
            {
                viewModel.DurationUnit = CourseDurationUnit.Months;
                viewModel.StudyMode = CourseStudyMode.FullTime;
                viewModel.AttendanceMode = CourseAttendancePattern.Daytime;
                viewModel.DeliveryMode = CourseDeliveryMode.ClassroomBased;
                viewModel.StartDateType = StartDateType.SpecifiedStartDate;
                viewModel.CourseType = CourseType.SkillsBootcamp;
                viewModel.SectorId = DefaultSectorId;
                viewModel.EducationLevel = EducationLevel.EntryLevel;
            }
            return viewModel;
        }
        #endregion

    }
}
