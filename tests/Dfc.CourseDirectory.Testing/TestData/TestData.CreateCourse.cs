using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Testing
{
    public partial class TestData
    {
        public async Task<Course> CreateCourse(
            Guid providerId,
            UserInfo createdBy,
            string learnAimRef = null,
            string whoThisCourseIsFor = "The description",
            string entryRequirements = "To be eligible for ESFA government funding, you must be 19 or over. You must be a UK or EU citizen or have indefinite leave to remain in the UK. Learners will be ineligible if they have previously achieved a GCSE grade A* to C in the subject.",
            string whatYoullLearn = "DIGITAL EMPLOYABILITY SKILLS",
            string howYoullLearn = "Group coaching and 1-2-1 support through Tutor led learning, written assignments throughout courses, Independent Learning, access to comprehensive online and paper based learning resources.",
            string whatYoullNeed = "Valid ID (driving licence, passport, birth certificate)",
            string howYoullBeAssessed = "Paper based or online assessments",
            string whereNext = "We will actively signpost and support you to achieve the next level of progression for all certificated aims.",
            DateTime? createdUtc = null,
            Action<CreateCourseCourseRunBuilder> configureCourseRuns = null)
        {
            if (learnAimRef == null)
            {
                var learningDelivery = await CreateLearningDelivery();
                learnAimRef = learningDelivery.LearnAimRef;
            }

            var courseId = Guid.NewGuid();

            var courseRunBuilder = new CreateCourseCourseRunBuilder();

            configureCourseRuns ??= builder => builder.WithOnlineCourseRun();
            configureCourseRuns.Invoke(courseRunBuilder);

            if (courseRunBuilder.CourseRuns.Count == 0)
            {
                throw new ArgumentException("At least one CourseRun must be specified.", nameof(configureCourseRuns));
            }

            var courseRuns = courseRunBuilder.CourseRuns;

            return await WithSqlQueryDispatcher(async dispatcher =>
            {
                await dispatcher.ExecuteQuery(
                    new CreateCourse()
                    {
                        CourseId = courseId,
                        ProviderId = providerId,
                        LearnAimRef = learnAimRef,
                        WhoThisCourseIsFor = whoThisCourseIsFor,
                        EntryRequirements = entryRequirements,
                        WhatYoullLearn = whatYoullLearn,
                        HowYoullLearn = howYoullLearn,
                        WhatYoullNeed = whatYoullNeed,
                        HowYoullBeAssessed = howYoullBeAssessed,
                        WhereNext = whereNext,
                        CourseRuns = courseRuns,
                        CreatedOn = createdUtc ?? _clock.UtcNow,
                        CreatedBy = createdBy
                    });

                var course = await dispatcher.ExecuteQuery(new GetCourse() { CourseId = courseId });

                return course;
            });
        }

        public class CreateCourseCourseRunBuilder
        {
            private readonly List<CreateCourseCourseRun> _courseRuns = new List<CreateCourseCourseRun>();

            public IReadOnlyCollection<CreateCourseCourseRun> CourseRuns => _courseRuns;

            public CreateCourseCourseRunBuilder WithCourseRun(
                    string courseName = "Education assessment in Maths",
                    bool? flexibleStartDate = null,
                    DateTime? startDate = null,
                    string courseUrl = null,
                    decimal? cost = 69m,
                    string costDescription = null,
                    CourseDurationUnit durationUnit = CourseDurationUnit.Months,
                    int durationValue = 3,
                    string providerCourseRef = null) =>
                WithOnlineCourseRun(courseName, flexibleStartDate, startDate, courseUrl, cost, costDescription, durationUnit, durationValue, providerCourseRef);

            public CreateCourseCourseRunBuilder WithClassroomBasedCourseRun(
                Guid venueId,
                CourseAttendancePattern attendancePattern = CourseAttendancePattern.Evening,
                CourseStudyMode studyMode = CourseStudyMode.PartTime,
                string courseName = "Education assessment in Maths",
                bool? flexibleStartDate = null,
                DateTime? startDate = null,
                string courseUrl = null,
                decimal? cost = 69m,
                string costDescription = null,
                CourseDurationUnit durationUnit = CourseDurationUnit.Months,
                int durationValue = 3,
                string providerCourseRef = null)
            {
                var courseRunId = Guid.NewGuid();

                _courseRuns.Add(new CreateCourseCourseRun()
                {
                    CourseRunId = courseRunId,
                    CourseName = courseName,
                    DeliveryMode = CourseDeliveryMode.ClassroomBased,
                    FlexibleStartDate = flexibleStartDate ?? !startDate.HasValue,
                    StartDate = startDate,
                    CourseUrl = courseUrl,
                    Cost = cost,
                    CostDescription = costDescription,
                    DurationUnit = durationUnit,
                    DurationValue = durationValue,
                    VenueId = venueId,
                    AttendancePattern = attendancePattern,
                    StudyMode = studyMode,
                    ProviderCourseId = providerCourseRef
                });

                return this;
            }

            public CreateCourseCourseRunBuilder WithOnlineCourseRun(
                string courseName = "Education assessment in Maths",
                bool? flexibleStartDate = null,
                DateTime? startDate = null,
                string courseUrl = null,
                decimal? cost = 69m,
                string costDescription = null,
                CourseDurationUnit durationUnit = CourseDurationUnit.Months,
                int durationValue = 3,
                string providerCourseRef = null,
                CourseStatus status = CourseStatus.Live)
            {
                var courseRunId = Guid.NewGuid();

                _courseRuns.Add(new CreateCourseCourseRun()
                {
                    CourseRunId = courseRunId,
                    CourseName = courseName,
                    DeliveryMode = CourseDeliveryMode.Online,
                    FlexibleStartDate = flexibleStartDate ?? !startDate.HasValue,
                    StartDate = startDate,
                    CourseUrl = courseUrl,
                    Cost = cost,
                    CostDescription = costDescription,
                    DurationUnit = durationUnit,
                    DurationValue = durationValue,
                    National = true,
                    ProviderCourseId = providerCourseRef
                });

                return this;
            }

            public CreateCourseCourseRunBuilder WithWorkBasedCourseRun(
                bool national = true,
                IEnumerable<string> subRegionIds = null,
                string courseName = "Education assessment in Maths",
                bool? flexibleStartDate = null,
                DateTime? startDate = null,
                string courseUrl = null,
                decimal? cost = 69m,
                string costDescription = null,
                CourseDurationUnit durationUnit = CourseDurationUnit.Months,
                int durationValue = 3,
                string providerCourseRef = null)
            {
                var courseRunId = Guid.NewGuid();

                _courseRuns.Add(new CreateCourseCourseRun()
                {
                    CourseRunId = courseRunId,
                    CourseName = courseName,
                    DeliveryMode = CourseDeliveryMode.WorkBased,
                    FlexibleStartDate = flexibleStartDate ?? !startDate.HasValue,
                    StartDate = startDate,
                    CourseUrl = courseUrl,
                    Cost = cost,
                    CostDescription = costDescription,
                    DurationUnit = durationUnit,
                    DurationValue = durationValue,
                    National = national,
                    SubRegionIds = subRegionIds ?? Array.Empty<string>(),
                    ProviderCourseId = providerCourseRef
                });

                return this;
            }
        }
    }
}
