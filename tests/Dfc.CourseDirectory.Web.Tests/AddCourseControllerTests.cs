using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.Services;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.WebV2.Security;
using Dfc.CourseDirectory.WebV2;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
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
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Web.RequestModels;
using Newtonsoft.Json;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.SelectVenue;
using Dfc.CourseDirectory.Services.Models.Regions;
using System.Linq;

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
        private const string SessionAddCourseSection2 = "AddCourseSection2";
        private const string SessionAddCourseSection1 = "AddCourseSection1";
        protected const string SessionVenues = "Venues";
        protected const string SessionRegions = "Regions";
        private readonly Mock<ICourseService> _mockCourseService;
        private readonly Mock<ISession> _mockSession;
        private readonly Mock<ISqlQueryDispatcher> _mockSqlQueryDispatcher;
        private readonly Mock<ICurrentUserProvider> _mockCurrentUserProvider;
        private readonly Mock<IProviderContextProvider> _mockProviderContextProvider;
        private readonly Mock<ICourseTypeService> _mockCourseTypeService;
        private readonly Mock<IWebRiskService> _mockWebRiskService;
        private readonly Mock<IConfiguration> _mockConfiguration;

        public AddCourseControllerTests()
        {
            _mockCourseService = new Mock<ICourseService>();
            _mockSession = new Mock<ISession>();
            _mockSqlQueryDispatcher = new Mock<ISqlQueryDispatcher>();
            _mockCurrentUserProvider = new Mock<ICurrentUserProvider>();
            _mockProviderContextProvider = new Mock<IProviderContextProvider>();
            _mockCourseTypeService = new Mock<ICourseTypeService>();
            _mockWebRiskService = new Mock<IWebRiskService>();
            _mockConfiguration = new Mock<IConfiguration>();
        }

        #region Tests for AddCourse action method
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
        #endregion

        #region Tests for AddCourseDetails action method
        [Fact]
        public async Task AddCourseDetails_WhenUkprnIsNull_RedirectsToHomePageWithErrorMessage()
        {
            // Arrange
            var addCourseController = GetController();            

            // Act
            var result = await addCourseController.AddCourseDetails(null) as RedirectToActionResult;

            // Assert            
            Assert.NotNull(result);

            Assert.Equal("Home", result.ControllerName);
            Assert.Equal("Index", result.ActionName);
            Assert.Equal("Please select a Provider.", result.RouteValues["errmsg"]);
        }

        [Fact]
        public async Task AddCourseDetails_WhenSessionAddCourseSection2IsNullAndCourseIsLars_ReturnsViewModelWithDefaultValues()
        {
            // Arrange
            var addCourseController = GetController();
            var ukprn = Encoding.UTF8.GetBytes("123456");
            var sessionUkprn = "UKPRN";

            _mockSession.Setup(m => m.TryGetValue(sessionUkprn, out ukprn)).Returns(true);

            var providerInfo = new ProviderInfo { ProviderId = Guid.NewGuid(), ProviderName = Faker.Company.Name() };
            var providerContext = new ProviderContext(providerInfo);
            _mockProviderContextProvider.Setup(m => m.GetProviderContext(true)).Returns(providerContext);

            var learnAimRefValue = Encoding.UTF8.GetBytes("6152348");
            _mockSession.Setup(m => m.TryGetValue(SessionLearnAimRef, out learnAimRefValue)).Returns(true);

            var sectors = new Faker<Sector>().Generate(3);
            _mockSqlQueryDispatcher.Setup(m => m.ExecuteQuery(It.IsAny<GetSectors>())).ReturnsAsync(sectors);

            var venues = new Faker<Venue>().Generate(2);
            _mockSqlQueryDispatcher.Setup(m => m.ExecuteQuery(It.IsAny<GetVenuesByProvider>())).ReturnsAsync(venues);

            // Act
            var viewResult = await addCourseController.AddCourseDetails(null) as ViewResult;

            // Assert            
            Assert.NotNull(viewResult);

            var viewModel = viewResult.Model as AddCourseDetailsViewModel;
            Assert.NotNull(viewModel);

            Assert.NotNull(viewModel.LearnAimRef);
            Assert.Null(viewModel.CourseType);
            Assert.Null(viewModel.SectorId);
            Assert.Null(viewModel.EducationLevel);            

            _mockSession.Verify(m => m.TryGetValue(sessionUkprn, out ukprn), Times.AtLeastOnce);
            _mockProviderContextProvider.Verify(m => m.GetProviderContext(true), Times.AtLeastOnce);
            _mockSqlQueryDispatcher.Verify(m => m.ExecuteQuery(It.IsAny<GetVenuesByProvider>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task AddCourseDetails_WhenSessionAddCourseSection2IsNullAndCourseIsNonLars_ReturnsViewModelWithDefaultValues()
        {
            // Arrange
            var addCourseController = GetController();
            var ukprn = Encoding.UTF8.GetBytes("123456");
            var sessionUkprn = "UKPRN";

            _mockSession.Setup(m => m.TryGetValue(sessionUkprn, out ukprn)).Returns(true);

            var providerInfo = new ProviderInfo { ProviderId = Guid.NewGuid(), ProviderName = Faker.Company.Name() };
            var providerContext = new ProviderContext(providerInfo);
            _mockProviderContextProvider.Setup(m => m.GetProviderContext(true)).Returns(providerContext);

            var sectors = new Faker<Sector>().Generate(3);
            _mockSqlQueryDispatcher.Setup(m => m.ExecuteQuery(It.IsAny<GetSectors>())).ReturnsAsync(sectors);

            var venues = new Faker<Venue>().Generate(2);
            _mockSqlQueryDispatcher.Setup(m => m.ExecuteQuery(It.IsAny<GetVenuesByProvider>())).ReturnsAsync(venues);

            var trueValue = Encoding.UTF8.GetBytes("true");
            _mockSession.Setup(m => m.TryGetValue(SessionNonLarsCourse, out trueValue)).Returns(true);

            // Act
            var viewResult = await addCourseController.AddCourseDetails(null) as ViewResult;

            // Assert            
            Assert.NotNull(viewResult);

            var viewModel = viewResult.Model as AddCourseDetailsViewModel;
            Assert.NotNull(viewModel);

            Assert.Equal(CourseType.SkillsBootcamp, viewModel.CourseType);
            Assert.Equal(1, viewModel.SectorId);
            Assert.Equal(EducationLevel.EntryLevel, viewModel.EducationLevel);
            Assert.Null(viewModel.LearnAimRef);

            _mockSession.Verify(m => m.TryGetValue(sessionUkprn, out ukprn), Times.AtLeastOnce);
            _mockProviderContextProvider.Verify(m => m.GetProviderContext(true), Times.AtLeastOnce);
            _mockSqlQueryDispatcher.Verify(m => m.ExecuteQuery(It.IsAny<GetVenuesByProvider>()), Times.AtLeastOnce);
            _mockSession.Verify(m => m.TryGetValue(SessionNonLarsCourse, out trueValue), Times.AtLeastOnce);
        }

        [Fact]
        public async Task AddCourseDetails_WhenSessionAddCourseSection2IsNotNullAndCourseIsLars_ReturnsViewModelWithDefaultValues()
        {
            // Arrange
            var addCourseController = GetController();
            var ukprn = Encoding.UTF8.GetBytes("123456");
            var sessionUkprn = "UKPRN";

            _mockSession.Setup(m => m.TryGetValue(sessionUkprn, out ukprn)).Returns(true);

            var providerInfo = new ProviderInfo { ProviderId = Guid.NewGuid(), ProviderName = Faker.Company.Name() };
            var providerContext = new ProviderContext(providerInfo);
            _mockProviderContextProvider.Setup(m => m.GetProviderContext(true)).Returns(providerContext);

            var learnAimRefValue = Encoding.UTF8.GetBytes("6152348");
            _mockSession.Setup(m => m.TryGetValue(SessionLearnAimRef, out learnAimRefValue)).Returns(true);

            var model = new Faker<AddCourseRequestModel>()
                .RuleFor(c => c.CourseId, f => Guid.NewGuid())
                .RuleFor(c => c.CourseRunId, f => Guid.NewGuid())
                .RuleFor(c => c.CourseName, f => f.Lorem.Sentence())
                .RuleFor(c => c.CourseProviderReference, f => f.Lorem.Word())
                .RuleFor(c => c.DeliveryMode, f => CourseDeliveryMode.ClassroomBased)
                .RuleFor(c => c.Cost, f => 20)
                .Generate();

            var modelValue = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(model));
            _mockSession.Setup(m => m.TryGetValue(SessionAddCourseSection2, out modelValue)).Returns(true);

            var sectors = new Faker<Sector>().Generate(3);
            _mockSqlQueryDispatcher.Setup(m => m.ExecuteQuery(It.IsAny<GetSectors>())).ReturnsAsync(sectors);

            var venues = new Faker<Venue>().Generate(2);
            _mockSqlQueryDispatcher.Setup(m => m.ExecuteQuery(It.IsAny<GetVenuesByProvider>())).ReturnsAsync(venues);

            // Act
            var viewResult = await addCourseController.AddCourseDetails(null) as ViewResult;

            // Assert            
            Assert.NotNull(viewResult);

            var viewModel = viewResult.Model as AddCourseDetailsViewModel;
            Assert.NotNull(viewModel);

            Assert.NotNull(viewModel.LearnAimRef);
            Assert.Null(viewModel.CourseType);
            Assert.Null(viewModel.SectorId);
            Assert.Null(viewModel.EducationLevel);

            _mockSession.Verify(m => m.TryGetValue(sessionUkprn, out ukprn), Times.AtLeastOnce);
            _mockProviderContextProvider.Verify(m => m.GetProviderContext(true), Times.AtLeastOnce);
            _mockSqlQueryDispatcher.Verify(m => m.ExecuteQuery(It.IsAny<GetVenuesByProvider>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task AddCourseDetails_WhenSessionAddCourseSection2IsNotNullAndCourseIsNonLars_ReturnsViewModelWithDefaultValues()
        {
            // Arrange
            var addCourseController = GetController();
            var ukprn = Encoding.UTF8.GetBytes("123456");
            var sessionUkprn = "UKPRN";

            _mockSession.Setup(m => m.TryGetValue(sessionUkprn, out ukprn)).Returns(true);

            var providerInfo = new ProviderInfo { ProviderId = Guid.NewGuid(), ProviderName = Faker.Company.Name() };
            var providerContext = new ProviderContext(providerInfo);
            _mockProviderContextProvider.Setup(m => m.GetProviderContext(true)).Returns(providerContext);

            var sectors = new Faker<Sector>().Generate(3);
            _mockSqlQueryDispatcher.Setup(m => m.ExecuteQuery(It.IsAny<GetSectors>())).ReturnsAsync(sectors);

            var venues = new Faker<Venue>().Generate(2);
            _mockSqlQueryDispatcher.Setup(m => m.ExecuteQuery(It.IsAny<GetVenuesByProvider>())).ReturnsAsync(venues);

            var trueValue = Encoding.UTF8.GetBytes("true");
            _mockSession.Setup(m => m.TryGetValue(SessionNonLarsCourse, out trueValue)).Returns(true);

            var model = GetAddCourseSection2RequestModel();
            model.SelectedVenues = new Guid[] { Guid.Empty };

            var modelValue = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(model));
            _mockSession.Setup(m => m.TryGetValue(SessionAddCourseSection2, out modelValue)).Returns(true);

            // Act
            var viewResult = await addCourseController.AddCourseDetails(null) as ViewResult;

            // Assert            
            Assert.NotNull(viewResult);

            var viewModel = viewResult.Model as AddCourseDetailsViewModel;
            Assert.NotNull(viewModel);

            Assert.Equal(CourseType.SkillsBootcamp, viewModel.CourseType);
            Assert.Equal(2, viewModel.SectorId);
            Assert.Equal(EducationLevel.Two, viewModel.EducationLevel);
            Assert.Null(viewModel.LearnAimRef);

            _mockSession.Verify(m => m.TryGetValue(sessionUkprn, out ukprn), Times.AtLeastOnce);
            _mockProviderContextProvider.Verify(m => m.GetProviderContext(true), Times.AtLeastOnce);
            _mockSqlQueryDispatcher.Verify(m => m.ExecuteQuery(It.IsAny<GetVenuesByProvider>()), Times.AtLeastOnce);
            _mockSession.Verify(m => m.TryGetValue(SessionNonLarsCourse, out trueValue), Times.AtLeastOnce);
        }
        #endregion

        #region Tests for AddCourseRun action method
        [Fact]
        public async Task AddCourseRun_WhenCalled_SetsSessionAddCourseSection2ModelObjectAndReturnsAddCourseSummaryViewModel()
        {
            // Arrange
            var controller = GetController();

            var model = GetAddCourseSection2RequestModel();
            var section1RequestModel = GetAddCourseSection1RequestModel();
            var venueModel = GetSelectVenueModel();
            var regionModel = new SelectRegionModel();

            var section2ModelBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(model));
            _mockSession.Setup(m => m.Set(SessionAddCourseSection2, section2ModelBytes));

            var section1ModelBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(section1RequestModel));
            _mockSession.Setup(m => m.TryGetValue(SessionAddCourseSection1, out section1ModelBytes)).Returns(true);

            var venueModelBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(venueModel));
            _mockSession.Setup(m => m.TryGetValue(SessionVenues, out venueModelBytes)).Returns(true);            

            var regionModelBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(regionModel));
            _mockSession.Setup(m => m.TryGetValue(SessionRegions, out regionModelBytes)).Returns(true);

            var sectors = new Faker<Sector>().Generate(3);
            _mockSqlQueryDispatcher.Setup(m => m.ExecuteQuery(It.IsAny<GetSectors>())).ReturnsAsync(sectors);

            // Act
            var result = await controller.AddCourseRun(model) as ViewResult;

            // Assert            
            Assert.NotNull(result);

            var viewModel = result.Model as AddCourseSummaryViewModel;
            Assert.NotNull(viewModel);

            _mockSession.Verify(m => m.Set(SessionAddCourseSection2, It.IsAny<byte[]>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task AddCourseRun_WhenDeliveryModeIsClassroomBased_ReturnsViewModelWithAtleast1Venue()
        {
            // Arrange
            var controller = GetController();

            var venueId = Guid.NewGuid();
            var model = GetAddCourseSection2RequestModel();
            model.SelectedVenues = new Guid[] { venueId };

            var section1RequestModel = GetAddCourseSection1RequestModel();

            var venueModel = GetSelectVenueModel();
            venueModel.VenueItems[0].Id = venueId.ToString();

            var regionModel = new SelectRegionModel();

            var section2ModelBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(model));
            _mockSession.Setup(m => m.Set(SessionAddCourseSection2, section2ModelBytes));

            var section1ModelBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(section1RequestModel));
            _mockSession.Setup(m => m.TryGetValue(SessionAddCourseSection1, out section1ModelBytes)).Returns(true);

            var venueModelBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(venueModel));
            _mockSession.Setup(m => m.TryGetValue(SessionVenues, out venueModelBytes)).Returns(true);

            var regionModelBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(regionModel));
            _mockSession.Setup(m => m.TryGetValue(SessionRegions, out regionModelBytes)).Returns(true);

            var sectors = new Faker<Sector>().Generate(3);
            _mockSqlQueryDispatcher.Setup(m => m.ExecuteQuery(It.IsAny<GetSectors>())).ReturnsAsync(sectors);

            // Act
            var result = await controller.AddCourseRun(model) as ViewResult;

            // Assert            
            Assert.NotNull(result);

            var viewModel = result.Model as AddCourseSummaryViewModel;
            Assert.NotNull(viewModel);

            Assert.True(viewModel.Venues.Any());
            Assert.Null(viewModel.Regions);
        }

        [Fact]
        public async Task AddCourseRun_WhenDeliveryModeIsOnline_ReturnsViewModelWithNoVenueAndRegions()
        {
            // Arrange
            var controller = GetController();
            
            var model = GetAddCourseSection2RequestModel();
            model.DeliveryMode = CourseDeliveryMode.Online;

            var section1RequestModel = GetAddCourseSection1RequestModel();
            var venueModel = GetSelectVenueModel();
            var regionModel = new SelectRegionModel();

            var section2ModelBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(model));
            _mockSession.Setup(m => m.Set(SessionAddCourseSection2, section2ModelBytes));

            var section1ModelBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(section1RequestModel));
            _mockSession.Setup(m => m.TryGetValue(SessionAddCourseSection1, out section1ModelBytes)).Returns(true);

            var venueModelBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(venueModel));
            _mockSession.Setup(m => m.TryGetValue(SessionVenues, out venueModelBytes)).Returns(true);

            var regionModelBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(regionModel));
            _mockSession.Setup(m => m.TryGetValue(SessionRegions, out regionModelBytes)).Returns(true);

            var sectors = new Faker<Sector>().Generate(3);
            _mockSqlQueryDispatcher.Setup(m => m.ExecuteQuery(It.IsAny<GetSectors>())).ReturnsAsync(sectors);

            // Act
            var result = await controller.AddCourseRun(model) as ViewResult;

            // Assert            
            Assert.NotNull(result);

            var viewModel = result.Model as AddCourseSummaryViewModel;
            Assert.NotNull(viewModel);

            Assert.Null(viewModel.Venues);
            Assert.Null(viewModel.Regions);
        }

        [Fact]
        public async Task AddCourseRun_WhenDeliveryModeIsWorkBasedAndNationalPropertyIsTrue_ReturnsViewModelWithOnly1RegionAndItIsNational()
        {
            // Arrange
            var controller = GetController();
            
            var model = GetAddCourseSection2RequestModel();
            model.DeliveryMode = CourseDeliveryMode.WorkBased;
            model.National = true;

            var section1RequestModel = GetAddCourseSection1RequestModel();
            var venueModel = GetSelectVenueModel();
            var regionModel = new SelectRegionModel();

            var section2ModelBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(model));
            _mockSession.Setup(m => m.Set(SessionAddCourseSection2, section2ModelBytes));

            var section1ModelBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(section1RequestModel));
            _mockSession.Setup(m => m.TryGetValue(SessionAddCourseSection1, out section1ModelBytes)).Returns(true);

            var venueModelBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(venueModel));
            _mockSession.Setup(m => m.TryGetValue(SessionVenues, out venueModelBytes)).Returns(true);

            var regionModelBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(regionModel));
            _mockSession.Setup(m => m.TryGetValue(SessionRegions, out regionModelBytes)).Returns(true);

            var sectors = new Faker<Sector>().Generate(3);
            _mockSqlQueryDispatcher.Setup(m => m.ExecuteQuery(It.IsAny<GetSectors>())).ReturnsAsync(sectors);

            // Act
            var result = await controller.AddCourseRun(model) as ViewResult;

            // Assert            
            Assert.NotNull(result);

            var viewModel = result.Model as AddCourseSummaryViewModel;
            Assert.NotNull(viewModel);            
            
            var foundNationalRegion = viewModel.Regions.FirstOrDefault(v => v.Equals("National")) != null;
            Assert.True(foundNationalRegion);
        }

        [Fact]
        public async Task AddCourseRun_WhenDeliveryModeIsWorkBasedAndRegionsAreSelected_ReturnsViewModelWithSelectedRegions()
        {
            // Arrange
            var controller = GetController();
                        
            var model = GetAddCourseSection2RequestModel();
            model.DeliveryMode = CourseDeliveryMode.WorkBased;
            model.SelectedRegions = new string[] { "E12000001", "E08000023" };

            var section1RequestModel = GetAddCourseSection1RequestModel();
            var venueModel = GetSelectVenueModel();
            var regionModel = new SelectRegionModel();

            var section2ModelBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(model));
            _mockSession.Setup(m => m.Set(SessionAddCourseSection2, section2ModelBytes));

            var section1ModelBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(section1RequestModel));
            _mockSession.Setup(m => m.TryGetValue(SessionAddCourseSection1, out section1ModelBytes)).Returns(true);

            var venueModelBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(venueModel));
            _mockSession.Setup(m => m.TryGetValue(SessionVenues, out venueModelBytes)).Returns(true);

            var regionModelBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(regionModel));
            _mockSession.Setup(m => m.TryGetValue(SessionRegions, out regionModelBytes)).Returns(true);

            var sectors = new Faker<Sector>().Generate(3);
            _mockSqlQueryDispatcher.Setup(m => m.ExecuteQuery(It.IsAny<GetSectors>())).ReturnsAsync(sectors);

            // Act
            var result = await controller.AddCourseRun(model) as ViewResult;

            // Assert            
            Assert.NotNull(result);

            var viewModel = result.Model as AddCourseSummaryViewModel;
            Assert.NotNull(viewModel);

            var regions = viewModel.Regions.ToList();
            Assert.Equal(2, regions.Count);
            Assert.Equal("South Tyneside", regions[0]);
            Assert.Equal("North East", regions[1]);
        }
        #endregion

        #region Tests for AcceptAndPublish action method
        [Fact]
        public async Task AcceptAndPublish_WhenCourseIsLarsAndLarsDataIsMissing_RedirectsToAddCourseActionWithErrorMessage()
        {
            // Arrange
            var controller = GetController();

            // Act
            var result = await controller.AcceptAndPublish() as RedirectToActionResult;

            // Assert            
            Assert.NotNull(result);

            Assert.Equal("AddCourse", result.ActionName);
            Assert.Equal("Course data is missing.", result.RouteValues["errmsg"]);
        }

        [Fact]
        public async Task AcceptAndPublish_WhenCourseIsLarsAndLarsDataIsFound_SavesDataAndRedirectsToPublishedAction()
        {
            // Arrange
            var controller = GetController();

            var learnAimRef = "6101137";
            var section1Model = GetAddCourseSection1RequestModel();
            var section2Model = GetAddCourseSection2RequestModel();            
            
            var section1ModelBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(section1Model));
            _mockSession.Setup(m => m.TryGetValue(SessionAddCourseSection1, out section1ModelBytes)).Returns(true);

            var section2ModelBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(section2Model));
            _mockSession.Setup(m => m.TryGetValue(SessionAddCourseSection2, out section2ModelBytes)).Returns(true);

            var learnAimRefBytes = Encoding.UTF8.GetBytes(learnAimRef);
            _mockSession.Setup(m => m.TryGetValue(SessionLearnAimRef, out learnAimRefBytes)).Returns(true);

            var notionalLevelBytes = Encoding.UTF8.GetBytes("level2");
            _mockSession.Setup(m => m.TryGetValue(SessionNotionalNvqLevelV2, out notionalLevelBytes)).Returns(true);

            var awardingBodyBytes = Encoding.UTF8.GetBytes("Awarding Body");
            _mockSession.Setup(m => m.TryGetValue(SessionAwardOrgCode, out awardingBodyBytes)).Returns(true);

            var learnAimRefTitleBytes = Encoding.UTF8.GetBytes("Learn Aim Ref Title");
            _mockSession.Setup(m => m.TryGetValue(SessionLearnAimRefTitle, out learnAimRefTitleBytes)).Returns(true);

            _mockCourseTypeService.Setup(m => m.GetCourseType(learnAimRef, It.IsAny<Guid>())).ReturnsAsync(CourseType.EssentialSkills);

            var providerInfo = new ProviderInfo { ProviderId = Guid.NewGuid(), ProviderName = Faker.Company.Name() };
            var providerContext = new ProviderContext(providerInfo);
            _mockProviderContextProvider.Setup(m => m.GetProviderContext(true)).Returns(providerContext);

            // Act
            var result = await controller.AcceptAndPublish() as RedirectToActionResult;

            // Assert            
            Assert.NotNull(result);

            Assert.Equal("Published", result.ActionName);
        }

        [Fact]
        public async Task AcceptAndPublish_WhenCourseIsNonLars_SavesDataAndRedirectsToPublishedAction()
        {
            // Arrange
            var controller = GetController();
                        
            var section1Model = GetAddCourseSection1RequestModel();
            var section2Model = GetAddCourseSection2RequestModel();
            section2Model.StartDateType = "SpecifiedStartDate";
            section2Model.Day = DateTime.Now.AddDays(30).Day.ToString();
            section2Model.Month = DateTime.Now.AddDays(30).Month.ToString();
            section2Model.Year = DateTime.Now.AddDays(30).Year.ToString();
            
            var section1ModelBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(section1Model));
            _mockSession.Setup(m => m.TryGetValue(SessionAddCourseSection1, out section1ModelBytes)).Returns(true);

            var section2ModelBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(section2Model));
            _mockSession.Setup(m => m.TryGetValue(SessionAddCourseSection2, out section2ModelBytes)).Returns(true);            

            var providerInfo = new ProviderInfo { ProviderId = Guid.NewGuid(), ProviderName = Faker.Company.Name() };
            var providerContext = new ProviderContext(providerInfo);
            _mockProviderContextProvider.Setup(m => m.GetProviderContext(true)).Returns(providerContext);

            var trueValueBytes = Encoding.UTF8.GetBytes("true");
            _mockSession.Setup(m => m.TryGetValue(SessionNonLarsCourse, out trueValueBytes)).Returns(true);

            // Act
            var result = await controller.AcceptAndPublish() as RedirectToActionResult;

            // Assert            
            Assert.NotNull(result);

            Assert.Equal("Published", result.ActionName);
        }

        [Fact]
        public async Task AcceptAndPublish_WhenDeliveryModeIsOnline_SavesDataAndRedirectsToPublishedAction()
        {
            // Arrange
            var controller = GetController();

            var section1Model = GetAddCourseSection1RequestModel();
            var section2Model = GetAddCourseSection2RequestModel();
            section2Model.DeliveryMode = CourseDeliveryMode.Online;

            var section1ModelBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(section1Model));
            _mockSession.Setup(m => m.TryGetValue(SessionAddCourseSection1, out section1ModelBytes)).Returns(true);

            var section2ModelBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(section2Model));
            _mockSession.Setup(m => m.TryGetValue(SessionAddCourseSection2, out section2ModelBytes)).Returns(true);

            var providerInfo = new ProviderInfo { ProviderId = Guid.NewGuid(), ProviderName = Faker.Company.Name() };
            var providerContext = new ProviderContext(providerInfo);
            _mockProviderContextProvider.Setup(m => m.GetProviderContext(true)).Returns(providerContext);

            var trueValueBytes = Encoding.UTF8.GetBytes("true");
            _mockSession.Setup(m => m.TryGetValue(SessionNonLarsCourse, out trueValueBytes)).Returns(true);

            // Act
            var result = await controller.AcceptAndPublish() as RedirectToActionResult;

            // Assert            
            Assert.NotNull(result);

            Assert.Equal("Published", result.ActionName);
        }

        [Fact]
        public async Task AcceptAndPublish_WhenDeliveryModeIsWorkBasedAndNationalIsTrue_SavesDataAndRedirectsToPublishedAction()
        {
            // Arrange
            var controller = GetController();

            var section1Model = GetAddCourseSection1RequestModel();
            var section2Model = GetAddCourseSection2RequestModel();
            section2Model.DeliveryMode = CourseDeliveryMode.WorkBased;
            section2Model.National = true;

            var section1ModelBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(section1Model));
            _mockSession.Setup(m => m.TryGetValue(SessionAddCourseSection1, out section1ModelBytes)).Returns(true);

            var section2ModelBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(section2Model));
            _mockSession.Setup(m => m.TryGetValue(SessionAddCourseSection2, out section2ModelBytes)).Returns(true);

            var providerInfo = new ProviderInfo { ProviderId = Guid.NewGuid(), ProviderName = Faker.Company.Name() };
            var providerContext = new ProviderContext(providerInfo);
            _mockProviderContextProvider.Setup(m => m.GetProviderContext(true)).Returns(providerContext);            

            var trueValueBytes = Encoding.UTF8.GetBytes("true");
            _mockSession.Setup(m => m.TryGetValue(SessionNonLarsCourse, out trueValueBytes)).Returns(true);

            // Act
            var result = await controller.AcceptAndPublish() as RedirectToActionResult;

            // Assert            
            Assert.NotNull(result);

            Assert.Equal("Published", result.ActionName);
        }

        [Fact]
        public async Task AcceptAndPublish_WhenDeliveryModeIsWorkBasedAndNationalIsFalse_SavesDataAndRedirectsToPublishedAction()
        {
            // Arrange
            var controller = GetController();

            var section1Model = GetAddCourseSection1RequestModel();
            var section2Model = GetAddCourseSection2RequestModel();
            section2Model.DeliveryMode = CourseDeliveryMode.WorkBased;
            section2Model.SelectedRegions = new string[] { "E12000001", "E08000023" };

            var section1ModelBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(section1Model));
            _mockSession.Setup(m => m.TryGetValue(SessionAddCourseSection1, out section1ModelBytes)).Returns(true);

            var section2ModelBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(section2Model));
            _mockSession.Setup(m => m.TryGetValue(SessionAddCourseSection2, out section2ModelBytes)).Returns(true);

            var providerInfo = new ProviderInfo { ProviderId = Guid.NewGuid(), ProviderName = Faker.Company.Name() };
            var providerContext = new ProviderContext(providerInfo);
            _mockProviderContextProvider.Setup(m => m.GetProviderContext(true)).Returns(providerContext);            

            var trueValueBytes = Encoding.UTF8.GetBytes("true");
            _mockSession.Setup(m => m.TryGetValue(SessionNonLarsCourse, out trueValueBytes)).Returns(true);

            // Act
            var result = await controller.AcceptAndPublish() as RedirectToActionResult;

            // Assert            
            Assert.NotNull(result);

            Assert.Equal("Published", result.ActionName);
        }

        #endregion

        private AddCourseController GetController()
        {
            var addCourseController = new AddCourseController(
                            _mockCourseService.Object,
                            _mockSqlQueryDispatcher.Object,
                            _mockCurrentUserProvider.Object,
                            _mockProviderContextProvider.Object,
                            _mockCourseTypeService.Object,
                            _mockWebRiskService.Object,
                            _mockConfiguration.Object);
            addCourseController.ControllerContext = new ControllerContext();
            addCourseController.ControllerContext.HttpContext = new DefaultHttpContext { Session = _mockSession.Object };
            return addCourseController;
        }

        private AddCourseRequestModel GetAddCourseSection2RequestModel()
        {
            return new Faker<AddCourseRequestModel>()
                            .RuleFor(c => c.CourseId, f => Guid.NewGuid())
                            .RuleFor(c => c.CourseRunId, f => Guid.NewGuid())
                            .RuleFor(c => c.CourseName, f => f.Lorem.Sentence())
                            .RuleFor(c => c.CourseProviderReference, f => f.Lorem.Word())
                            .RuleFor(c => c.DeliveryMode, f => CourseDeliveryMode.ClassroomBased)
                            .RuleFor(c => c.Cost, f => 20)
                            .RuleFor(c => c.CourseType, f => CourseType.SkillsBootcamp)
                            .RuleFor(c => c.SectorId, f => 2)
                            .RuleFor(c => c.EducationLevel, f => EducationLevel.Two)
                            .RuleFor(c => c.SelectedVenues, f => new Guid[] { Guid.NewGuid() })
                            .RuleFor(c => c.StartDateType, f => "FlexibleStartDate")
                            .RuleFor(c => c.DurationUnit, f => CourseDurationUnit.Hours)
                            .RuleFor(c => c.DurationLength, f => 2)
                            .Generate();
        }

        private AddCourseSection1RequestModel GetAddCourseSection1RequestModel()
        {
            return new Faker<AddCourseSection1RequestModel>()
                            .RuleFor(c => c.CourseId, f => Guid.NewGuid())
                            .RuleFor(c => c.CourseRunId, f => Guid.NewGuid())
                            .RuleFor(c => c.CourseFor, f => f.Lorem.Sentence())
                            .RuleFor(c => c.EntryRequirements, f => f.Lorem.Paragraph())
                            .RuleFor(c => c.HowAssessed, f => f.Lorem.Paragraph())
                            .RuleFor(c => c.HowYouWillLearn, f => f.Lorem.Paragraph())
                            .RuleFor(c => c.WhatWillLearn, f => f.Lorem.Paragraph())
                            .RuleFor(c => c.WhatYouNeed, f => f.Lorem.Paragraph())
                            .RuleFor(c => c.WhereNext, f => f.Lorem.Paragraph())
                            .RuleFor(c => c.LearnAimRef, f => f.Random.AlphaNumeric(8))
                            .RuleFor(c => c.LearnAimRefTitle, f => f.Lorem.Sentence())
                            .RuleFor(c => c.AwardOrgCode, f => f.Random.AlphaNumeric(8))
                            .Generate();
        }

        private SelectVenueModel GetSelectVenueModel()
        {
            return new Faker<SelectVenueModel>()
                            .RuleFor(c => c.VenueItems, f => new Faker<VenueItemModel>()
                                                        .RuleFor(v => v.Id, f => f.Random.Number(1, 20).ToString())
                                                        .RuleFor(v => v.VenueName, f => f.Address.City())                            
                                                        .Generate(4))                            
                            .Generate();
        }        
    }
}
