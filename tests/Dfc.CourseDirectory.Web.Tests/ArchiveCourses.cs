using Dfc.CourseDirectory.Common.Settings;
using Dfc.CourseDirectory.Services.CourseService;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Dfc.CourseDirectory.Web.Tests
{
    public class ArchiveCourses
    {
        //[Fact]
        //public async Task ArchiveServiceCall()
        //{
        //    var serviceLogger = new Mock<ILogger<CourseService>>();
        //    //var mockLogger = new Mock<ILogger<PublishCoursesController>>();
        //    HttpContextAccessor contextAccessor = new HttpContextAccessor();
        //    CourseForComponentSettings courseForComponentSettings = new CourseForComponentSettings
        //    {
        //        TextFieldMaxChars = 500
        //    };
        //    EntryRequirementsComponentSettings entryRequirementsComponentSettings = new EntryRequirementsComponentSettings
        //    {
        //        TextFieldMaxChars = 500
        //    };
        //WhatWillLearnComponentSettings whatWillLearnComponentSettings = new WhatWillLearnComponentSettings
        //{
        //    TextFieldMaxChars = 500
        //}; 
        //    HowYouWillLearnComponentSettings howYouWillLearnComponentSettings = new HowYouWillLearnComponentSettings
        //    {
        //        TextFieldMaxChars = 500
        //    };
        //    WhatYouNeedComponentSettings whatYouNeedComponentSettings = new WhatYouNeedComponentSettings
        //    {
        //        TextFieldMaxChars = 500
        //    };
        //    HowAssessedComponentSettings howAssessedComponentSettings = new HowAssessedComponentSettings
        //    {
        //        TextFieldMaxChars = 500
        //    };
        //    WhereNextComponentSettings whereNextComponentSettings = new WhereNextComponentSettings
        //    {
        //        TextFieldMaxChars = 500
        //    };


        //    var settings = new CourseServiceSettings()
        //    {
        //        ApiUrl = "",
        //        ApiKey = ""
        //    };

        //    CourseService service = new CourseService(
        //        serviceLogger.Object, 
        //        new HttpClient(), 
        //        Options.Create(settings),
        //        Options.Create(courseForComponentSettings),
        //        Options.Create(entryRequirementsComponentSettings),
        //        Options.Create(whatWillLearnComponentSettings),
        //        Options.Create(howYouWillLearnComponentSettings),
        //        Options.Create(whatYouNeedComponentSettings),
        //        Options.Create(howAssessedComponentSettings),
        //        Options.Create(whereNextComponentSettings));

            
        //    CourseSearchCriteria criteria = new CourseSearchCriteria(UKPRNvalue: 10000654);

        //    var resultCourses = service.GetYourCoursesByUKPRNAsync(criteria).Result.Value;

        //    var Courses = resultCourses.Value.SelectMany(o => o.Value).SelectMany(i => i.Value).ToList();
        //    var NumberOfCoursesInFiles = Courses.Count();

        //    Guid courseId = Guid.Parse("2644789e-d237-4e03-b034-0a1cd681ded0");
        //    Guid courseRunId = Guid.Parse("932ecd90-1a0e-41d8-8e30-1a939d291bf4");
        //    // Act
        //    var result = await service.UpdateStatus(courseId, courseRunId, 4);

        //    Assert.NotNull(result);
        //}
    }
}
