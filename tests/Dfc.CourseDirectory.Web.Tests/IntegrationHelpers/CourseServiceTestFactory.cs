using Dfc.CourseDirectory.Common.Settings;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Microsoft.Extensions.Options;

namespace Dfc.CourseDirectory.Web.Tests.IntegrationHelpers
{
    public static class CourseServiceTestFactory
    {
        public static ICourseService GetService()
        {
            var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<CourseService>.Instance;
            var courseSettings = TestConfig.GetSettings<CourseServiceSettings>("CourseServiceSettings");
            var facSettings = TestConfig.GetSettings<FindACourseServiceSettings>("FindACourseServiceSettings");
            var courseForComponentSettings = TestConfig.GetSettings<CourseForComponentSettings>("CourseForComponentSettings");
            var entryRequirementsComponentSettings = TestConfig.GetSettings<EntryRequirementsComponentSettings>("EntryRequirementsComponentSettings");
            var whatWillLearnComponentSettings = TestConfig.GetSettings<WhatWillLearnComponentSettings>("WhatWillLearnComponentSettings");
            var whatYouNeedComponentSettings = TestConfig.GetSettings<WhatYouNeedComponentSettings>("WhatYouNeedComponentSettings");
            var howYouWillLearnComponentSettings = TestConfig.GetSettings<HowYouWillLearnComponentSettings>("HowYouWillLearnComponentSettings");
            var howAssessedComponentSettings = TestConfig.GetSettings<HowAssessedComponentSettings>("HowAssessedComponentSettings");
            var whereNextComponentSettings = TestConfig.GetSettings<WhereNextComponentSettings>("WhereNextComponentSettings");

            ICourseService service = new CourseService(logger,
                new System.Net.Http.HttpClient(),
                Options.Create(courseSettings),
                Options.Create(facSettings),
                Options.Create(courseForComponentSettings),
                Options.Create(entryRequirementsComponentSettings),
                Options.Create(whatWillLearnComponentSettings),
                Options.Create(whatYouNeedComponentSettings),
                Options.Create(howYouWillLearnComponentSettings),
                Options.Create(howAssessedComponentSettings),
                Options.Create(whereNextComponentSettings));
            return service;
        }
    }
}
