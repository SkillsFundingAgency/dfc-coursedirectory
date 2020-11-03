using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.CourseTextService;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.CourseFor;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.EntryRequirements;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.HowAssessed;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.HowYouWillLearn;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.WhatWillLearn;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.WhatYouNeed;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.WhereNext;
using Dfc.CourseDirectory.Web.ViewModels;
using Dfc.CourseDirectory.Web.ViewModels.EditCourse;
using Dfc.CourseDirectory.WebV2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dfc.CourseDirectory.Web.Controllers.EditCourse
{
    public class EditCourseController : Controller
    {
        private readonly ILogger<EditCourseController> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ICourseService _courseService;
        private readonly ICourseTextService _courseTextService;

        private ISession _session => _contextAccessor.HttpContext.Session;

        public EditCourseController(
            ILogger<EditCourseController> logger,
            IOptions<CourseServiceSettings> courseSearchSettings,
            IHttpContextAccessor contextAccessor,
            ICourseService courseService, ICourseTextService courseTextService)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(courseSearchSettings, nameof(courseSearchSettings));
            Throw.IfNull(contextAccessor, nameof(contextAccessor));
            Throw.IfNull(courseService, nameof(courseService));
            Throw.IfNull(courseTextService, nameof(courseTextService));

            _logger = logger;
            _contextAccessor = contextAccessor;
            _courseService = courseService;
            _courseTextService = courseTextService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index(string learnAimRef, string notionalNVQLevelv2, string awardOrgCode, string learnAimRefTitle, string learnAimRefTypeDesc, Guid? courseId, Guid? courseRunId, bool fromBulkUpload, PublishMode mode)
        {

            int? UKPRN;

            if (_session.GetInt32("UKPRN") != null)
            {
                UKPRN = _session.GetInt32("UKPRN").Value;
            }
            else
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            if (courseId.HasValue)
            {
                var course = await _courseService.GetCourseByIdAsync(new GetCourseByIdCriteria(courseId.Value));

                if (course != null)
                {
                    EditCourseViewModel vm = new EditCourseViewModel
                    {
                        CourseName = course.Value.QualificationCourseTitle,
                        AwardOrgCode = awardOrgCode,
                        LearnAimRef = learnAimRef,
                        LearnAimRefTitle = learnAimRefTitle,
                        NotionalNVQLevelv2 = notionalNVQLevelv2,
                        CourseId = courseId,
                        CourseRunId = courseRunId,
                        Mode = mode,
                        CourseFor = new CourseForModel()
                        {
                            LabelText = "Who this course is for",
                            HintText = "Information that will help the learner decide whether this course is suitable for them, the learning experience and opportunities they can expect from the course.",
                            AriaDescribedBy = "Please enter who this course is for.",
                            CourseFor = course?.Value?.CourseDescription
                        },

                        EntryRequirements = new EntryRequirementsModel()
                        {
                            LabelText = "Entry requirements",
                            HintText = "Specific skills, licences, vocational or academic requirements. For example, DBS, driving licence, computer knowledge, literacy or numeracy requirements.",
                            AriaDescribedBy = "Please list entry requirements.",
                            EntryRequirements = course?.Value?.EntryRequirements
                        },
                        WhatWillLearn = new WhatWillLearnModel()
                        {
                            LabelText = "What you’ll learn",
                            HintText = "The main topics, units or modules of the course a learner can expect, include key features. For example, communication, team leadership and time management.",
                            AriaDescribedBy = "Please enter what will be learned.",
                            WhatWillLearn = course?.Value?.WhatYoullLearn

                        },
                        HowYouWillLearn = new HowYouWillLearnModel()
                        {
                            LabelText = "How you’ll learn",
                            HintText = "The methods used to deliver the course. For example, classroom based exercises, a work environment or online study materials.",
                            AriaDescribedBy = "Please enter how you’ll learn.",
                            HowYouWillLearn = course?.Value?.HowYoullLearn
                        },
                        WhatYouNeed = new WhatYouNeedModel()
                        {
                            LabelText = "What you’ll need to bring",
                            HintText = "What the learner will need to access or bring to the course. For example, personal protective clothing, tools, devices or internet access.",
                            AriaDescribedBy = "Please enter what you need.",
                            WhatYouNeed = course?.Value?.WhatYoullNeed

                        },
                        HowAssessed = new HowAssessedModel()
                        {
                            LabelText = "How you’ll be assessed",
                            HintText = "The ways a learner will be assessed. For example, workplace assessment, written assignments, exams, group or individual project work or portfolio of evidence.",
                            AriaDescribedBy = "Please enter how you’ll be assessed.",
                            HowAssessed = course?.Value?.HowYoullBeAssessed
                        },
                        WhereNext = new WhereNextModel()
                        {
                            LabelText = "What you can do next",
                            HintText = "The further opportunities a learner can expect after successfully completing the course. For example, a higher level course, apprenticeship or entry to employment.",
                            AriaDescribedBy = "Please enter what you can do next.",
                            WhereNext = course?.Value?.WhereNext
                        }
                    };
                    vm.QualificationType = course?.Value?.QualificationType;
                    vm.AdultEducationBudget = course?.Value?.AdultEducationBudget;
                    vm.AdvancedLearnerLoan = course?.Value?.AdvancedLearnerLoan;
                    vm.NotionalNVQLevelv2 = course?.Value?.NotionalNVQLevelv2;

                    return View("EditCourse", vm);
                }
            }

            //error page
            return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Index(EditCourseSaveViewModel model)
        {
            if (model.CourseId.HasValue)
            {
                var courseForEdit = await _courseService.GetCourseByIdAsync(new GetCourseByIdCriteria(model.CourseId.Value));

                if (courseForEdit.IsSuccess && courseForEdit.HasValue)
                {
                    courseForEdit.Value.CourseDescription = model?.CourseFor;
                    courseForEdit.Value.EntryRequirements = model?.EntryRequirements;
                    courseForEdit.Value.WhatYoullLearn = model?.WhatWillLearn;
                    courseForEdit.Value.HowYoullLearn = model?.HowYouWillLearn;
                    courseForEdit.Value.WhatYoullNeed = model?.WhatYouNeed;
                    courseForEdit.Value.HowYoullBeAssessed = model?.HowAssessed;
                    courseForEdit.Value.WhereNext = model?.WhereNext;
                    courseForEdit.Value.AdultEducationBudget = model.AdultEducationBudget;
                    courseForEdit.Value.AdvancedLearnerLoan = model.AdvancedLearnerLoan;
                    courseForEdit.Value.IsValid = true; // The same for Live, BulkUpload, Migration
                    courseForEdit.Value.UpdatedBy = User.Claims.Where(c => c.Type == "email").Select(c => c.Value).SingleOrDefault(); // User.Identity.Name;
                    courseForEdit.Value.UpdatedDate = DateTime.Now;

                    courseForEdit.Value.ValidationErrors = _courseService.ValidateCourse(courseForEdit.Value).Select(x => x.Value);

                    if (model.Mode == PublishMode.Migration)
                    {
                        if (!(courseForEdit.Value.ValidationErrors != null && courseForEdit.Value.ValidationErrors.Any()))
                        {
                            // Change courseruns status of MigrationReadyToGoLive to Live so the entire course can go live
                            foreach (var courseRun in courseForEdit.Value.CourseRuns.Where(x => x.RecordStatus == RecordStatus.MigrationReadyToGoLive))
                            {
                                courseRun.RecordStatus = RecordStatus.Live;
                            }
                        }
                    }

                    var message = string.Empty;
                    bool isCourseValid = !(courseForEdit.Value.ValidationErrors != null && courseForEdit.Value.ValidationErrors.Any());

                    RecordStatus[] validStatuses = new[] { RecordStatus.MigrationReadyToGoLive, RecordStatus.Live };

                    if (isCourseValid && !(courseForEdit.Value.CourseRuns.Where(x => !validStatuses.Contains(x.RecordStatus)).Any()))
                    {
                        // Course run was fixed
                        message = $"'{courseForEdit.Value.QualificationCourseTitle}' was successfully fixed and published.";
                    }
                    else
                    {
                        // Course was fixed
                        message = $"'{courseForEdit.Value.QualificationCourseTitle}' was successfully fixed.";
                    }

                    var updatedCourse = await _courseService.UpdateCourseAsync(courseForEdit.Value);

                    switch (model.Mode)
                    {

                        case PublishMode.Migration:
                           return RedirectToAction("Index", "PublishCourses",
                                new
                                {
                                    publishMode = model.Mode,
                                    courseId = model.CourseId,
                                    notificationTitle = message
                                });
                        case PublishMode.BulkUpload:
                            return RedirectToAction("Index", "PublishCourses",
                                new
                                {
                                    publishMode = model.Mode,
                                    courseId = model.CourseId
                                });
                        case PublishMode.Summary:
                            TempData[TempDataKeys.ShowCourseUpdatedNotification] = true;
                            return RedirectToAction("Index", "CourseSummary",
                                new
                                {
                                    courseId = model.CourseId,
                                    courseRunId = model.CourseRunId
                                });
                        default:
                            return RedirectToAction("Courses", "Provider",
                                new
                                {
                                    NotificationTitle = "Course edited",
                                    NotificationMessage = "You edited",
                                    qualificationType = courseForEdit.Value.QualificationType,
                                    courseId = updatedCourse.Value.id
                                });
                    }
                }
            }

            return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}