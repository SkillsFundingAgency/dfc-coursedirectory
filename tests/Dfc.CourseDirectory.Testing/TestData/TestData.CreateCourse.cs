using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing.DataStore.CosmosDb.Queries;

namespace Dfc.CourseDirectory.Testing
{
    public partial class TestData
    {
        public async Task<Core.DataStore.Sql.Models.Course> CreateCourse(
            Guid providerId,
            UserInfo createdBy,
            string learnAimRef = null,
            string courseDescription = "The description",
            string entryRequirements = "To be eligible for ESFA government funding, you must be 19 or over. You must be a UK or EU citizen or have indefinite leave to remain in the UK. Learners will be ineligible if they have previously achieved a GCSE grade A* to C in the subject.",
            string whatYoullLearn = "DIGITAL EMPLOYABILITY SKILLS",
            string howYoullLearn = "Group coaching and 1-2-1 support through Tutor led learning, written assignments throughout courses, Independent Learning, access to comprehensive online and paper based learning resources.",
            string whatYoullNeed = "Valid ID (driving licence, passport, birth certificate)",
            string howYoullBeAssessed = "Paper based or online assessments",
            string whereNext = "We will actively signpost and support you to achieve the next level of progression for all certificated aims.",
            bool adultEducationBudget = false,
            bool advancedLearnerLoan = false,
            DateTime? createdUtc = null,
            Action<CreateCourseCourseRunBuilder> configureCourseRuns = null)
        {
            var provider = await _cosmosDbQueryDispatcher.ExecuteQuery(new Core.DataStore.CosmosDb.Queries.GetProviderById()
            {
                ProviderId = providerId
            });

            if (provider == null)
            {
                throw new ArgumentException("Provider does not exist.", nameof(providerId));
            }

            var learningDelivery = learnAimRef != null ?
                (await WithSqlQueryDispatcher(
                    dispatcher => dispatcher.ExecuteQuery(
                        new GetLearningDeliveries() { LearnAimRefs = new[] { learnAimRef } })))[learnAimRef] :
                await CreateLearningDelivery();

            var courseId = Guid.NewGuid();

            var courseRunBuilder = new CreateCourseCourseRunBuilder();

            configureCourseRuns ??= builder => builder.WithOnlineCourseRun();
            configureCourseRuns.Invoke(courseRunBuilder);

            if (courseRunBuilder.CourseRuns.Count == 0)
            {
                throw new InvalidOperationException("At least one CourseRun must be specified.");
            }

            var courseRuns = courseRunBuilder.CourseRuns;

            await _cosmosDbQueryDispatcher.ExecuteQuery(
                new CreateCourse()
                {
                    CourseId = courseId,
                    ProviderId = providerId,
                    ProviderUkprn = provider.Ukprn,
                    QualificationCourseTitle = learningDelivery.LearnAimRefTitle,
                    LearnAimRef = learningDelivery.LearnAimRef,
                    NotionalNVQLevelv2 = learningDelivery.NotionalNVQLevelv2,
                    AwardOrgCode = learningDelivery.AwardOrgCode,
                    QualificationType = learningDelivery.LearnAimRefTypeDesc,
                    CourseDescription = courseDescription,
                    EntryRequirements = entryRequirements,
                    WhatYoullLearn = whatYoullLearn,
                    HowYoullLearn = howYoullLearn,
                    WhatYoullNeed = whatYoullNeed,
                    HowYoullBeAssessed = howYoullBeAssessed,
                    WhereNext = whereNext,
                    AdultEducationBudget = adultEducationBudget,
                    AdvancedLearnerLoan = advancedLearnerLoan,
                    CourseRuns = courseRuns,
                    CreatedDate = createdUtc ?? _clock.UtcNow,
                    CreatedByUser = createdBy
                });

            var cosmosCourse = await _cosmosDbQueryDispatcher.ExecuteQuery(new GetCourseById() { CourseId = courseId });
            await _sqlDataSync.SyncCourse(cosmosCourse);

            var course = await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new GetCourse() { CourseId = courseId }));

            return course;
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
                    string providerCourseRef = null,
                    CourseStatus status = CourseStatus.Live) =>
                WithOnlineCourseRun(courseName, flexibleStartDate, startDate, courseUrl, cost, costDescription, durationUnit, durationValue, providerCourseRef, status);

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
                string providerCourseRef = null,
                CourseStatus status = CourseStatus.Live)
            {
                var courseRunId = Guid.NewGuid();

                _courseRuns.Add(new CreateCourseCourseRun()
                {
                    CourseRunId = courseRunId,
                    CourseRunStatus = status,
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
                    CourseRunStatus = status,
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
                string providerCourseRef = null,
                CourseStatus status = CourseStatus.Live)
            {
                var courseRunId = Guid.NewGuid();

                _courseRuns.Add(new CreateCourseCourseRun()
                {
                    CourseRunId = courseRunId,
                    CourseRunStatus = status,
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
