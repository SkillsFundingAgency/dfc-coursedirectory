using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Web.Controllers;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.Views.Shared;
using Dfc.CourseDirectory.WebV2;
using Dfc.CourseDirectory.WebV2.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using Bogus;
using Dfc.CourseDirectory.Web.Extensions;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using System.Collections.ObjectModel;
using System.Linq;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Web.Tests
{
    public class AddCourseControllerTests
    {

        private readonly Mock<ICourseService> _mockCourseService;
        private readonly Mock<ISqlQueryDispatcher> _mockSqlQueryDispatcher;
        private readonly Mock<ICurrentUserProvider> _mockCurrentUserProvider;
        private readonly Mock<IProviderContextProvider> _mockProviderContextProvider;

        public AddCourseControllerTests()
        {
            _mockCourseService = new Mock<ICourseService>();
            _mockSqlQueryDispatcher = new Mock<ISqlQueryDispatcher>();
            _mockCurrentUserProvider = new Mock<ICurrentUserProvider>();
            _mockProviderContextProvider = new Mock<IProviderContextProvider>();
        }

        [Fact]
        public void AcceptAndPublish_WhenLARSCategoryRefIs3AndLearnAimRefTitleStartsWithTLevel_AddsCourseWithCourseTypeAsTLevels()
        {
            // Arrange
            var controller = new AddCourseController(_mockCourseService.Object, _mockSqlQueryDispatcher.Object, _mockCurrentUserProvider.Object, _mockProviderContextProvider.Object);
            var learnAimRef = "00214511";
            var expectedCourseType = CourseType.TLevels;
            ArrangeObjects(controller, learnAimRef);

            // Act
            var result = controller.AcceptAndPublish();

            // Assert
            var redirectActionResult = result.Result as RedirectToActionResult;
            Assert.NotNull(redirectActionResult);

            _mockSqlQueryDispatcher.Verify(p => p.ExecuteQuery(It.Is<CreateCourse>(p => p.CourseType == expectedCourseType)), Times.Once);
        }

        [Fact]
        public void AcceptAndPublish_WhenLARSCategoryRefIs3AndLearnAimRefTitleDoesNotStartWithTLevel_AddsCourseWithCourseTypeAsNull()
        {
            // Arrange
            var controller = new AddCourseController(_mockCourseService.Object, _mockSqlQueryDispatcher.Object, _mockCurrentUserProvider.Object, _mockProviderContextProvider.Object);
            var learnAimRef = "00214512";
            CourseType? expectedCourseType = null;
            ArrangeObjects(controller, learnAimRef);

            // Act
            var result = controller.AcceptAndPublish();

            // Assert
            var redirectActionResult = result.Result as RedirectToActionResult;
            Assert.NotNull(redirectActionResult);

            _mockSqlQueryDispatcher.Verify(p => p.ExecuteQuery(It.Is<CreateCourse>(p => p.CourseType == expectedCourseType)), Times.Once);
        }

        [Fact]
        public void AcceptAndPublish_WhenLARSCategoryRefIs24_AddsCourseWithCourseTypeAsEssentialSkills()
        {
            // Arrange
            var controller = new AddCourseController(_mockCourseService.Object, _mockSqlQueryDispatcher.Object, _mockCurrentUserProvider.Object, _mockProviderContextProvider.Object);
            var learnAimRef = "0021452";
            var expectedCourseType = CourseType.EssentialSkills;
            ArrangeObjects(controller, learnAimRef);

            // Act
            var result = controller.AcceptAndPublish();

            // Assert
            var redirectActionResult = result.Result as RedirectToActionResult;
            Assert.NotNull(redirectActionResult);

            _mockSqlQueryDispatcher.Verify(p => p.ExecuteQuery(It.Is<CreateCourse>(p => p.CourseType == expectedCourseType)), Times.Once);
        }

        [Fact]
        public void AcceptAndPublish_WhenLARSCategoryRefIs29_AddsCourseWithCourseTypeAsEssentialSkills()
        {
            // Arrange
            var controller = new AddCourseController(_mockCourseService.Object, _mockSqlQueryDispatcher.Object, _mockCurrentUserProvider.Object, _mockProviderContextProvider.Object);
            var learnAimRef = "0021453";
            var expectedCourseType = CourseType.EssentialSkills;
            ArrangeObjects(controller, learnAimRef);

            // Act
            var result = controller.AcceptAndPublish();

            // Assert
            var redirectActionResult = result.Result as RedirectToActionResult;
            Assert.NotNull(redirectActionResult);

            _mockSqlQueryDispatcher.Verify(p => p.ExecuteQuery(It.Is<CreateCourse>(p => p.CourseType == expectedCourseType)), Times.Once);
        }

        [Fact]
        public void AcceptAndPublish_WhenLARSCategoryRefIs37AndLearnAimRefTitleContainsGCSEEnglishLanguageText_AddsCourseWithCourseTypeAsEssentialSkills()
        {
            // Arrange
            var controller = new AddCourseController(_mockCourseService.Object, _mockSqlQueryDispatcher.Object, _mockCurrentUserProvider.Object, _mockProviderContextProvider.Object);
            var learnAimRef = "00214541";
            var expectedCourseType = CourseType.EssentialSkills;
            ArrangeObjects(controller, learnAimRef);

            // Act
            var result = controller.AcceptAndPublish();

            // Assert
            var redirectActionResult = result.Result as RedirectToActionResult;
            Assert.NotNull(redirectActionResult);

            _mockSqlQueryDispatcher.Verify(p => p.ExecuteQuery(It.Is<CreateCourse>(p => p.CourseType == expectedCourseType)), Times.Once);
        }

        [Fact]
        public void AcceptAndPublish_WhenLARSCategoryRefIs37AndLearnAimRefTitleContainsGCSEEnglishLiteratureText_AddsCourseWithCourseTypeAsEssentialSkills()
        {
            // Arrange
            var controller = new AddCourseController(_mockCourseService.Object, _mockSqlQueryDispatcher.Object, _mockCurrentUserProvider.Object, _mockProviderContextProvider.Object);
            var learnAimRef = "00214542";
            var expectedCourseType = CourseType.EssentialSkills;
            ArrangeObjects(controller, learnAimRef);

            // Act
            var result = controller.AcceptAndPublish();

            // Assert
            var redirectActionResult = result.Result as RedirectToActionResult;
            Assert.NotNull(redirectActionResult);

            _mockSqlQueryDispatcher.Verify(p => p.ExecuteQuery(It.Is<CreateCourse>(p => p.CourseType == expectedCourseType)), Times.Once);
        }

        [Fact]
        public void AcceptAndPublish_WhenLARSCategoryRefIs37AndLearnAimRefTitleDoesNotContainGCSEText_AddsCourseWithCourseTypeAsNull()
        {
            // Arrange
            var controller = new AddCourseController(_mockCourseService.Object, _mockSqlQueryDispatcher.Object, _mockCurrentUserProvider.Object, _mockProviderContextProvider.Object);
            var learnAimRef = "00214543";
            CourseType? expectedCourseType = null;
            ArrangeObjects(controller, learnAimRef);

            // Act
            var result = controller.AcceptAndPublish();

            // Assert
            var redirectActionResult = result.Result as RedirectToActionResult;
            Assert.NotNull(redirectActionResult);

            _mockSqlQueryDispatcher.Verify(p => p.ExecuteQuery(It.Is<CreateCourse>(p => p.CourseType == expectedCourseType)), Times.Once);
        }

        [Fact]
        public void AcceptAndPublish_WhenLARSCategoryRefIs39_AddsCourseWithCourseTypeAsEssentialSkills()
        {
            // Arrange
            var controller = new AddCourseController(_mockCourseService.Object, _mockSqlQueryDispatcher.Object, _mockCurrentUserProvider.Object, _mockProviderContextProvider.Object);
            var learnAimRef = "0021455";
            var expectedCourseType = CourseType.EssentialSkills;
            ArrangeObjects(controller, learnAimRef);

            // Act
            var result = controller.AcceptAndPublish();

            // Assert
            var redirectActionResult = result.Result as RedirectToActionResult;
            Assert.NotNull(redirectActionResult);

            _mockSqlQueryDispatcher.Verify(p => p.ExecuteQuery(It.Is<CreateCourse>(p => p.CourseType == expectedCourseType)), Times.Once);
        }

        [Fact]
        public void AcceptAndPublish_WhenLARSCategoryRefIs40AndLearnAimRefTitleContainsESOL_AddsCourseWithCourseTypeAsEssentialSkills()
        {
            // Arrange
            var controller = new AddCourseController(_mockCourseService.Object, _mockSqlQueryDispatcher.Object, _mockCurrentUserProvider.Object, _mockProviderContextProvider.Object);
            var learnAimRef = "00214561";
            var expectedCourseType = CourseType.EssentialSkills;
            ArrangeObjects(controller, learnAimRef);

            // Act
            var result = controller.AcceptAndPublish();

            // Assert
            var redirectActionResult = result.Result as RedirectToActionResult;
            Assert.NotNull(redirectActionResult);

            _mockSqlQueryDispatcher.Verify(p => p.ExecuteQuery(It.Is<CreateCourse>(p => p.CourseType == expectedCourseType)), Times.Once);
        }

        [Fact]
        public void AcceptAndPublish_WhenLARSCategoryRefIs40AndLearnAimRefTitleContainsGCSEEnglishLanguageText_AddsCourseWithCourseTypeAsEssentialSkills()
        {
            // Arrange
            var controller = new AddCourseController(_mockCourseService.Object, _mockSqlQueryDispatcher.Object, _mockCurrentUserProvider.Object, _mockProviderContextProvider.Object);
            var learnAimRef = "00214562";
            var expectedCourseType = CourseType.EssentialSkills;
            ArrangeObjects(controller, learnAimRef);

            // Act
            var result = controller.AcceptAndPublish();

            // Assert
            var redirectActionResult = result.Result as RedirectToActionResult;
            Assert.NotNull(redirectActionResult);

            _mockSqlQueryDispatcher.Verify(p => p.ExecuteQuery(It.Is<CreateCourse>(p => p.CourseType == expectedCourseType)), Times.Once);
        }

        [Fact]
        public void AcceptAndPublish_WhenLARSCategoryRefIs40AndLearnAimRefTitleContainsGCSEEnglishLiteratureText_AddsCourseWithCourseTypeAsEssentialSkills()
        {
            // Arrange
            var controller = new AddCourseController(_mockCourseService.Object, _mockSqlQueryDispatcher.Object, _mockCurrentUserProvider.Object, _mockProviderContextProvider.Object);
            var learnAimRef = "00214563";
            var expectedCourseType = CourseType.EssentialSkills;
            ArrangeObjects(controller, learnAimRef);

            // Act
            var result = controller.AcceptAndPublish();

            // Assert
            var redirectActionResult = result.Result as RedirectToActionResult;
            Assert.NotNull(redirectActionResult);

            _mockSqlQueryDispatcher.Verify(p => p.ExecuteQuery(It.Is<CreateCourse>(p => p.CourseType == expectedCourseType)), Times.Once);
        }

        [Fact]
        public void AcceptAndPublish_WhenLARSCategoryRefIs40AndLearnAimRefTitleDoesNotContainESOL_AddsCourseWithCourseTypeAsNull()
        {
            // Arrange
            var controller = new AddCourseController(_mockCourseService.Object, _mockSqlQueryDispatcher.Object, _mockCurrentUserProvider.Object, _mockProviderContextProvider.Object);
            var learnAimRef = "00214564";
            CourseType? expectedCourseType = null;
            ArrangeObjects(controller, learnAimRef);

            // Act
            var result = controller.AcceptAndPublish();

            // Assert
            var redirectActionResult = result.Result as RedirectToActionResult;
            Assert.NotNull(redirectActionResult);

            _mockSqlQueryDispatcher.Verify(p => p.ExecuteQuery(It.Is<CreateCourse>(p => p.CourseType == expectedCourseType)), Times.Once);
        }

        [Fact]
        public void AcceptAndPublish_WhenLARSCategoryRefIs42_AddsCourseWithCourseTypeAsEssentialSkills()
        {
            // Arrange
            var controller = new AddCourseController(_mockCourseService.Object, _mockSqlQueryDispatcher.Object, _mockCurrentUserProvider.Object, _mockProviderContextProvider.Object);
            var learnAimRef = "0021457";
            var expectedCourseType = CourseType.EssentialSkills;
            ArrangeObjects(controller, learnAimRef);

            // Act
            var result = controller.AcceptAndPublish();

            // Assert
            var redirectActionResult = result.Result as RedirectToActionResult;
            Assert.NotNull(redirectActionResult);

            _mockSqlQueryDispatcher.Verify(p => p.ExecuteQuery(It.Is<CreateCourse>(p => p.CourseType == expectedCourseType)), Times.Once);
        }

        [Fact]
        public void AcceptAndPublish_WhenLARSCategoryRefIs45_AddsCourseWithCourseTypeAsFreeCoursesForJobs()
        {
            // Arrange
            var controller = new AddCourseController(_mockCourseService.Object, _mockSqlQueryDispatcher.Object, _mockCurrentUserProvider.Object, _mockProviderContextProvider.Object);
            var learnAimRef = "0021458";
            var expectedCourseType = CourseType.FreeCoursesForJobs;
            ArrangeObjects(controller, learnAimRef);

            // Act
            var result = controller.AcceptAndPublish();

            // Assert
            var redirectActionResult = result.Result as RedirectToActionResult;
            Assert.NotNull(redirectActionResult);

            _mockSqlQueryDispatcher.Verify(p => p.ExecuteQuery(It.Is<CreateCourse>(p => p.CourseType == expectedCourseType)), Times.Once);
        }

        [Fact]
        public void AcceptAndPublish_WhenLARSCategoryRefIs46_AddsCourseWithCourseTypeAsFreeCoursesForJobs()
        {
            // Arrange
            var controller = new AddCourseController(_mockCourseService.Object, _mockSqlQueryDispatcher.Object, _mockCurrentUserProvider.Object, _mockProviderContextProvider.Object);
            var learnAimRef = "0021459";
            var expectedCourseType = CourseType.FreeCoursesForJobs;
            ArrangeObjects(controller, learnAimRef);

            // Act
            var result = controller.AcceptAndPublish();

            // Assert
            var redirectActionResult = result.Result as RedirectToActionResult;
            Assert.NotNull(redirectActionResult);

            _mockSqlQueryDispatcher.Verify(p => p.ExecuteQuery(It.Is<CreateCourse>(p => p.CourseType == expectedCourseType)), Times.Once);
        }

        [Fact]
        public void AcceptAndPublish_WhenLARSCategoryRefIs48_AddsCourseWithCourseTypeAsFreeCoursesForJobs()
        {
            // Arrange
            var controller = new AddCourseController(_mockCourseService.Object, _mockSqlQueryDispatcher.Object, _mockCurrentUserProvider.Object, _mockProviderContextProvider.Object);
            var learnAimRef = "0021460";
            var expectedCourseType = CourseType.FreeCoursesForJobs;
            ArrangeObjects(controller, learnAimRef);

            // Act
            var result = controller.AcceptAndPublish();

            // Assert
            var redirectActionResult = result.Result as RedirectToActionResult;
            Assert.NotNull(redirectActionResult);

            _mockSqlQueryDispatcher.Verify(p => p.ExecuteQuery(It.Is<CreateCourse>(p => p.CourseType == expectedCourseType)), Times.Once);
        }

        [Fact]
        public void AcceptAndPublish_WhenLARSCategoryRefIs49_AddsCourseWithCourseTypeAsFreeCoursesForJobs()
        {
            // Arrange
            var controller = new AddCourseController(_mockCourseService.Object, _mockSqlQueryDispatcher.Object, _mockCurrentUserProvider.Object, _mockProviderContextProvider.Object);
            var learnAimRef = "0021461";
            var expectedCourseType = CourseType.FreeCoursesForJobs;
            ArrangeObjects(controller, learnAimRef);

            // Act
            var result = controller.AcceptAndPublish();

            // Assert
            var redirectActionResult = result.Result as RedirectToActionResult;
            Assert.NotNull(redirectActionResult);

            _mockSqlQueryDispatcher.Verify(p => p.ExecuteQuery(It.Is<CreateCourse>(p => p.CourseType == expectedCourseType)), Times.Once);
        }

        [Fact]
        public void AcceptAndPublish_WhenLARSCategoryRefIs55_AddsCourseWithCourseTypeAsHTQs()
        {
            // Arrange
            var controller = new AddCourseController(_mockCourseService.Object, _mockSqlQueryDispatcher.Object, _mockCurrentUserProvider.Object, _mockProviderContextProvider.Object);
            var learnAimRef = "0021462";
            var expectedCourseType = CourseType.HTQs;
            ArrangeObjects(controller, learnAimRef);

            // Act
            var result = controller.AcceptAndPublish();

            // Assert
            var redirectActionResult = result.Result as RedirectToActionResult;
            Assert.NotNull(redirectActionResult);

            _mockSqlQueryDispatcher.Verify(p => p.ExecuteQuery(It.Is<CreateCourse>(p => p.CourseType == expectedCourseType)), Times.Once);
        }

        [Fact]
        public void AcceptAndPublish_WhenLARSCategoryRefIs63_AddsCourseWithCourseTypeAsMultiply()
        {
            // Arrange
            var controller = new AddCourseController(_mockCourseService.Object, _mockSqlQueryDispatcher.Object, _mockCurrentUserProvider.Object, _mockProviderContextProvider.Object);
            var learnAimRef = "0021463";
            var expectedCourseType = CourseType.Multiply;
            ArrangeObjects(controller, learnAimRef);

            // Act
            var result = controller.AcceptAndPublish();

            // Assert
            var redirectActionResult = result.Result as RedirectToActionResult;
            Assert.NotNull(redirectActionResult);

            _mockSqlQueryDispatcher.Verify(p => p.ExecuteQuery(It.Is<CreateCourse>(p => p.CourseType == expectedCourseType)), Times.Once);
        }

        private void ArrangeObjects(AddCourseController controller, string learnAimRef)
        {
            SetSessionObjects(controller, learnAimRef);

            var providerContext = new ProviderContext(new ProviderInfo() { ProviderId = Guid.NewGuid() });
            _mockProviderContextProvider.Setup(m => m.GetProviderContext(true)).Returns(providerContext);

            var larsCourseTypesList = new List<LarsCourseType>()
            {
                { new LarsCourseType { LearnAimRef = "00214511", CategoryRef = "3", CourseType = CourseType.TLevels, LearnAimRefTitle = "T Level Title" }},
                { new LarsCourseType { LearnAimRef = "00214512", CategoryRef = "3", CourseType = CourseType.TLevels, LearnAimRefTitle = "Title" }},
                { new LarsCourseType { LearnAimRef = "0021452", CategoryRef = "24", CourseType = CourseType.EssentialSkills, LearnAimRefTitle = "Title" }},
                { new LarsCourseType { LearnAimRef = "0021453", CategoryRef = "29", CourseType = CourseType.EssentialSkills, LearnAimRefTitle = "Title" }},                                
                { new LarsCourseType { LearnAimRef = "00214541", CategoryRef = "37", CourseType = CourseType.EssentialSkills, LearnAimRefTitle = "Title GCSE (9-1) in English Language Title" }},
                { new LarsCourseType { LearnAimRef = "00214542", CategoryRef = "37", CourseType = CourseType.EssentialSkills, LearnAimRefTitle = "Title GCSE (9-1) in English Literature Title" }},
                { new LarsCourseType { LearnAimRef = "00214543", CategoryRef = "37", CourseType = CourseType.EssentialSkills, LearnAimRefTitle = "Title" }},
                { new LarsCourseType { LearnAimRef = "0021455", CategoryRef = "39", CourseType = CourseType.EssentialSkills, LearnAimRefTitle = "Title" }},
                { new LarsCourseType { LearnAimRef = "00214561", CategoryRef = "40", CourseType = CourseType.EssentialSkills, LearnAimRefTitle = "Title ESOL Title" }},
                { new LarsCourseType { LearnAimRef = "00214562", CategoryRef = "40", CourseType = CourseType.EssentialSkills, LearnAimRefTitle = "Title GCSE (9-1) in English Language Title" }},
                { new LarsCourseType { LearnAimRef = "00214563", CategoryRef = "40", CourseType = CourseType.EssentialSkills, LearnAimRefTitle = "Title GCSE (9-1) in English Literature Title" }},
                { new LarsCourseType { LearnAimRef = "00214564", CategoryRef = "40", CourseType = CourseType.EssentialSkills, LearnAimRefTitle = "Title" }},
                { new LarsCourseType { LearnAimRef = "0021457", CategoryRef = "42", CourseType = CourseType.EssentialSkills, LearnAimRefTitle = "Title" }},                
                { new LarsCourseType { LearnAimRef = "0021458", CategoryRef = "45", CourseType = CourseType.FreeCoursesForJobs, LearnAimRefTitle = "Title" }},
                { new LarsCourseType { LearnAimRef = "0021459", CategoryRef = "46", CourseType = CourseType.FreeCoursesForJobs, LearnAimRefTitle = "Title" }},
                { new LarsCourseType { LearnAimRef = "0021460", CategoryRef = "48", CourseType = CourseType.FreeCoursesForJobs, LearnAimRefTitle = "Title" }},
                { new LarsCourseType { LearnAimRef = "0021461", CategoryRef = "49", CourseType = CourseType.FreeCoursesForJobs, LearnAimRefTitle = "Title" }},
                { new LarsCourseType { LearnAimRef = "0021462", CategoryRef = "55", CourseType = CourseType.HTQs, LearnAimRefTitle = "Title" }},
                { new LarsCourseType { LearnAimRef = "0021463", CategoryRef = "63", CourseType = CourseType.Multiply, LearnAimRefTitle = "Title" }}
            };

            var larsCourseTypesReadOnlyList = new ReadOnlyCollection<LarsCourseType>(larsCourseTypesList.Where(l => l.LearnAimRef == learnAimRef).ToList());
            _mockSqlQueryDispatcher.Setup(m => m.ExecuteQuery(It.IsAny<GetLarsCourseType>()))
                .ReturnsAsync(larsCourseTypesReadOnlyList);
        }

        private static void SetSessionObjects(AddCourseController controller, string learnAimRef)
        {
            var addCourseSection1RequestModelFaker = new Faker<AddCourseSection1RequestModel>()
                                        .RuleFor(u => u.CourseId, f => f.Random.Guid())
                                        .RuleFor(u => u.EntryRequirements, f => f.Lorem.Text())
                                        .RuleFor(u => u.HowYouWillLearn, f => f.Lorem.Text());
            var addCourseSection1RequestModel = addCourseSection1RequestModelFaker.Generate();

            Faker<AddCourseRequestModel> addCourseRequestModelFaker = new Faker<AddCourseRequestModel>()
                .RuleFor(u => u.CourseId, f => f.Random.Guid())
                .RuleFor(u => u.CourseName, f => f.Lorem.Text())
                .RuleFor(u => u.DeliveryMode, CourseDeliveryMode.Online)
                .RuleFor(u => u.StartDateType, "FlexibleStartDate")
                .RuleFor(u => u.Url, f => f.Lorem.Text())
                .RuleFor(u => u.DurationLength, 8)
                .RuleFor(u => u.DurationUnit, CourseDurationUnit.Hours);
            var addCourseRequestModel = addCourseRequestModelFaker.Generate();

            MockHttpSession mockHttpSession = new MockHttpSession();
            mockHttpSession.SetString("LearnAimRef", learnAimRef);
            mockHttpSession.SetString("NotionalNVQLevelv2", "2");
            mockHttpSession.SetString("AwardOrgCode", "test code");
            mockHttpSession.SetString("LearnAimRefTitle", "Test title");
            mockHttpSession.SetString("LearnAimRefTypeDesc", "test lars description");
            mockHttpSession.SetObject("AddCourseSection1", addCourseSection1RequestModel);
            mockHttpSession.SetObject("AddCourseSection2", addCourseRequestModel);

            controller.ControllerContext.HttpContext = new DefaultHttpContext
            {
                Session = mockHttpSession
            };
        }
    }
}
