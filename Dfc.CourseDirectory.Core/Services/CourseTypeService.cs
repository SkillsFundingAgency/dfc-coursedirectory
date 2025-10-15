using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Microsoft.AspNetCore.Components;

namespace Dfc.CourseDirectory.Core.Services
{
    public class CourseTypeService : ICourseTypeService
    {
        private const string ESOL_Text = "ESOL";
        private const string GCSE_9_1_In_English_Language_Text = "GCSE (9-1) in English Language";        
        private const string GCSE_9_1_In_English_Literature_Text = "GCSE (9-1) in English Literature";
        private const string TLevel_Text = "T Level";
        private const string GCSE_A_Level= "GCE A Level";
        private const string GCSE_A2_Level = "GCE A2 Level";
        private const string GCSE_AS_Level = "GCE AS Level";
        private const string REGULATED_QUALIFICATION_FRAMEWORK = "RQF";
        private const string VOCATIONAL_REGULATED_QUALIFICATIONS = "VRQ";
        private const string NATIONAL_VOCATIONAL_QUALIFICATIONS = "NVQ";
        private const string DEGREE = "Degree";
        private const string BAHONS = "BA (Hons)";
        private const string BSCHONS = "BSc (Hons)";
        private const string BENGHONS = "BEng (Hons)";
        private const string BSCORDHONS = "BSc (Ord/Hons)";
        private const string NONREGULATED = "Non regulated";
        private const string NOREGCATEGORY_27 = "27";
        private const string NOREGCATEGORY_23 = "23";
        private const string NOREGCATEGORY_28 = "28";
        private const string NOREGCATEGORY_75 = "75";
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
                var eligibleProvidersList = await _sqlQueryDispatcher.ExecuteQuery(new GetFcfjEligibleProvidersList());

                if (!eligibleProvidersList.Contains(providerId))
                    courseType = null;
            }

            return courseType;
        }
    }
}
