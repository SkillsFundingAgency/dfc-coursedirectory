using System;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Core.Services
{
    public class CourseTypeService : ICourseTypeService
    {
        private const string ESOL_Text = "ESOL";
        private const string GCSE_9_1_In_English_Language_Text = "GCSE (9-1) in English Language";        
        private const string GCSE_9_1_In_English_Literature_Text = "GCSE (9-1) in English Literature";
        private const string TLevel_Text = "T Level";
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;

        public CourseTypeService(
            ISqlQueryDispatcher sqlQueryDispatcher)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
        }

        public async Task<Models.CourseType?> GetCourseType(string learnAimRef, Guid providerId)
        {
            var larsCourseTypes = await _sqlQueryDispatcher.ExecuteQuery(new GetLarsCourseType() { LearnAimRef = learnAimRef });

            foreach (var larsCourseType in larsCourseTypes)
            {
                if (larsCourseType.CategoryRef == "40" &&
                    !(larsCourseType.LearnAimRefTitle.Contains(ESOL_Text)
                    || larsCourseType.LearnAimRefTitle.Contains(GCSE_9_1_In_English_Language_Text)
                    || larsCourseType.LearnAimRefTitle.Contains(GCSE_9_1_In_English_Literature_Text)))
                {
                    larsCourseType.CourseType = null;
                    continue;
                }

                if (larsCourseType.CategoryRef == "37" &&
                    !(larsCourseType.LearnAimRefTitle.Contains(GCSE_9_1_In_English_Language_Text)
                    || larsCourseType.LearnAimRefTitle.Contains(GCSE_9_1_In_English_Literature_Text)))
                {
                    larsCourseType.CourseType = null;
                    continue;
                }

                if (larsCourseType.CategoryRef == "3" && !larsCourseType.LearnAimRefTitle.StartsWith(TLevel_Text))
                {
                    larsCourseType.CourseType = null;
                }
            }

            var distinctLarsCourseTypes = larsCourseTypes.Select(lc => lc.CourseType).Distinct();
            var courseType = distinctLarsCourseTypes.FirstOrDefault(l => l.HasValue);

            if (courseType.HasValue && courseType.Value == Models.CourseType.FreeCoursesForJobs)
            {                
                var providerCampaignCodes = await _sqlQueryDispatcher.ExecuteQuery(new GetCampaignCodesForProvider() { ProviderId = providerId });

                var providerInEligibleList = providerCampaignCodes.Any();

                if (!providerInEligibleList)
                    courseType = null;
            }

            return courseType;
        }
    }
}
