using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Web.Controllers.PublishCourses;
using Dfc.CourseDirectory.Web.ViewModels.PublishCourses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Xunit;

namespace Dfc.CourseDirectory.Web.Tests
{
    public class UpdateCourseRunTests
    {
        PublishViewModel vm;

        [Fact]
        public void UpdateCourseRun()
        {
            List<Course> courses;

            courses = new List<Course>()
            {
                new Course()
                {
                    CourseDescription = "Course Description 1",
                    id =Guid.NewGuid(),
                    IsValid = false,
                    QualificationCourseTitle = "Test Qualification 1",
                    LearnAimRef = "Test Lars Ref 1",
                    NotionalNVQLevelv2 = "Test Level 1",
                    AwardOrgCode = "Test Award Code 1",
                    CourseRuns = new List<CourseRun>()
                    {
                        new CourseRun()
                        {
                            id = Guid.NewGuid(),
                            CourseName = "Test Course Name 1",
                            RecordStatus = RecordStatus.Pending

                        },
                        new CourseRun()
                        {
                            id = Guid.NewGuid(),
                            CourseName = "Test Course Name 2",
                            RecordStatus = RecordStatus.Pending

                        },
                    }
                },
                new Course()
                {
                    CourseDescription = "Course Description 2",
                    id =Guid.NewGuid(),
                    QualificationCourseTitle = "Test Qualification 2",
                    LearnAimRef = "Test Lars Ref 2",
                    NotionalNVQLevelv2 = "Test Level 2",
                    AwardOrgCode = "Test Award Code 2",
                    IsValid = false,
                    CourseRuns = new List<CourseRun>()
                    {
                        new CourseRun()
                        {
                            id = Guid.NewGuid(),
                            CourseName = "Test Course Name 3",
                            RecordStatus = RecordStatus.Pending

                        },
                        new CourseRun()
                        {
                            id = Guid.NewGuid(),
                            CourseName = "Test Course Name 4",
                            RecordStatus = RecordStatus.Pending

                        },
                    }
                }
            };

            vm = new PublishViewModel {
                Courses = courses,
                NumberOfCoursesInFiles = courses.Count
                
            };
            var serviceLogger = new Mock<ILogger<CourseService>>();
            var mockLogger = new Mock<ILogger<PublishCoursesController>>();
            HttpContextAccessor contextAccessor = new HttpContextAccessor();
            var settings = new CourseServiceSettings()
            {
                //ApiUrl = "https://dfc-dev-prov-venue-fa.azurewebsites.net/api/",
                ApiUrl = "http://localhost:7071/api/", //DEBUG URL
                ApiKey = ""
            };

            CourseService service = new CourseService(serviceLogger.Object, new HttpClient(), Options.Create(settings));
            //Arrange
            var controller = new PublishCoursesController(
                logger: mockLogger.Object,
                contextAccessor: contextAccessor,
                courseService: service
                );

            // Act
            var result = controller.Index(vm) as ViewResult;

            // Assert
            Assert.NotNull(result);
        }
    }
}
