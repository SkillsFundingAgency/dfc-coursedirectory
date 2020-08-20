using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing.DataStore.CosmosDb.Queries;

namespace Dfc.CourseDirectory.Testing
{
    public partial class TestData
    {
        public async Task<Guid> CreateCourse(
            Guid providerId,
            UserInfo createdBy,
            string qualificationCourseTitle = "Education assessment in Maths",
            string learnAimRef = "Z0007842",
            string notionalNVQLevelv2 = "E",  // TODO When we have a LARS data store for testing we should lookup from there
            string awardOrgCode = "NONE",  // as above
            string qualificationType = "Other",  // as above
            string courseDescription = "The description",
            string entryRequirements = "To be eligible for ESFA government funding, you must be 19 or over. You must be a UK or EU citizen or have indefinite leave to remain in the UK. Learners will be ineligible if they have previously achieved a GCSE grade A* to C in the subject.",
            string whatYoullLearn = "DIGITAL EMPLOYABILITY SKILLS",
            string howYoullLearn = "Group coaching and 1-2-1 support through Tutor led learning, written assignments throughout courses, Independent Learning, access to comprehensive online and paper based learning resources.",
            string whatYoullNeed = "Valid ID (driving licence, passport, birth certificate)",
            string howYoullBeAssessed = "Paper based or online assessments",
            string whereNext = "We will actively signpost and support you to achieve the next level of progression for all certificated aims.",
            bool adultEducationBudget = false,
            bool advancedLearnerLoan = false,
            DateTime? createdUtc = null)
        {
            var provider = await _cosmosDbQueryDispatcher.ExecuteQuery(new GetProviderById()
            {
                ProviderId = providerId
            });

            if (provider == null)
            {
                throw new ArgumentException("Provider does not exist.", nameof(providerId));
            }

            var courseId = Guid.NewGuid();

            await _cosmosDbQueryDispatcher.ExecuteQuery(
                new CreateCourse()
                {
                    CourseId = courseId,
                    ProviderId = providerId,
                    ProviderUkprn = provider.Ukprn,
                    QualificationCourseTitle = qualificationCourseTitle,
                    LearnAimRef = learnAimRef,
                    NotionalNVQLevelv2 = notionalNVQLevelv2,
                    AwardOrgCode = awardOrgCode,
                    QualificationType = qualificationType,
                    CourseDescription = courseDescription,
                    EntryRequirements = entryRequirements,
                    WhatYoullLearn = whatYoullLearn,
                    HowYoullLearn = howYoullLearn,
                    WhatYoullNeed = whatYoullNeed,
                    HowYoullBeAssessed = howYoullBeAssessed,
                    WhereNext = whereNext,
                    AdultEducationBudget = adultEducationBudget,
                    AdvancedLearnerLoan = advancedLearnerLoan,
                    CourseRuns = new[]
                    {
                        new CreateCourseCourseRun()
                        {
                            CourseRunId = Guid.NewGuid(),
                            CourseName = qualificationCourseTitle,
                            DeliveryMode = CourseDeliveryMode.Online,
                            FlexibleStartDate = true,
                            Cost = 69,
                            DurationUnit = CourseDurationUnit.Months,
                            DurationValue = 6,
                            AttendancePattern = CourseAttendancePattern.Evening,
                            National = true
                        }
                    },
                    CreatedDate = createdUtc ?? _clock.UtcNow,
                    CreatedByUser = createdBy
                });

            return courseId;
        }
    }
}
