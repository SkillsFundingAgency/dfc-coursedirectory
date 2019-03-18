using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Dfc.CourseDirectory.Web.Tests
{
    public class UnregulatedCoursesTests
    {
        Mock serviceLogger = new Mock<ILogger<LarsSearchService>>();
        Mock mockLogger = new Mock<ILogger<UnregulatedCoursesController>>();
        HttpContextAccessor contextAccessor = new HttpContextAccessor();
        LarsSearchSettings settings = new LarsSearchSettings()
        {
            ApiUrl = "",
            ApiKey = ""
        };

        //    CourseService service = new CourseService(serviceLogger.Object, new HttpClient(), Options.Create(settings));
        //    CourseSearchCriteria criteria = new CourseSearchCriteria(UKPRNvalue: 10000654);

        //    var resultCourses = service.GetYourCoursesByUKPRNAsync(criteria).Result.Value;

        //    vm.Courses = resultCourses.Value.SelectMany(o => o.Value).SelectMany(i => i.Value).ToList();
        //    vm.NumberOfCoursesInFiles = vm.Courses.Count();
        //    //var course = courses2.SingleOrDefault(x => x.id == model.CourseId);

        //    // var courserun = course.CourseRuns.SingleOrDefault(x => x.id == model.courseRun.id);

        //    //Arrange
        //    var controller = new PublishCoursesController(
        //        logger: mockLogger.Object,
        //        contextAccessor: contextAccessor,
        //        courseService: service
        //        );


        [Fact]
        public void ServiceWrapper()
        {
            
        }
    }
}
