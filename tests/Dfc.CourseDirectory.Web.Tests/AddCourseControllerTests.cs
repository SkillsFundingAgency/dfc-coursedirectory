using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        public void AcceptAndPublish_WhenLARSCategoryRefIs39_AddsCourseWithCourseTypeAsEssentialSkills()
        {
            // Arrange
            var controller = new AddCourseController(_mockCourseService.Object, _mockSqlQueryDispatcher.Object, _mockCurrentUserProvider.Object, _mockProviderContextProvider.Object);
            var learnAimRef = "0021451";
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
            var learnAimRef = "00214521";
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
            var learnAimRef = "00214522";
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
        public void AcceptAndPublish_WhenLARSCategoryRefIs3AndLearnAimRefTitleStartsWithTLevel_AddsCourseWithCourseTypeAsTLevels()
        {
            // Arrange
            var controller = new AddCourseController(_mockCourseService.Object, _mockSqlQueryDispatcher.Object, _mockCurrentUserProvider.Object, _mockProviderContextProvider.Object);
            var learnAimRef = "00214541";
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
            var learnAimRef = "00214542";
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
        public void AcceptAndPublish_WhenLARSCategoryRefIs55_AddsCourseWithCourseTypeAsHTQs()
        {
            // Arrange
            var controller = new AddCourseController(_mockCourseService.Object, _mockSqlQueryDispatcher.Object, _mockCurrentUserProvider.Object, _mockProviderContextProvider.Object);
            var learnAimRef = "0021455";
            var expectedCourseType = CourseType.HTQs;
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

            var providerContext = new ProviderContext(new ProviderInfo() { ProviderId = Guid.NewGuid() });
            _mockProviderContextProvider.Setup(m => m.GetProviderContext(true)).Returns(providerContext);

            var larsCourseTypesList = new List<LarsCourseType>()
            {
                { new LarsCourseType { LearnAimRef = "0021451", CategoryRef = "39", CourseType = CourseType.EssentialSkills, LearnAimRefTitle = "Title" }},
                { new LarsCourseType { LearnAimRef = "00214521", CategoryRef = "40", CourseType = CourseType.EssentialSkills, LearnAimRefTitle = "Title ESOL Title" }},
                { new LarsCourseType { LearnAimRef = "00214522", CategoryRef = "40", CourseType = CourseType.EssentialSkills, LearnAimRefTitle = "Title" }},
                { new LarsCourseType { LearnAimRef = "0021453", CategoryRef = "42", CourseType = CourseType.EssentialSkills, LearnAimRefTitle = "Title" }},
                { new LarsCourseType { LearnAimRef = "00214541", CategoryRef = "3", CourseType = CourseType.TLevels, LearnAimRefTitle = "T Level Title" }},
                { new LarsCourseType { LearnAimRef = "00214542", CategoryRef = "3", CourseType = CourseType.TLevels, LearnAimRefTitle = "Title" }},
                { new LarsCourseType { LearnAimRef = "0021455", CategoryRef = "55", CourseType = CourseType.HTQs, LearnAimRefTitle = "Title" }},
                { new LarsCourseType { LearnAimRef = "0021456", CategoryRef = "45", CourseType = CourseType.FreeCourseForJobs, LearnAimRefTitle = "Title" }},
                { new LarsCourseType { LearnAimRef = "0021457", CategoryRef = "46", CourseType = CourseType.FreeCourseForJobs, LearnAimRefTitle = "Title" }},
                { new LarsCourseType { LearnAimRef = "0021458", CategoryRef = "48", CourseType = CourseType.FreeCourseForJobs, LearnAimRefTitle = "Title" }},
                { new LarsCourseType { LearnAimRef = "0021459", CategoryRef = "49", CourseType = CourseType.FreeCourseForJobs, LearnAimRefTitle = "Title" }},
                { new LarsCourseType { LearnAimRef = "0021460", CategoryRef = "63", CourseType = CourseType.Multiply, LearnAimRefTitle = "Title" }}
            };


            var larsCourseTypesReadOnlyList = new ReadOnlyCollection<LarsCourseType>(larsCourseTypesList.Where(l => l.LearnAimRef == learnAimRef).ToList());
            _mockSqlQueryDispatcher.Setup(m => m.ExecuteQuery(It.IsAny<GetLarsCourseType>()))
                .ReturnsAsync(larsCourseTypesReadOnlyList);

            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.Session = mockHttpSession;
        }
    }

    public class MockHttpSession : ISession
    {
        readonly Dictionary<string, object> _sessionStorage = new Dictionary<string, object>();
        string ISession.Id => throw new NotImplementedException();
        bool ISession.IsAvailable => throw new NotImplementedException();
        IEnumerable<string> ISession.Keys => _sessionStorage.Keys;
        void ISession.Clear()
        {
            _sessionStorage.Clear();
        }
        Task ISession.CommitAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
        Task ISession.LoadAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
        void ISession.Remove(string key)
        {
            _sessionStorage.Remove(key);
        }
        void ISession.Set(string key, byte[] value)
        {
            _sessionStorage[key] = Encoding.UTF8.GetString(value);
        }
        bool ISession.TryGetValue(string key, out byte[] value)
        {
            if (_sessionStorage[key] != null)
            {
                value = Encoding.ASCII.GetBytes(_sessionStorage[key].ToString());
                return true;
            }
            value = null;
            return false;
        }
    }
}
