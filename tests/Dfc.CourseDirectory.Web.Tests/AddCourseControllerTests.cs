using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.Services;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.WebV2.Security;
using Dfc.CourseDirectory.WebV2;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;
using Dfc.CourseDirectory.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using Dfc.CourseDirectory.Web.ViewModels;
using System;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Bogus;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;

namespace Dfc.CourseDirectory.Web.Tests
{
    public class AddCourseControllerTests
    {
        private const string SessionNonLarsCourse = "NonLarsCourse";
        private const string SessionLearnAimRef = "LearnAimRef";
        private const string SessionNotionalNvqLevelV2 = "NotionalNVQLevelv2";
        private const string SessionAwardOrgCode = "AwardOrgCode";
        private const string SessionLearnAimRefTitle = "LearnAimRefTitle";
        private const string SessionLearnAimRefTypeDesc = "LearnAimRefTypeDesc";
        private readonly Mock<ICourseService> _mockCourseService;
        private readonly Mock<ISession> _mockSession;
        private readonly Mock<ISqlQueryDispatcher> _mockSqlQueryDispatcher;
        private readonly Mock<ICurrentUserProvider> _mockCurrentUserProvider;
        private readonly Mock<IProviderContextProvider> _mockProviderContextProvider;
        private readonly Mock<ICourseTypeService> _mockCourseTypeService;

        public AddCourseControllerTests()
        {
            _mockCourseService = new Mock<ICourseService>();
            _mockSession = new Mock<ISession>();
            _mockSqlQueryDispatcher = new Mock<ISqlQueryDispatcher>();
            _mockCurrentUserProvider = new Mock<ICurrentUserProvider>();
            _mockProviderContextProvider = new Mock<IProviderContextProvider>();
            _mockCourseTypeService = new Mock<ICourseTypeService>();
        }

        [Fact]
        public async Task AddCourse_WhenLearnAimRefHasValueAndCourseIdIsNull_ReturnsViewModelWithEmptyCourseDataAndSetsLarsSessionObjects()
        {
            // Arrange
            var addCourseController = GetController();

            var learnAimRef = "67894653";
            var notionalNVQLevelv2 = "level";
            var awardOrgCode = "C123";
            var learnAimRefTitle = "learn aim ref title";
            var learnAimRefTitleDescription = "learn aim ref title desc";

            // Act
            var viewResult = await addCourseController.AddCourse(learnAimRef, notionalNVQLevelv2, awardOrgCode, learnAimRefTitle, learnAimRefTitleDescription, null) as ViewResult;

            // Assert            
            Assert.NotNull(viewResult);

            var viewModel = viewResult.Model as AddCourseViewModel;
            Assert.NotNull(viewModel);

            Assert.Null(viewModel.EntryRequirements.EntryRequirements);
            Assert.Null(viewModel.WhatWillLearn.WhatWillLearn);
            Assert.Null(viewModel.HowYouWillLearn.HowYouWillLearn);
            Assert.Null(viewModel.WhatYouNeed.WhatYouNeed);
            Assert.Null(viewModel.HowAssessed.HowAssessed);

            _mockSession.Verify(p => p.Set(SessionLearnAimRef, Encoding.UTF8.GetBytes(learnAimRef)), Times.Once);
            _mockSession.Verify(p => p.Set(SessionNotionalNvqLevelV2, Encoding.UTF8.GetBytes(notionalNVQLevelv2)), Times.Once);
            _mockSession.Verify(p => p.Set(SessionAwardOrgCode, Encoding.UTF8.GetBytes(awardOrgCode)), Times.Once);
            _mockSession.Verify(p => p.Set(SessionLearnAimRefTitle, Encoding.UTF8.GetBytes(learnAimRefTitle)), Times.Once);
            _mockSession.Verify(p => p.Set(SessionLearnAimRefTypeDesc, Encoding.UTF8.GetBytes(learnAimRefTitleDescription)), Times.Once);
        }

        [Fact]
        public async Task AddCourse_WhenLearnAimRefHasValueAndItsDefaultDataExistsAndCourseIdIsNull_ReturnsViewModelWithDefaultCourseDataAndSetsLarsSessionObjects()
        {
            // Arrange
            var addCourseController = GetController();

            var learnAimRef = "67894653";
            var notionalNVQLevelv2 = Faker.Lorem.GetFirstWord();
            var awardOrgCode = Faker.Lorem.GetFirstWord();
            var learnAimRefTitle = Faker.Lorem.Sentence(1);
            var learnAimRefTitleDescription = Faker.Lorem.Paragraph(1);
            Guid? courseId = null;

            var course = new Faker<CourseText>()                
                .RuleFor(c => c.CourseDescription, f => f.Lorem.Paragraph())
                .RuleFor(c => c.EntryRequirements, f => f.Lorem.Paragraph())
                .RuleFor(c => c.WhatYoullLearn, f => f.Lorem.Paragraph())
                .RuleFor(c => c.HowYoullLearn, f => f.Lorem.Paragraph())
                .RuleFor(c => c.WhatYoullNeed, f => f.Lorem.Paragraph())
                .RuleFor(c => c.HowYoullBeAssessed, f => f.Lorem.Paragraph())
                .Generate();

            _mockSqlQueryDispatcher.Setup(m => m.ExecuteQuery(It.IsAny<GetCourseTextByLearnAimRef>())).ReturnsAsync(course);

            // Act
            var viewResult = await addCourseController.AddCourse(learnAimRef, notionalNVQLevelv2, awardOrgCode, learnAimRefTitle, learnAimRefTitleDescription, courseId) as ViewResult;

            // Assert            
            Assert.NotNull(viewResult);

            var viewModel = viewResult.Model as AddCourseViewModel;
            Assert.NotNull(viewModel);

            Assert.Equal(course.EntryRequirements, viewModel.EntryRequirements.EntryRequirements);
            Assert.Equal(course.WhatYoullLearn, viewModel.WhatWillLearn.WhatWillLearn);
            Assert.Equal(course.HowYoullLearn, viewModel.HowYouWillLearn.HowYouWillLearn);
            Assert.Equal(course.WhatYoullNeed, viewModel.WhatYouNeed.WhatYouNeed);
            Assert.Equal(course.HowYoullBeAssessed, viewModel.HowAssessed.HowAssessed);

            _mockSession.Verify(p => p.Set(SessionLearnAimRef, Encoding.UTF8.GetBytes(learnAimRef)), Times.Once);
            _mockSession.Verify(p => p.Set(SessionNotionalNvqLevelV2, Encoding.UTF8.GetBytes(notionalNVQLevelv2)), Times.Once);
            _mockSession.Verify(p => p.Set(SessionAwardOrgCode, Encoding.UTF8.GetBytes(awardOrgCode)), Times.Once);
            _mockSession.Verify(p => p.Set(SessionLearnAimRefTitle, Encoding.UTF8.GetBytes(learnAimRefTitle)), Times.Once);
            _mockSession.Verify(p => p.Set(SessionLearnAimRefTypeDesc, Encoding.UTF8.GetBytes(learnAimRefTitleDescription)), Times.Once);
        }

        [Fact]
        public async Task AddCourse_WhenLearnAimRefAndCourseIdHaveValues_ReturnsViewModelWithAleardyExistingCourseDataAndSetsLarsSessionObjects()
        {
            // Arrange
            var addCourseController = GetController();

            var learnAimRef = "67894653";
            var notionalNVQLevelv2 = "level";
            var awardOrgCode = "C123";
            var learnAimRefTitle = "learn aim ref title";
            var learnAimRefTitleDescription = "learn aim ref title desc";
            var courseId = Guid.NewGuid();

            var course = new Faker<Course>()
                .RuleFor(c => c.CourseId, f => courseId)
                .RuleFor(c => c.CourseDescription, f => f.Lorem.Paragraph())
                .RuleFor(c => c.EntryRequirements, f => f.Lorem.Paragraph())
                .RuleFor(c => c.WhatYoullLearn, f => f.Lorem.Paragraph())
                .RuleFor(c => c.HowYoullLearn, f => f.Lorem.Paragraph())
                .RuleFor(c => c.WhatYoullNeed, f => f.Lorem.Paragraph())
                .RuleFor(c => c.HowYoullBeAssessed, f => f.Lorem.Paragraph()).Generate();            

            _mockSqlQueryDispatcher.Setup(m => m.ExecuteQuery(It.IsAny<GetCourse>())).ReturnsAsync(course);

            // Act
            var viewResult = await addCourseController.AddCourse(learnAimRef, notionalNVQLevelv2, awardOrgCode, learnAimRefTitle, learnAimRefTitleDescription, courseId) as ViewResult;

            // Assert            
            Assert.NotNull(viewResult);

            var viewModel = viewResult.Model as AddCourseViewModel;
            Assert.NotNull(viewModel);

            Assert.Equal(course.EntryRequirements, viewModel.EntryRequirements.EntryRequirements);
            Assert.Equal(course.WhatYoullLearn, viewModel.WhatWillLearn.WhatWillLearn);
            Assert.Equal(course.HowYoullLearn, viewModel.HowYouWillLearn.HowYouWillLearn);
            Assert.Equal(course.WhatYoullNeed, viewModel.WhatYouNeed.WhatYouNeed);
            Assert.Equal(course.HowYoullBeAssessed, viewModel.HowAssessed.HowAssessed);

            _mockSession.Verify(p => p.Set(SessionLearnAimRef, Encoding.UTF8.GetBytes(learnAimRef)), Times.Once);
            _mockSession.Verify(p => p.Set(SessionNotionalNvqLevelV2, Encoding.UTF8.GetBytes(notionalNVQLevelv2)), Times.Once);
            _mockSession.Verify(p => p.Set(SessionAwardOrgCode, Encoding.UTF8.GetBytes(awardOrgCode)), Times.Once);
            _mockSession.Verify(p => p.Set(SessionLearnAimRefTitle, Encoding.UTF8.GetBytes(learnAimRefTitle)), Times.Once);
            _mockSession.Verify(p => p.Set(SessionLearnAimRefTypeDesc, Encoding.UTF8.GetBytes(learnAimRefTitleDescription)), Times.Once);
        }        

        [Fact]
        public async Task AddCourse_WhenLearnAimRefIsNullOrEmptyAndCourseIdIsNull_ReturnsViewModelWithEmptyCourseDataAndSetsNonLarsSessionObject()
        {
            // Arrange
            var addCourseController = GetController();

            // Act
            var viewResult = await addCourseController.AddCourse(null, null, null, null, null, null) as ViewResult;

            // Assert            
            Assert.NotNull(viewResult);

            var viewModel = viewResult.Model as AddCourseViewModel;
            Assert.NotNull(viewModel);

            _mockSession.Verify(p => p.Set(SessionNonLarsCourse, Encoding.UTF8.GetBytes("true")), Times.Once);
        }

        [Fact]
        public async Task AddCourse_WhenLearnAimRefIsNullOrEmptyAndCourseIdHasValue_ReturnsViewModelWithCourseDataAndSetsNonLarsSessionObject()
        {
            // Arrange
            var addCourseController = GetController();
            var courseId = Guid.NewGuid();

            var course = new Faker<Course>()
               .RuleFor(c => c.CourseId, f => courseId)
               .RuleFor(c => c.CourseDescription, f => f.Lorem.Paragraph())
               .RuleFor(c => c.EntryRequirements, f => f.Lorem.Paragraph())
               .RuleFor(c => c.WhatYoullLearn, f => f.Lorem.Paragraph())
               .RuleFor(c => c.HowYoullLearn, f => f.Lorem.Paragraph())
               .RuleFor(c => c.WhatYoullNeed, f => f.Lorem.Paragraph())
               .RuleFor(c => c.HowYoullBeAssessed, f => f.Lorem.Paragraph()).Generate();

            _mockSqlQueryDispatcher.Setup(m => m.ExecuteQuery(It.IsAny<GetNonLarsCourse>())).ReturnsAsync(course);

            // Act
            var viewResult = await addCourseController.AddCourse(null, null, null, null, null, courseId) as ViewResult;

            // Assert            
            Assert.NotNull(viewResult);

            var viewModel = viewResult.Model as AddCourseViewModel;
            Assert.NotNull(viewModel);

            Assert.Equal(course.EntryRequirements, viewModel.EntryRequirements.EntryRequirements);
            Assert.Equal(course.WhatYoullLearn, viewModel.WhatWillLearn.WhatWillLearn);
            Assert.Equal(course.HowYoullLearn, viewModel.HowYouWillLearn.HowYouWillLearn);
            Assert.Equal(course.WhatYoullNeed, viewModel.WhatYouNeed.WhatYouNeed);
            Assert.Equal(course.HowYoullBeAssessed, viewModel.HowAssessed.HowAssessed);

            _mockSession.Verify(p => p.Set(SessionNonLarsCourse, Encoding.UTF8.GetBytes("true")), Times.Once);
            _mockSqlQueryDispatcher.Verify(m => m.ExecuteQuery(It.IsAny<GetNonLarsCourse>()), Times.Once); 
        }

        private AddCourseController GetController()
        {
            var addCourseController = new AddCourseController(
                            _mockCourseService.Object,
                            _mockSqlQueryDispatcher.Object,
                            _mockCurrentUserProvider.Object,
                            _mockProviderContextProvider.Object,
                            _mockCourseTypeService.Object);
            addCourseController.ControllerContext = new ControllerContext();
            addCourseController.ControllerContext.HttpContext = new DefaultHttpContext { Session = _mockSession.Object };
            return addCourseController;
        }
    }
}
