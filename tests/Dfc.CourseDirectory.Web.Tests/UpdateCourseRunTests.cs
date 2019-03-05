using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Web.Controllers.PublishCourses;
using Dfc.CourseDirectory.Web.ViewModels.PublishCourses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
    public class UpdateCourseRunTests
    {
        //PublishViewModel vm;

        //[Fact]
        //public async Task UpdateCourseRun()
        //{
        //    vm = new PublishViewModel();
        //    var serviceLogger = new Mock<ILogger<CourseService>>();
        //    var mockLogger = new Mock<ILogger<PublishCoursesController>>();
        //    HttpContextAccessor contextAccessor = new HttpContextAccessor();
        //    var settings = new CourseServiceSettings()
        //    {
        //        ApiUrl = "",
        //        ApiKey = ""
        //    };

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

        //    // Act
        //    var result = await controller.Index(vm);

        //    // Assert
        //    Assert.NotNull(result);
        //}
    }
}
