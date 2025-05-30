using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Helpers;
using Dfc.CourseDirectory.Services.Models;
using Dfc.CourseDirectory.Web.Validation;
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
using Dfc.CourseDirectory.WebV2.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OneOf.Types;

namespace Dfc.CourseDirectory.Web.Controllers.EditCourse
{
    public class EditCourseController : BaseController
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IClock _clock;        

        private ISession Session => HttpContext.Session;

        public EditCourseController(
            ISqlQueryDispatcher sqlQueryDispatcher,
            ICurrentUserProvider currentUserProvider,
            IClock clock) : base(sqlQueryDispatcher)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _currentUserProvider = currentUserProvider;
            _clock = clock;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index(string learnAimRef, string notionalNVQLevelv2, string awardOrgCode, string learnAimRefTitle, string learnAimRefTypeDesc, Guid? courseId, Guid? courseRunId, bool fromBulkUpload, PublishMode mode)
        {
            if (!Session.GetInt32("UKPRN").HasValue)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            if (!courseId.HasValue)
            {
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
                        
            var result = await GetCourse(courseId);            

            if (result == null)
            {
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }

            var vm = new EditCourseViewModel
            {
                CourseName = result.LearnAimRefTitle,
                AwardOrgCode = awardOrgCode,
                LearnAimRef = learnAimRef,
                LearnAimRefTitle = learnAimRefTitle,
                NotionalNVQLevelv2 = result.NotionalNVQLevelv2,
                CourseId = courseId,
                CourseRunId = courseRunId,
                Mode = mode,
                CourseFor = new CourseForModel()
                {
                    LabelText = "Who this course is for",
                    HintText = "Information that will help the learner decide whether this course is suitable for them, the learning experience and opportunities they can expect from the course.",
                    AriaDescribedBy = "Please enter who this course is for.",
                    CourseFor = result.CourseDescription
                },

                EntryRequirements = new EntryRequirementsModel()
                {
                    LabelText = "Entry requirements",
                    HintText = "Specific skills, licences, vocational or academic requirements. For example, DBS, driving licence, computer knowledge, literacy or numeracy requirements.",
                    AriaDescribedBy = "Please list entry requirements.",
                    EntryRequirements = result.EntryRequirements
                },
                WhatWillLearn = new WhatWillLearnModel()
                {
                    LabelText = "What you’ll learn",
                    HintText = "The main topics, units or modules of the course a learner can expect, include key features. For example, communication, team leadership and time management.",
                    AriaDescribedBy = "Please enter what will be learned.",
                    WhatWillLearn = result.WhatYoullLearn

                },
                HowYouWillLearn = new HowYouWillLearnModel()
                {
                    LabelText = "How you’ll learn",
                    HintText = "The methods used to deliver the course. For example, classroom based exercises, a work environment or online study materials.",
                    AriaDescribedBy = "Please enter how you’ll learn.",
                    HowYouWillLearn = result.HowYoullLearn
                },
                WhatYouNeed = new WhatYouNeedModel()
                {
                    LabelText = "What you’ll need to bring",
                    HintText = "What the learner will need to access or bring to the course. For example, personal protective clothing, tools, devices or internet access.",
                    AriaDescribedBy = "Please enter what you need.",
                    WhatYouNeed = result.WhatYoullNeed

                },
                HowAssessed = new HowAssessedModel()
                {
                    LabelText = "How you’ll be assessed",
                    HintText = "The ways a learner will be assessed. For example, workplace assessment, written assignments, exams, group or individual project work or portfolio of evidence.",
                    AriaDescribedBy = "Please enter how you’ll be assessed.",
                    HowAssessed = result.HowYoullBeAssessed
                },
                WhereNext = new WhereNextModel()
                {
                    LabelText = "What you can do next",
                    HintText = "The further opportunities a learner can expect after successfully completing the course. For example, a higher level course or entry to employment.",
                    AriaDescribedBy = "Please enter what you can do next.",
                    WhereNext = result.WhereNext
                },
                QualificationType = result.LearnAimRefTypeDesc
            };

            //Generate Live service URL accordingly based on current host
            string host = HttpContext.Request.Host.ToString();
            ViewBag.LiveServiceURL = LiveServiceURLHelper.GetLiveServiceURLFromHost(host) + "find-a-course/search";

            return View("EditCourse", vm);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Index(EditCourseSaveViewModel model)
        {
            if (!model.CourseId.HasValue)
            {
                return BadRequest();
            }

            EditCourseSaveViewModel formattedModel = new EditCourseSaveViewModel
            {
                CourseFor = model.CourseFor?.Replace("\r\n", "\n"),
                EntryRequirements = model.EntryRequirements?.Replace("\r\n", "\n"),
                WhatWillLearn = model.WhatWillLearn?.Replace("\r\n", "\n"),
                HowYouWillLearn = model.HowYouWillLearn?.Replace("\r\n", "\n"),
                WhatYouNeed = model.WhatYouNeed?.Replace("\r\n", "\n"),
                HowAssessed = model.HowAssessed?.Replace("\r\n", "\n"),
                WhereNext = model.WhereNext?.Replace("\r\n", "\n"),
                AdultEducationBudget = model.AdultEducationBudget,
                AdvancedLearnerLoan = model.AdvancedLearnerLoan,
                CourseId = model.CourseId,
                CourseRunId = model.CourseRunId,
                Mode = model.Mode,
                CourseName = model.CourseName
            };

            var courseId = model.CourseId.Value;                        
            var course = await GetCourse(courseId);

            var validationResult = new EditCourseSaveViewModelValidator().Validate(formattedModel);
            if (!validationResult.IsValid)
            {
                return BadRequest();
            }

            var updateResult = await _sqlQueryDispatcher.ExecuteQuery(new UpdateCourse()
            {
                CourseId = courseId,
                WhoThisCourseIsFor = ASCIICodeHelper.ReplaceHexCodes(model.CourseFor),
                EntryRequirements = ASCIICodeHelper.ReplaceHexCodes(model.EntryRequirements),
                WhatYoullLearn = ASCIICodeHelper.ReplaceHexCodes(model.WhatWillLearn),
                HowYoullLearn = ASCIICodeHelper.ReplaceHexCodes(model.HowYouWillLearn),
                WhatYoullNeed = ASCIICodeHelper.ReplaceHexCodes(model.WhatYouNeed),
                HowYoullBeAssessed = ASCIICodeHelper.ReplaceHexCodes(model.HowAssessed),
                WhereNext = ASCIICodeHelper.ReplaceHexCodes(model.WhereNext),
                UpdatedBy = _currentUserProvider.GetCurrentUser(),
                UpdatedOn = _clock.UtcNow,
                CourseType = course.CourseType,
                SectorId = course.SectorId,
                EducationLevel = course.EducationLevel,
                AwardingBody = course.AwardingBody
            });

            if (!(updateResult.Value is Success))
            {
                return BadRequest();
            }

            course = await GetCourse(courseId);

            switch (model.Mode)
            {
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
                            qualificationType = course.LearnAimRefTypeDesc,
                            courseId = courseId
                        });
            }
        }        
    }
}
