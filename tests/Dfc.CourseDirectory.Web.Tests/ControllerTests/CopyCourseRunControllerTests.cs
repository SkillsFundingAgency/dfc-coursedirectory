using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Bogus;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Middleware;
using Dfc.CourseDirectory.Core.Security;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Models.Regions;
using Dfc.CourseDirectory.Web.Controllers.CopyCourse;
using Dfc.CourseDirectory.WebV2.ViewModels.CopyCourse;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Dfc.CourseDirectory.Web.Tests.ControllerTests
{
    public class CopyCourseRunControllerTests
    {
        private const string SessionNonLarsCourse = "NonLarsCourse";

        private readonly Mock<ICourseService> _mockCourseService;
        private readonly Mock<ILogger<CopyCourseRunController>> _mockLogger;
        private readonly Mock<ISqlQueryDispatcher> _mockSqlQueryDispatcher;
        private readonly Mock<ICurrentUserProvider> _mockCurrentUserProvider;
        private readonly Mock<IProviderContextProvider> _mockProviderContextProvider;
        private readonly Mock<IClock> _mockClock;
        private readonly Mock<IRegionCache> _mockRegionCache;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ISession> _mockSession;

        public CopyCourseRunControllerTests()
        {
            _mockLogger = new Mock<ILogger<CopyCourseRunController>>();
            _mockCourseService = new Mock<ICourseService>();
            _mockSqlQueryDispatcher = new Mock<ISqlQueryDispatcher>();
            _mockProviderContextProvider = new Mock<IProviderContextProvider>();
            _mockCurrentUserProvider = new Mock<ICurrentUserProvider>();
            _mockClock = new Mock<IClock>();
            _mockRegionCache = new Mock<IRegionCache>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockSession = new Mock<ISession>();
        }

        [Fact]
        public async Task Index_WhenUkprnSessionHasNoValue_RedirectsToHome()
        {
            // Arrange
            var copyCourseRunController = GetController();

            var courseId = Guid.NewGuid();
            var courseRunId = Guid.NewGuid();

            // Act
            var result = await copyCourseRunController.Index(courseId, courseRunId) as RedirectToActionResult;

            // Assert            
            Assert.NotNull(result);

            Assert.Equal("Home", result.ControllerName);
            Assert.Equal("Index", result.ActionName);
            Assert.Equal("Please select a Provider.", result.RouteValues["errmsg"]);
        }

        [Fact]
        public async Task Index_WhenCourseIsNotFound_ReturnsNotFoundResult()
        {
            // Arrange
            var copyCourseRunController = GetController();

            var courseId = Guid.NewGuid();
            var courseRunId = Guid.NewGuid();

            var providerInfo = new ProviderInfo { ProviderId = Guid.NewGuid(), ProviderName = Faker.Company.Name() };
            var providerContext = new ProviderContext(providerInfo);

            var ukprn = Encoding.UTF8.GetBytes("123456");

            _mockSession.Setup(m => m.TryGetValue("UKPRN", out ukprn)).Returns(true);
            _mockProviderContextProvider.Setup(m => m.GetProviderContext(true)).Returns(providerContext);

            // Act
            var result = await copyCourseRunController.Index(courseId, courseRunId) as NotFoundResult;

            // Assert            
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Index_WhenCourseRunIsNotFound_ReturnsNotFoundResult()
        {
            // Arrange
            var copyCourseRunController = GetController();

            var courseId = Guid.NewGuid();
            var courseRunId = Guid.NewGuid();

            var providerInfo = new ProviderInfo { ProviderId = Guid.NewGuid(), ProviderName = Faker.Company.Name() };
            var providerContext = new ProviderContext(providerInfo);

            var ukprn = Encoding.UTF8.GetBytes("123456");

            _mockSession.Setup(m => m.TryGetValue("UKPRN", out ukprn)).Returns(true);
            _mockProviderContextProvider.Setup(m => m.GetProviderContext(true)).Returns(providerContext);

            var course = new Faker<Course>()
                .RuleFor(c => c.CourseDescription, f => f.Lorem.Paragraph())
                .RuleFor(c => c.EntryRequirements, f => f.Lorem.Paragraph())
                .RuleFor(c => c.WhatYoullLearn, f => f.Lorem.Paragraph())
                .RuleFor(c => c.HowYoullLearn, f => f.Lorem.Paragraph())
                .RuleFor(c => c.WhatYoullNeed, f => f.Lorem.Paragraph())
                .RuleFor(c => c.HowYoullBeAssessed, f => f.Lorem.Paragraph())
                .Generate();
            course.CourseRuns = new List<CourseRun>();
            _mockSqlQueryDispatcher.Setup(m => m.ExecuteQuery(It.IsAny<GetCourse>())).ReturnsAsync(course);

            // Act
            var result = await copyCourseRunController.Index(courseId, courseRunId) as NotFoundResult;

            // Assert            
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Index_WhenCourseAndCourseRunAreFound_ReturnsViewResultWithModelData()
        {
            // Arrange
            var copyCourseRunController = GetController();

            var courseId = Guid.NewGuid();
            var courseRunId = Guid.NewGuid();

            var providerInfo = new ProviderInfo { ProviderId = Guid.NewGuid(), ProviderName = Faker.Company.Name() };
            var providerContext = new ProviderContext(providerInfo);

            var ukprn = Encoding.UTF8.GetBytes("123456");

            _mockSession.Setup(m => m.TryGetValue("UKPRN", out ukprn)).Returns(true);
            _mockProviderContextProvider.Setup(m => m.GetProviderContext(true)).Returns(providerContext);

            var course = new Faker<Course>()
                .RuleFor(c => c.CourseDescription, f => f.Lorem.Paragraph())
                .RuleFor(c => c.EntryRequirements, f => f.Lorem.Paragraph())
                .RuleFor(c => c.WhatYoullLearn, f => f.Lorem.Paragraph())
                .RuleFor(c => c.HowYoullLearn, f => f.Lorem.Paragraph())
                .RuleFor(c => c.WhatYoullNeed, f => f.Lorem.Paragraph())
                .RuleFor(c => c.HowYoullBeAssessed, f => f.Lorem.Paragraph())
                .Generate();
            course.CourseRuns = new Faker<CourseRun>().RuleFor(c => c.CourseRunId, f => courseRunId).Generate(1);
            _mockSqlQueryDispatcher.Setup(m => m.ExecuteQuery(It.IsAny<GetCourse>())).ReturnsAsync(course);

            var venues = new Faker<Venue>().Generate(2);
            _mockSqlQueryDispatcher.Setup(m => m.ExecuteQuery(It.IsAny<GetVenuesByProvider>())).ReturnsAsync(venues);

            var regions = new SelectRegionModel();
            _mockCourseService.Setup(m => m.GetRegions()).Returns(regions);

            // Act
            var result = await copyCourseRunController.Index(courseId, courseRunId) as ViewResult;

            // Assert            
            Assert.NotNull(result);
            Assert.Equal("CopyCourseRun", result.ViewName);

            var model = result.Model as CopyCourseRunViewModel;
            Assert.NotNull(model);
        }

        [Fact]
        public async Task Index_WhenCourseIdAndCourseRunIdAreNotProvidedAndSavedModelHasData_ReturnsViewResultWithModelData()
        {
            // Arrange
            var copyCourseRunController = GetController();

            var courseId = Guid.NewGuid();
            var courseRunId = Guid.NewGuid();

            var providerInfo = new ProviderInfo { ProviderId = Guid.NewGuid(), ProviderName = Faker.Company.Name() };
            var providerContext = new ProviderContext(providerInfo);

            var ukprn = Encoding.UTF8.GetBytes("123456");

            _mockSession.Setup(m => m.TryGetValue("UKPRN", out ukprn)).Returns(true);
            _mockProviderContextProvider.Setup(m => m.GetProviderContext(true)).Returns(providerContext);

            var falseValue = Encoding.UTF8.GetBytes("false");
            _mockSession.Setup(m => m.TryGetValue("NonLarsCourse", out falseValue)).Returns(true);

            var saveModel = new Faker<CopyCourseRunSaveViewModel>()
                .RuleFor(c => c.CourseId, f => courseId)
                .RuleFor(c => c.CourseRunId, f => courseRunId)
                .Generate();
            var saveModelBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(saveModel));
            _mockSession.Setup(m => m.TryGetValue("CopyCourseRunSaveViewModel", out saveModelBytes)).Returns(true);

            var course = new Faker<Course>()
                .RuleFor(c => c.CourseDescription, f => f.Lorem.Paragraph())
                .RuleFor(c => c.EntryRequirements, f => f.Lorem.Paragraph())
                .RuleFor(c => c.WhatYoullLearn, f => f.Lorem.Paragraph())
                .RuleFor(c => c.HowYoullLearn, f => f.Lorem.Paragraph())
                .RuleFor(c => c.WhatYoullNeed, f => f.Lorem.Paragraph())
                .RuleFor(c => c.HowYoullBeAssessed, f => f.Lorem.Paragraph())
                .Generate();
            course.CourseRuns = new Faker<CourseRun>().RuleFor(c => c.CourseRunId, f => courseRunId).Generate(1);
            _mockSqlQueryDispatcher.Setup(m => m.ExecuteQuery(It.IsAny<GetCourse>())).ReturnsAsync(course);

            var venues = new Faker<Venue>().Generate(2);
            _mockSqlQueryDispatcher.Setup(m => m.ExecuteQuery(It.IsAny<GetVenuesByProvider>())).ReturnsAsync(venues);

            var regions = new SelectRegionModel();
            _mockCourseService.Setup(m => m.GetRegions()).Returns(regions);

            // Act
            var result = await copyCourseRunController.Index(null, null) as ViewResult;

            // Assert            
            Assert.NotNull(result);
            Assert.Equal("CopyCourseRun", result.ViewName);

            var model = result.Model as CopyCourseRunViewModel;
            Assert.NotNull(model);

            _mockSession.Verify(m => m.TryGetValue(SessionNonLarsCourse, out falseValue), Times.AtLeastOnce);
        }

        [Fact]
        public async Task Index_WhenCourseIdAndCourseRunIdAreProvidedAndItIsNonLarsCourse_ReturnsViewResultWithModelData()
        {
            // Arrange
            var copyCourseRunController = GetController();

            var courseId = Guid.NewGuid();
            var courseRunId = Guid.NewGuid();

            var providerInfo = new ProviderInfo { ProviderId = Guid.NewGuid(), ProviderName = Faker.Company.Name() };
            var providerContext = new ProviderContext(providerInfo);

            var ukprn = Encoding.UTF8.GetBytes("123456");

            _mockSession.Setup(m => m.TryGetValue("UKPRN", out ukprn)).Returns(true);
            _mockProviderContextProvider.Setup(m => m.GetProviderContext(true)).Returns(providerContext);

            var trueValue = Encoding.UTF8.GetBytes("true");
            _mockSession.Setup(m => m.TryGetValue(SessionNonLarsCourse, out trueValue)).Returns(true);

            var saveModel = new Faker<CopyCourseRunSaveViewModel>().Generate();
            var saveModelBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(saveModel));
            _mockSession.Setup(m => m.TryGetValue("CopyCourseRunSaveViewModel", out saveModelBytes)).Returns(true);

            var course = new Faker<Course>()
                .RuleFor(c => c.CourseDescription, f => f.Lorem.Paragraph())
                .RuleFor(c => c.EntryRequirements, f => f.Lorem.Paragraph())
                .RuleFor(c => c.WhatYoullLearn, f => f.Lorem.Paragraph())
                .RuleFor(c => c.HowYoullLearn, f => f.Lorem.Paragraph())
                .RuleFor(c => c.WhatYoullNeed, f => f.Lorem.Paragraph())
                .RuleFor(c => c.HowYoullBeAssessed, f => f.Lorem.Paragraph())
                .Generate();
            course.CourseRuns = new Faker<CourseRun>().RuleFor(c => c.CourseRunId, f => courseRunId).Generate(1);
            _mockSqlQueryDispatcher.Setup(m => m.ExecuteQuery(It.IsAny<GetNonLarsCourse>())).ReturnsAsync(course);

            var venues = new Faker<Venue>().Generate(2);
            _mockSqlQueryDispatcher.Setup(m => m.ExecuteQuery(It.IsAny<GetVenuesByProvider>())).ReturnsAsync(venues);

            var regions = new SelectRegionModel();
            _mockCourseService.Setup(m => m.GetRegions()).Returns(regions);

            // Act
            var result = await copyCourseRunController.Index(courseId, courseRunId) as ViewResult;

            // Assert            
            Assert.NotNull(result);
            Assert.Equal("CopyCourseRun", result.ViewName);

            var model = result.Model as CopyCourseRunViewModel;
            Assert.NotNull(model);

            _mockSession.Verify(m => m.TryGetValue(SessionNonLarsCourse, out trueValue), Times.AtLeastOnce);
        }

        private CopyCourseRunController GetController()
        {
            var addCourseController = new CopyCourseRunController(
                _mockLogger.Object,
                            _mockCourseService.Object,
                            _mockSqlQueryDispatcher.Object,
                            _mockProviderContextProvider.Object,
                            _mockCurrentUserProvider.Object,
                            _mockClock.Object,
                            _mockRegionCache.Object,
                            _mockConfiguration.Object);
            addCourseController.ControllerContext = new ControllerContext();
            addCourseController.ControllerContext.HttpContext = new DefaultHttpContext { Session = _mockSession.Object };
            return addCourseController;
        }
    }
}
