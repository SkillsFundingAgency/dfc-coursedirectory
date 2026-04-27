using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Middleware;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Models.Regions;
using Dfc.CourseDirectory.WebV2.Controllers.ProviderCourses;
using Dfc.CourseDirectory.WebV2.ViewComponents.RequestModels;
using Dfc.CourseDirectory.WebV2.ViewModels.ProviderCourses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.ControllerTests
{
    public class ProviderCoursesControllerTests
    {
        private readonly Mock<ILogger<ProviderCoursesController>> _mockLogger;
        private readonly Mock<ICourseService> _mockCourseService;
        private readonly Mock<ISqlQueryDispatcher> _mockSqlQueryDispatcher;
        private readonly Mock<IProviderContextProvider> _mockProviderContextProvider;
        private readonly Mock<ISession> _mockSession;
        private readonly Guid _providerId = Guid.NewGuid();

        public ProviderCoursesControllerTests()
        {
            _mockLogger = new Mock<ILogger<ProviderCoursesController>>();
            _mockCourseService = new Mock<ICourseService>();
            _mockSqlQueryDispatcher = new Mock<ISqlQueryDispatcher>();
            _mockProviderContextProvider = new Mock<IProviderContextProvider>();
            _mockSession = new Mock<ISession>();

            _mockCourseService.Setup(m => m.GetRegions()).Returns(new SelectRegionModel());

            _mockSqlQueryDispatcher
                .Setup(m => m.ExecuteQuery(It.IsAny<GetCoursesForProvider>()))
                .ReturnsAsync((IReadOnlyCollection<Course>)Array.Empty<Course>());

            _mockSqlQueryDispatcher
                .Setup(m => m.ExecuteQuery(It.IsAny<GetNonLarsCoursesForProvider>()))
                .ReturnsAsync((IReadOnlyCollection<Course>)Array.Empty<Course>());

            _mockSqlQueryDispatcher
                .Setup(m => m.ExecuteQuery(It.IsAny<GetVenuesByProvider>()))
                .ReturnsAsync((IReadOnlyCollection<Venue>)new List<Venue>());

            var providerInfo = new ProviderInfo { ProviderId = _providerId, ProviderName = "Test Provider" };
            _mockProviderContextProvider
                .Setup(m => m.GetProviderContext(It.IsAny<bool>()))
                .Returns(new ProviderContext(providerInfo));
        }

        [Fact]
        public async Task Index_NoUkprnInSession_RedirectsToHome()
        {
            var controller = GetController();

            var result = await controller.Index();

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Home", redirect.ControllerName);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Please select a Provider.", redirect.RouteValues["errmsg"]);
        }

        [Fact]
        public async Task Index_WithLarsCourses_ReturnsViewResult()
        {
            SetupUkprn(10012345);
            SetupNoCourseCache();
            SetupNoSearchState();
            var controller = GetController();

            var result = await controller.Index();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Index_KeywordInSearchState_FiltersCoursesByCourseName()
        {
            SetupUkprn(10012345);
            SetupNoCourseCache();
            SetupSearchState(new ProviderCourseSearchState { Keyword = "mathematics" });
            _mockSqlQueryDispatcher
                .Setup(m => m.ExecuteQuery(It.IsAny<GetCoursesForProvider>()))
                .ReturnsAsync((IReadOnlyCollection<Course>)new[] { MakeCourse("Mathematics Advanced"), MakeCourse("Art History Basic") });
            var controller = GetController();

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProviderCoursesViewModel>(viewResult.Model);
            Assert.Contains(model.ProviderCourseRuns, r => r.CourseName == "Mathematics Advanced");
            Assert.DoesNotContain(model.ProviderCourseRuns, r => r.CourseName == "Art History Basic");
        }

        [Fact]
        public async Task Index_KeywordTooShort_AddsValidationError()
        {
            SetupUkprn(10012345);
            SetupNoCourseCache();
            SetupSearchState(new ProviderCourseSearchState { Keyword = "ab" });
            _mockSqlQueryDispatcher
                .Setup(m => m.ExecuteQuery(It.IsAny<GetCoursesForProvider>()))
                .ReturnsAsync((IReadOnlyCollection<Course>)new[] { MakeCourse("Some Course") });
            var controller = GetController();

            var result = await controller.Index();

            Assert.IsType<ViewResult>(result);
            var error = controller.ModelState["Keyword"]?.Errors.FirstOrDefault();
            Assert.NotNull(error);
            Assert.Equal("Enter a minimum of 3 characters", error.ErrorMessage);
        }

        [Fact]
        public async Task Index_ActiveKeywordFilter_HasFiltersIsTrue()
        {
            SetupUkprn(10012345);
            SetupNoCourseCache();
            SetupSearchState(new ProviderCourseSearchState { Keyword = "maths" });
            _mockSqlQueryDispatcher
                .Setup(m => m.ExecuteQuery(It.IsAny<GetCoursesForProvider>()))
                .ReturnsAsync((IReadOnlyCollection<Course>)new[] { MakeCourse("Maths Level 3") });
            var controller = GetController();

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProviderCoursesViewModel>(viewResult.Model);
            Assert.True(model.HasFilters);
        }

        [Fact]
        public async Task Index_SecondPage_ReturnsPaginatedResults()
        {
            SetupUkprn(10012345);
            SetupNoCourseCache();
            SetupNoSearchState();
            var courses = Enumerable.Range(1, 12).Select(i => MakeCourse($"Course {i:D2}")).ToArray();
            _mockSqlQueryDispatcher
                .Setup(m => m.ExecuteQuery(It.IsAny<GetCoursesForProvider>()))
                .ReturnsAsync((IReadOnlyCollection<Course>)courses);
            var controller = GetController();

            var result = await controller.Index(page: 2);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProviderCoursesViewModel>(viewResult.Model);
            Assert.Equal(2, model.ProviderCourseRuns.Count);
        }

        [Fact]
        public void Search_NoUkprnInSession_RedirectsToHome()
        {
            var controller = GetController();

            var result = controller.Search(new ProviderCourseSearchState { NonLarsCourse = false });

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Home", redirect.ControllerName);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Please select a Provider.", redirect.RouteValues["errmsg"]);
        }

        [Fact]
        public void Search_ValidKeyword_StoresSearchStateInSession()
        {
            SetupUkprn(10012345);
            var controller = GetController();
            var searchState = new ProviderCourseSearchState { Keyword = "test keyword", NonLarsCourse = false };

            controller.Search(searchState);

            _mockSession.Verify(
                m => m.Set("ProviderCoursesSearchState", It.IsAny<byte[]>()),
                Times.Once);
        }

        [Fact]
        public void Search_ValidKeyword_RedirectsToIndex()
        {
            SetupUkprn(10012345);
            var controller = GetController();

            var result = controller.Search(new ProviderCourseSearchState { NonLarsCourse = false });

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public void Search_NonLarsCourseFlag_RedirectIncludesNlcParam()
        {
            SetupUkprn(10012345);
            var controller = GetController();

            var result = controller.Search(new ProviderCourseSearchState { NonLarsCourse = true });

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("true", redirect.RouteValues?["nlc"]?.ToString());
        }

        [Fact]
        public void Search_LarsCourseFlag_RedirectHasNoNlcRouteValues()
        {
            SetupUkprn(10012345);
            var controller = GetController();

            var result = controller.Search(new ProviderCourseSearchState { NonLarsCourse = false });

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Null(redirect.RouteValues);
        }

        [Fact]
        public void ClearFilters_RemovesSearchStateFromSession()
        {
            var controller = GetController();

            controller.ClearFilters();

            _mockSession.Verify(m => m.Remove("ProviderCoursesSearchState"), Times.Once);
        }

        [Fact]
        public void ClearFilters_RedirectsToIndex()
        {
            var controller = GetController();

            var result = controller.ClearFilters();

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public void ClearFilters_WithNlcTrue_RedirectIncludesNlcParam()
        {
            var controller = GetController();

            var result = controller.ClearFilters(nlc: true);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("true", redirect.RouteValues?["nlc"]?.ToString());
        }

        [Fact]
        public async Task Index_CourseRunWithMatchingVenue_VenuePropertyEqualsVenueName()
        {
            var venueId = Guid.NewGuid();
            var venue = new Venue { VenueId = venueId, VenueName = "Test Venue" };

            SetupUkprn(10012345);
            SetupNoCourseCache();
            SetupNoSearchState();
            _mockSqlQueryDispatcher
                .Setup(m => m.ExecuteQuery(It.IsAny<GetVenuesByProvider>()))
                .ReturnsAsync((IReadOnlyCollection<Venue>)new[] { venue });
            _mockSqlQueryDispatcher
                .Setup(m => m.ExecuteQuery(It.IsAny<GetCoursesForProvider>()))
                .ReturnsAsync((IReadOnlyCollection<Course>)new[] { MakeCourse("Test Course", venueId) });
            var controller = GetController();

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProviderCoursesViewModel>(viewResult.Model);
            Assert.Equal("Test Venue", model.ProviderCourseRuns.Single().Venue);
        }

        [Fact]
        public async Task Index_CourseRunWithUnmatchedVenueId_VenuePropertyIsEmpty()
        {
            var unmatchedVenueId = Guid.NewGuid();

            SetupUkprn(10012345);
            SetupNoCourseCache();
            SetupNoSearchState();
            _mockSqlQueryDispatcher
                .Setup(m => m.ExecuteQuery(It.IsAny<GetCoursesForProvider>()))
                .ReturnsAsync((IReadOnlyCollection<Course>)new[] { MakeCourse("Test Course", unmatchedVenueId) });
            var controller = GetController();

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProviderCoursesViewModel>(viewResult.Model);
            Assert.Equal(string.Empty, model.ProviderCourseRuns.Single().Venue);
        }

        [Fact]
        public async Task Index_SearchStateNlcMismatch_ClearsCourseCache()
        {
            SetupUkprn(10012345);
            SetupNoCourseCache();
            SetupSearchState(new ProviderCourseSearchState { NonLarsCourse = false });
            var controller = GetController();

            var result = await controller.Index(nlc: true);

            _mockSession.Verify(m => m.Remove("ProviderCourses"), Times.AtLeastOnce);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Index_CourseRunWithFixedStartDate_DisplaysFormattedDate()
        {
            SetupUkprn(10012345);
            SetupNoCourseCache();
            SetupNoSearchState();
            _mockSqlQueryDispatcher
                .Setup(m => m.ExecuteQuery(It.IsAny<GetCoursesForProvider>()))
                .ReturnsAsync((IReadOnlyCollection<Course>)new[]
                {
                    MakeCourse("Test Course", configureRun: r =>
                    {
                        r.FlexibleStartDate = false;
                        r.StartDate = new DateTime(2025, 6, 1);
                    })
                });
            var controller = GetController();

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProviderCoursesViewModel>(viewResult.Model);
            Assert.Equal("01 Jun 2025", model.ProviderCourseRuns.Single().StartDate);
        }

        [Fact]
        public async Task Index_CourseRunWithCost_DisplaysFormattedCost()
        {
            SetupUkprn(10012345);
            SetupNoCourseCache();
            SetupNoSearchState();
            _mockSqlQueryDispatcher
                .Setup(m => m.ExecuteQuery(It.IsAny<GetCoursesForProvider>()))
                .ReturnsAsync((IReadOnlyCollection<Course>)new[]
                {
                    MakeCourse("Test Course", configureRun: r => r.Cost = 150m)
                });
            var controller = GetController();

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProviderCoursesViewModel>(viewResult.Model);
            Assert.Equal("£ 150.00", model.ProviderCourseRuns.Single().Cost);
        }

        [Fact]
        public async Task Index_CourseRunWithNullCost_CostIsEmptyString()
        {
            SetupUkprn(10012345);
            SetupNoCourseCache();
            SetupNoSearchState();
            _mockSqlQueryDispatcher
                .Setup(m => m.ExecuteQuery(It.IsAny<GetCoursesForProvider>()))
                .ReturnsAsync((IReadOnlyCollection<Course>)new[]
                {
                    MakeCourse("Test Course", configureRun: r => r.Cost = null)
                });
            var controller = GetController();

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProviderCoursesViewModel>(viewResult.Model);
            Assert.Equal(string.Empty, model.ProviderCourseRuns.Single().Cost);
        }

        [Fact]
        public async Task Index_CourseRunWithStudyMode_DisplaysStudyModeDescription()
        {
            SetupUkprn(10012345);
            SetupNoCourseCache();
            SetupNoSearchState();
            _mockSqlQueryDispatcher
                .Setup(m => m.ExecuteQuery(It.IsAny<GetCoursesForProvider>()))
                .ReturnsAsync((IReadOnlyCollection<Course>)new[]
                {
                    MakeCourse("Test Course", configureRun: r => r.StudyMode = CourseStudyMode.FullTime)
                });
            var controller = GetController();

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProviderCoursesViewModel>(viewResult.Model);
            Assert.Equal("Full-time", model.ProviderCourseRuns.Single().StudyMode);
        }

        [Fact]
        public async Task Index_CourseRunWithNullStudyMode_StudyModeIsEmpty()
        {
            SetupUkprn(10012345);
            SetupNoCourseCache();
            SetupNoSearchState();
            _mockSqlQueryDispatcher
                .Setup(m => m.ExecuteQuery(It.IsAny<GetCoursesForProvider>()))
                .ReturnsAsync((IReadOnlyCollection<Course>)new[]
                {
                    MakeCourse("Test Course", configureRun: r => r.StudyMode = null)
                });
            var controller = GetController();

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProviderCoursesViewModel>(viewResult.Model);
            Assert.Equal(string.Empty, model.ProviderCourseRuns.Single().StudyMode);
        }

        [Fact]
        public async Task Index_CourseRunWithDurationValue_DisplaysDuration()
        {
            SetupUkprn(10012345);
            SetupNoCourseCache();
            SetupNoSearchState();
            _mockSqlQueryDispatcher
                .Setup(m => m.ExecuteQuery(It.IsAny<GetCoursesForProvider>()))
                .ReturnsAsync((IReadOnlyCollection<Course>)new[]
                {
                    MakeCourse("Test Course", configureRun: r =>
                    {
                        r.DurationValue = 6;
                        r.DurationUnit = CourseDurationUnit.Months;
                    })
                });
            var controller = GetController();

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProviderCoursesViewModel>(viewResult.Model);
            Assert.Equal("6 Months", model.ProviderCourseRuns.Single().Duration);
        }

        [Fact]
        public async Task Index_CourseRunWithNullDurationValue_DisplaysZeroDuration()
        {
            SetupUkprn(10012345);
            SetupNoCourseCache();
            SetupNoSearchState();
            _mockSqlQueryDispatcher
                .Setup(m => m.ExecuteQuery(It.IsAny<GetCoursesForProvider>()))
                .ReturnsAsync((IReadOnlyCollection<Course>)new[]
                {
                    MakeCourse("Test Course", configureRun: r =>
                    {
                        r.DurationValue = null;
                        r.DurationUnit = CourseDurationUnit.Hours;
                    })
                });
            var controller = GetController();

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProviderCoursesViewModel>(viewResult.Model);
            Assert.Equal("0 Hours", model.ProviderCourseRuns.Single().Duration);
        }

        [Fact]
        public async Task Index_NationalCourseRun_RegionContainsAllRegionNames()
        {
            SetupUkprn(10012345);
            SetupNoCourseCache();
            SetupNoSearchState();
            _mockSqlQueryDispatcher
                .Setup(m => m.ExecuteQuery(It.IsAny<GetCoursesForProvider>()))
                .ReturnsAsync((IReadOnlyCollection<Course>)new[]
                {
                    MakeCourse("National Course", configureRun: r =>
                    {
                        r.DeliveryMode = CourseDeliveryMode.WorkBased;
                        r.National = null;
                    })
                });
            var controller = GetController();

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProviderCoursesViewModel>(viewResult.Model);
            var run = model.ProviderCourseRuns.Single();
            Assert.Contains("North East", run.Region);
            Assert.Contains("North West", run.Region);
        }

        [Fact]
        public async Task Index_NonNationalCourseRunWithSubRegions_RegionContainsFormattedRegionName()
        {
            SetupUkprn(10012345);
            SetupNoCourseCache();
            SetupNoSearchState();
            _mockSqlQueryDispatcher
                .Setup(m => m.ExecuteQuery(It.IsAny<GetCoursesForProvider>()))
                .ReturnsAsync((IReadOnlyCollection<Course>)new[]
                {
                    MakeCourse("Regional Course", configureRun: r =>
                    {
                        r.SubRegionIds = new[] { "E06000001" };
                        r.National = false;
                    })
                });
            var controller = GetController();

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProviderCoursesViewModel>(viewResult.Model);
            Assert.Equal("North East", model.ProviderCourseRuns.Single().Region);
        }

        [Fact]
        public async Task Index_KeywordMatchesLearnAimRef_IncludesCourse()
        {
            SetupUkprn(10012345);
            SetupNoCourseCache();
            SetupSearchState(new ProviderCourseSearchState { Keyword = "abc123" });
            _mockSqlQueryDispatcher
                .Setup(m => m.ExecuteQuery(It.IsAny<GetCoursesForProvider>()))
                .ReturnsAsync((IReadOnlyCollection<Course>)new[]
                {
                    MakeCourse("Target Course", learnAimRef: "ABC123"),
                    MakeCourse("Other Course", learnAimRef: "XYZ999")
                });
            var controller = GetController();

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProviderCoursesViewModel>(viewResult.Model);
            Assert.Single(model.ProviderCourseRuns);
            Assert.Equal("Target Course", model.ProviderCourseRuns.Single().CourseName);
        }

        [Fact]
        public async Task Index_KeywordMatchesLearnAimRefTypeDesc_IncludesCourse()
        {
            SetupUkprn(10012345);
            SetupNoCourseCache();
            SetupSearchState(new ProviderCourseSearchState { Keyword = "advanced" });
            _mockSqlQueryDispatcher
                .Setup(m => m.ExecuteQuery(It.IsAny<GetCoursesForProvider>()))
                .ReturnsAsync((IReadOnlyCollection<Course>)new[]
                {
                    MakeCourse("Target Course", learnAimRefTypeDesc: "Advanced Maths"),
                    MakeCourse("Other Course", learnAimRefTypeDesc: "Basic Science")
                });
            var controller = GetController();

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProviderCoursesViewModel>(viewResult.Model);
            Assert.Single(model.ProviderCourseRuns);
            Assert.Equal("Target Course", model.ProviderCourseRuns.Single().CourseName);
        }

        [Fact]
        public async Task Index_KeywordMatchesCourseTextId_IncludesCourse()
        {
            SetupUkprn(10012345);
            SetupNoCourseCache();
            SetupSearchState(new ProviderCourseSearchState { Keyword = "prov99" });
            _mockSqlQueryDispatcher
                .Setup(m => m.ExecuteQuery(It.IsAny<GetCoursesForProvider>()))
                .ReturnsAsync((IReadOnlyCollection<Course>)new[]
                {
                    MakeCourse("Target Course", providerCourseId: "PROV99"),
                    MakeCourse("Other Course", providerCourseId: "OTHER01")
                });
            var controller = GetController();

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProviderCoursesViewModel>(viewResult.Model);
            Assert.Single(model.ProviderCourseRuns);
            Assert.Equal("Target Course", model.ProviderCourseRuns.Single().CourseName);
        }

        [Fact]
        public async Task Index_LevelFilter_ExcludesNonMatchingLevels()
        {
            SetupUkprn(10012345);
            SetupNoCourseCache();
            SetupSearchState(new ProviderCourseSearchState { LevelFilter = new[] { "3" } });
            _mockSqlQueryDispatcher
                .Setup(m => m.ExecuteQuery(It.IsAny<GetCoursesForProvider>()))
                .ReturnsAsync((IReadOnlyCollection<Course>)new[]
                {
                    MakeCourse("Level 3 Course", notionalNVQLevelv2: "3"),
                    MakeCourse("Level 4 Course", notionalNVQLevelv2: "4")
                });
            var controller = GetController();

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProviderCoursesViewModel>(viewResult.Model);
            Assert.Single(model.ProviderCourseRuns);
            Assert.Equal("Level 3 Course", model.ProviderCourseRuns.Single().CourseName);
        }

        [Fact]
        public async Task Index_DeliveryModeFilter_ExcludesNonMatchingModes()
        {
            SetupUkprn(10012345);
            SetupNoCourseCache();
            SetupSearchState(new ProviderCourseSearchState { DeliveryModeFilter = new[] { "Online" } });
            _mockSqlQueryDispatcher
                .Setup(m => m.ExecuteQuery(It.IsAny<GetCoursesForProvider>()))
                .ReturnsAsync((IReadOnlyCollection<Course>)new[]
                {
                    MakeCourse("Online Course"),
                    MakeCourse("WorkBased Course", configureRun: r => r.DeliveryMode = CourseDeliveryMode.WorkBased)
                });
            var controller = GetController();

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProviderCoursesViewModel>(viewResult.Model);
            Assert.Single(model.ProviderCourseRuns);
            Assert.Equal("Online Course", model.ProviderCourseRuns.Single().CourseName);
        }

        [Fact]
        public async Task Index_VenueFilter_ExcludesNonMatchingVenues()
        {
            var venueAId = Guid.NewGuid();
            var venueBId = Guid.NewGuid();

            SetupUkprn(10012345);
            SetupNoCourseCache();
            SetupSearchState(new ProviderCourseSearchState { VenueFilter = new[] { "Venue A" } });
            _mockSqlQueryDispatcher
                .Setup(m => m.ExecuteQuery(It.IsAny<GetVenuesByProvider>()))
                .ReturnsAsync((IReadOnlyCollection<Venue>)new[]
                {
                    new Venue { VenueId = venueAId, VenueName = "Venue A" },
                    new Venue { VenueId = venueBId, VenueName = "Venue B" }
                });
            _mockSqlQueryDispatcher
                .Setup(m => m.ExecuteQuery(It.IsAny<GetCoursesForProvider>()))
                .ReturnsAsync((IReadOnlyCollection<Course>)new[]
                {
                    MakeCourse("Course A", venueAId),
                    MakeCourse("Course B", venueBId)
                });
            var controller = GetController();

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProviderCoursesViewModel>(viewResult.Model);
            Assert.Single(model.ProviderCourseRuns);
            Assert.Equal("Course A", model.ProviderCourseRuns.Single().CourseName);
        }

        [Fact]
        public async Task Index_AttendancePatternFilter_ExcludesNonMatchingPatterns()
        {
            SetupUkprn(10012345);
            SetupNoCourseCache();
            SetupSearchState(new ProviderCourseSearchState { AttendancePatternFilter = new[] { "Daytime" } });
            _mockSqlQueryDispatcher
                .Setup(m => m.ExecuteQuery(It.IsAny<GetCoursesForProvider>()))
                .ReturnsAsync((IReadOnlyCollection<Course>)new[]
                {
                    MakeCourse("Daytime Course", configureRun: r => r.AttendancePattern = CourseAttendancePattern.Daytime),
                    MakeCourse("Evening Course", configureRun: r => r.AttendancePattern = CourseAttendancePattern.Evening)
                });
            var controller = GetController();

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProviderCoursesViewModel>(viewResult.Model);
            Assert.Single(model.ProviderCourseRuns);
            Assert.Equal("Daytime Course", model.ProviderCourseRuns.Single().CourseName);
        }

        [Fact]
        public async Task Index_RegionFilter_ExcludesNonMatchingRegions()
        {
            SetupUkprn(10012345);
            SetupNoCourseCache();
            SetupSearchState(new ProviderCourseSearchState { RegionFilter = new[] { "E12000001" } });
            _mockSqlQueryDispatcher
                .Setup(m => m.ExecuteQuery(It.IsAny<GetCoursesForProvider>()))
                .ReturnsAsync((IReadOnlyCollection<Course>)new[]
                {
                    MakeCourse("North East Course", configureRun: r =>
                    {
                        r.SubRegionIds = new[] { "E06000001" };
                        r.National = false;
                    }),
                    MakeCourse("No Region Course")
                });
            var controller = GetController();

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProviderCoursesViewModel>(viewResult.Model);
            Assert.Single(model.ProviderCourseRuns);
            Assert.Equal("North East Course", model.ProviderCourseRuns.Single().CourseName);
        }

        [Fact]
        public async Task Index_CourseWithLevelE_LevelsContainsEntryLevel()
        {
            SetupUkprn(10012345);
            SetupNoCourseCache();
            SetupNoSearchState();
            _mockSqlQueryDispatcher
                .Setup(m => m.ExecuteQuery(It.IsAny<GetCoursesForProvider>()))
                .ReturnsAsync((IReadOnlyCollection<Course>)new[] { MakeCourse("Test Course", notionalNVQLevelv2: "E") });
            var controller = GetController();

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProviderCoursesViewModel>(viewResult.Model);
            Assert.Contains(model.Levels, l => l.Text == "Entry level");
        }

        [Fact]
        public async Task Index_CourseWithLevelX_LevelsContainsNotApplicable()
        {
            SetupUkprn(10012345);
            SetupNoCourseCache();
            SetupNoSearchState();
            _mockSqlQueryDispatcher
                .Setup(m => m.ExecuteQuery(It.IsAny<GetCoursesForProvider>()))
                .ReturnsAsync((IReadOnlyCollection<Course>)new[] { MakeCourse("Test Course", notionalNVQLevelv2: "X") });
            var controller = GetController();

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProviderCoursesViewModel>(viewResult.Model);
            Assert.Contains(model.Levels, l => l.Text == "X - Not applicable/unknown");
        }

        [Fact]
        public async Task Index_CourseWithLevelH_LevelsContainsHigher()
        {
            SetupUkprn(10012345);
            SetupNoCourseCache();
            SetupNoSearchState();
            _mockSqlQueryDispatcher
                .Setup(m => m.ExecuteQuery(It.IsAny<GetCoursesForProvider>()))
                .ReturnsAsync((IReadOnlyCollection<Course>)new[] { MakeCourse("Test Course", notionalNVQLevelv2: "H") });
            var controller = GetController();

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProviderCoursesViewModel>(viewResult.Model);
            Assert.Contains(model.Levels, l => l.Text == "Higher");
        }

        [Fact]
        public async Task Index_CourseWithLevelM_LevelsContainsMixed()
        {
            SetupUkprn(10012345);
            SetupNoCourseCache();
            SetupNoSearchState();
            _mockSqlQueryDispatcher
                .Setup(m => m.ExecuteQuery(It.IsAny<GetCoursesForProvider>()))
                .ReturnsAsync((IReadOnlyCollection<Course>)new[] { MakeCourse("Test Course", notionalNVQLevelv2: "M") });
            var controller = GetController();

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProviderCoursesViewModel>(viewResult.Model);
            Assert.Contains(model.Levels, l => l.Text == "Mixed");
        }

        [Fact]
        public async Task Index_CourseWithNumericLevel_LevelsContainsLevelPrefix()
        {
            SetupUkprn(10012345);
            SetupNoCourseCache();
            SetupNoSearchState();
            _mockSqlQueryDispatcher
                .Setup(m => m.ExecuteQuery(It.IsAny<GetCoursesForProvider>()))
                .ReturnsAsync((IReadOnlyCollection<Course>)new[] { MakeCourse("Test Course", notionalNVQLevelv2: "3") });
            var controller = GetController();

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProviderCoursesViewModel>(viewResult.Model);
            Assert.Contains(model.Levels, l => l.Text == "Level 3");
        }


        [Fact]
        public async Task Index_CoursesWithEntryAndOtherLevels_EntryLevelIsFirstInLevels()
        {
            SetupUkprn(10012345);
            SetupNoCourseCache();
            SetupNoSearchState();
            _mockSqlQueryDispatcher
                .Setup(m => m.ExecuteQuery(It.IsAny<GetCoursesForProvider>()))
                .ReturnsAsync((IReadOnlyCollection<Course>)new[]
                {
                    MakeCourse("Level 3 Course", notionalNVQLevelv2: "3"),
                    MakeCourse("Entry Level Course", notionalNVQLevelv2: "E")
                });
            var controller = GetController();

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProviderCoursesViewModel>(viewResult.Model);
            Assert.Equal("Entry level", model.Levels.First().Text);
        }

        [Fact]
        public async Task Index_ActiveLevelFilter_LevelFilterItemIsSelected()
        {
            SetupUkprn(10012345);
            SetupNoCourseCache();
            SetupSearchState(new ProviderCourseSearchState { LevelFilter = new[] { "3" } });
            _mockSqlQueryDispatcher
                .Setup(m => m.ExecuteQuery(It.IsAny<GetCoursesForProvider>()))
                .ReturnsAsync((IReadOnlyCollection<Course>)new[] { MakeCourse("Level 3 Course", notionalNVQLevelv2: "3") });
            var controller = GetController();

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProviderCoursesViewModel>(viewResult.Model);
            Assert.True(model.Levels.Single(l => l.Value == "3").IsSelected);
        }

        [Fact]
        public async Task Index_CoursesWithMultipleDeliveryModes_PopulatesDeliveryModes()
        {
            SetupUkprn(10012345);
            SetupNoCourseCache();
            SetupNoSearchState();
            _mockSqlQueryDispatcher
                .Setup(m => m.ExecuteQuery(It.IsAny<GetCoursesForProvider>()))
                .ReturnsAsync((IReadOnlyCollection<Course>)new[]
                {
                    MakeCourse("Online Course"),
                    MakeCourse("WorkBased Course", configureRun: r => r.DeliveryMode = CourseDeliveryMode.WorkBased)
                });
            var controller = GetController();

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProviderCoursesViewModel>(viewResult.Model);
            Assert.Equal(2, model.DeliveryModes.Count);
        }

        [Fact]
        public async Task Index_ActiveDeliveryModeFilter_DeliveryModeFilterItemIsSelected()
        {
            SetupUkprn(10012345);
            SetupNoCourseCache();
            SetupSearchState(new ProviderCourseSearchState { DeliveryModeFilter = new[] { "Online" } });
            _mockSqlQueryDispatcher
                .Setup(m => m.ExecuteQuery(It.IsAny<GetCoursesForProvider>()))
                .ReturnsAsync((IReadOnlyCollection<Course>)new[] { MakeCourse("Online Course") });
            var controller = GetController();

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProviderCoursesViewModel>(viewResult.Model);
            Assert.True(model.DeliveryModes.Single(d => d.Value == "Online").IsSelected);
        }

        [Fact]
        public async Task Index_CoursesWithVenue_PopulatesVenueFilterItems()
        {
            var venueId = Guid.NewGuid();

            SetupUkprn(10012345);
            SetupNoCourseCache();
            SetupNoSearchState();
            _mockSqlQueryDispatcher
                .Setup(m => m.ExecuteQuery(It.IsAny<GetVenuesByProvider>()))
                .ReturnsAsync((IReadOnlyCollection<Venue>)new[] { new Venue { VenueId = venueId, VenueName = "Venue A" } });
            _mockSqlQueryDispatcher
                .Setup(m => m.ExecuteQuery(It.IsAny<GetCoursesForProvider>()))
                .ReturnsAsync((IReadOnlyCollection<Course>)new[] { MakeCourse("Test Course", venueId) });
            var controller = GetController();

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProviderCoursesViewModel>(viewResult.Model);
            Assert.Contains(model.Venues, v => v.Value == "Venue A");
        }

        [Fact]
        public async Task Index_ActiveVenueFilter_VenueFilterItemIsSelected()
        {
            var venueId = Guid.NewGuid();

            SetupUkprn(10012345);
            SetupNoCourseCache();
            SetupSearchState(new ProviderCourseSearchState { VenueFilter = new[] { "Venue A" } });
            _mockSqlQueryDispatcher
                .Setup(m => m.ExecuteQuery(It.IsAny<GetVenuesByProvider>()))
                .ReturnsAsync((IReadOnlyCollection<Venue>)new[] { new Venue { VenueId = venueId, VenueName = "Venue A" } });
            _mockSqlQueryDispatcher
                .Setup(m => m.ExecuteQuery(It.IsAny<GetCoursesForProvider>()))
                .ReturnsAsync((IReadOnlyCollection<Course>)new[] { MakeCourse("Test Course", venueId) });
            var controller = GetController();

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProviderCoursesViewModel>(viewResult.Model);
            Assert.True(model.Venues.Single(v => v.Value == "Venue A").IsSelected);
        }

        [Fact]
        public async Task Index_CoursesWithAttendancePattern_PopulatesAttendancePatternItems()
        {
            SetupUkprn(10012345);
            SetupNoCourseCache();
            SetupNoSearchState();
            _mockSqlQueryDispatcher
                .Setup(m => m.ExecuteQuery(It.IsAny<GetCoursesForProvider>()))
                .ReturnsAsync((IReadOnlyCollection<Course>)new[]
                {
                    MakeCourse("Test Course", configureRun: r => r.AttendancePattern = CourseAttendancePattern.Daytime)
                });
            var controller = GetController();

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProviderCoursesViewModel>(viewResult.Model);
            Assert.Contains(model.AttendancePattern, x => x.Value == "Daytime");
        }

        [Fact]
        public async Task Index_ActiveAttendancePatternFilter_AttendancePatternFilterItemIsSelected()
        {
            SetupUkprn(10012345);
            SetupNoCourseCache();
            SetupSearchState(new ProviderCourseSearchState { AttendancePatternFilter = new[] { "Daytime" } });
            _mockSqlQueryDispatcher
                .Setup(m => m.ExecuteQuery(It.IsAny<GetCoursesForProvider>()))
                .ReturnsAsync((IReadOnlyCollection<Course>)new[]
                {
                    MakeCourse("Test Course", configureRun: r => r.AttendancePattern = CourseAttendancePattern.Daytime)
                });
            var controller = GetController();

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProviderCoursesViewModel>(viewResult.Model);
            Assert.True(model.AttendancePattern.Single(x => x.Value == "Daytime").IsSelected);
        }

        [Fact]
        public async Task Index_CoursesWithSubRegions_PopulatesRegionFilterItems()
        {
            SetupUkprn(10012345);
            SetupNoCourseCache();
            SetupNoSearchState();
            _mockSqlQueryDispatcher
                .Setup(m => m.ExecuteQuery(It.IsAny<GetCoursesForProvider>()))
                .ReturnsAsync((IReadOnlyCollection<Course>)new[]
                {
                    MakeCourse("North East Course", configureRun: r =>
                    {
                        r.SubRegionIds = new[] { "E06000001" };
                        r.National = false;
                    })
                });
            var controller = GetController();

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProviderCoursesViewModel>(viewResult.Model);
            Assert.Contains(model.Regions, r => r.Text == "North East");
        }

        [Fact]
        public async Task Index_ActiveRegionFilter_RegionFilterItemIsSelected()
        {
            SetupUkprn(10012345);
            SetupNoCourseCache();
            SetupSearchState(new ProviderCourseSearchState { RegionFilter = new[] { "E12000001" } });
            _mockSqlQueryDispatcher
                .Setup(m => m.ExecuteQuery(It.IsAny<GetCoursesForProvider>()))
                .ReturnsAsync((IReadOnlyCollection<Course>)new[]
                {
                    MakeCourse("North East Course", configureRun: r =>
                    {
                        r.SubRegionIds = new[] { "E06000001" };
                        r.National = false;
                    })
                });
            var controller = GetController();

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProviderCoursesViewModel>(viewResult.Model);
            Assert.True(model.Regions.Single(r => r.Text == "North East").IsSelected);
        }

        [Fact]
        public async Task Index_CourseRunIdMatchesExistingRun_NotificationMessageIsAnchorTag()
        {
            var courseRunId = Guid.NewGuid();

            SetupUkprn(10012345);
            SetupNoCourseCache();
            SetupNoSearchState();
            _mockSqlQueryDispatcher
                .Setup(m => m.ExecuteQuery(It.IsAny<GetCoursesForProvider>()))
                .ReturnsAsync((IReadOnlyCollection<Course>)new[]
                {
                    MakeCourse("Test Course", configureRun: r => r.CourseRunId = courseRunId)
                });
            var controller = GetController();

            var result = await controller.Index(
                courseRunId: courseRunId,
                notificationTitle: "Saved",
                notificationMessage: "View");

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProviderCoursesViewModel>(viewResult.Model);
            Assert.Contains("<a", model.NotificationMessage);
            Assert.Contains(courseRunId.ToString(), model.NotificationMessage);
            Assert.Equal("Saved", model.NotificationTitle);
        }

        [Fact]
        public async Task Index_CourseRunIdNotInResults_NotificationMessageIsPlain()
        {
            SetupUkprn(10012345);
            SetupNoCourseCache();
            SetupNoSearchState();
            var controller = GetController();

            var result = await controller.Index(
                courseRunId: Guid.NewGuid(),
                notificationTitle: "Title",
                notificationMessage: "Msg");

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProviderCoursesViewModel>(viewResult.Model);
            Assert.Equal("Title", model.NotificationTitle);
            Assert.Equal("Msg", model.NotificationMessage);
        }

        [Fact]
        public async Task Index_ActiveLevelFilter_HasFiltersIsTrue()
        {
            SetupUkprn(10012345);
            SetupNoCourseCache();
            SetupSearchState(new ProviderCourseSearchState { LevelFilter = new[] { "3" } });
            _mockSqlQueryDispatcher
                .Setup(m => m.ExecuteQuery(It.IsAny<GetCoursesForProvider>()))
                .ReturnsAsync((IReadOnlyCollection<Course>)new[] { MakeCourse("Level 3 Course", notionalNVQLevelv2: "3") });
            var controller = GetController();

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProviderCoursesViewModel>(viewResult.Model);
            Assert.True(model.HasFilters);
        }

        [Fact]
        public async Task Index_ActiveDeliveryModeFilter_HasFiltersIsTrue()
        {
            SetupUkprn(10012345);
            SetupNoCourseCache();
            SetupSearchState(new ProviderCourseSearchState { DeliveryModeFilter = new[] { "Online" } });
            _mockSqlQueryDispatcher
                .Setup(m => m.ExecuteQuery(It.IsAny<GetCoursesForProvider>()))
                .ReturnsAsync((IReadOnlyCollection<Course>)new[] { MakeCourse("Online Course") });
            var controller = GetController();

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProviderCoursesViewModel>(viewResult.Model);
            Assert.True(model.HasFilters);
        }

        [Fact]
        public async Task Index_ActiveVenueFilter_HasFiltersIsTrue()
        {
            var venueId = Guid.NewGuid();

            SetupUkprn(10012345);
            SetupNoCourseCache();
            SetupSearchState(new ProviderCourseSearchState { VenueFilter = new[] { "Venue A" } });
            _mockSqlQueryDispatcher
                .Setup(m => m.ExecuteQuery(It.IsAny<GetVenuesByProvider>()))
                .ReturnsAsync((IReadOnlyCollection<Venue>)new[] { new Venue { VenueId = venueId, VenueName = "Venue A" } });
            _mockSqlQueryDispatcher
                .Setup(m => m.ExecuteQuery(It.IsAny<GetCoursesForProvider>()))
                .ReturnsAsync((IReadOnlyCollection<Course>)new[] { MakeCourse("Test Course", venueId) });
            var controller = GetController();

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProviderCoursesViewModel>(viewResult.Model);
            Assert.True(model.HasFilters);
        }

        [Fact]
        public async Task Index_ActiveAttendancePatternFilter_HasFiltersIsTrue()
        {
            SetupUkprn(10012345);
            SetupNoCourseCache();
            SetupSearchState(new ProviderCourseSearchState { AttendancePatternFilter = new[] { "Daytime" } });
            _mockSqlQueryDispatcher
                .Setup(m => m.ExecuteQuery(It.IsAny<GetCoursesForProvider>()))
                .ReturnsAsync((IReadOnlyCollection<Course>)new[]
                {
                    MakeCourse("Test Course", configureRun: r => r.AttendancePattern = CourseAttendancePattern.Daytime)
                });
            var controller = GetController();

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProviderCoursesViewModel>(viewResult.Model);
            Assert.True(model.HasFilters);
        }

        [Fact]
        public async Task Index_ActiveRegionFilter_HasFiltersIsTrue()
        {
            SetupUkprn(10012345);
            SetupNoCourseCache();
            SetupSearchState(new ProviderCourseSearchState { RegionFilter = new[] { "E12000001" } });
            _mockSqlQueryDispatcher
                .Setup(m => m.ExecuteQuery(It.IsAny<GetCoursesForProvider>()))
                .ReturnsAsync((IReadOnlyCollection<Course>)new[]
                {
                    MakeCourse("North East Course", configureRun: r =>
                    {
                        r.SubRegionIds = new[] { "E06000001" };
                        r.National = false;
                    })
                });
            var controller = GetController();

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProviderCoursesViewModel>(viewResult.Model);
            Assert.True(model.HasFilters);
        }

        [Fact]
        public async Task Index_NonLarsCourseFlag_CallsGetNonLarsCoursesForProvider()
        {
            SetupUkprn(10012345);
            SetupNoCourseCache();
            SetupNoSearchState();
            var controller = GetController();

            await controller.Index(nlc: true);

            _mockSqlQueryDispatcher.Verify(
                m => m.ExecuteQuery(It.IsAny<GetNonLarsCoursesForProvider>()),
                Times.Once);
        }

        private ProviderCoursesController GetController()
        {
            var controller = new ProviderCoursesController(
                _mockLogger.Object,
                _mockCourseService.Object,
                _mockSqlQueryDispatcher.Object,
                _mockProviderContextProvider.Object);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { Session = _mockSession.Object }
            };

            return controller;
        }

        private void SetupUkprn(int ukprn)
        {
            var bytes = new byte[]
            {
                (byte)(ukprn >> 24),
                (byte)(0xFF & (ukprn >> 16)),
                (byte)(0xFF & (ukprn >> 8)),
                (byte)(0xFF & ukprn)
            };
            _mockSession.Setup(m => m.TryGetValue("UKPRN", out bytes)).Returns(true);
        }

        private void SetupNoSearchState()
        {
            byte[] noBytes = null;
            _mockSession.Setup(m => m.TryGetValue("ProviderCoursesSearchState", out noBytes)).Returns(false);
        }

        private void SetupSearchState(ProviderCourseSearchState state)
        {
            var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(state));
            _mockSession.Setup(m => m.TryGetValue("ProviderCoursesSearchState", out bytes)).Returns(true);
        }

        private void SetupNoCourseCache()
        {
            byte[] noBytes = null;
            _mockSession.Setup(m => m.TryGetValue("ProviderCourses", out noBytes)).Returns(false);
        }

        private static Course MakeCourse(
            string courseName,
            Guid? venueId = null,
            string notionalNVQLevelv2 = "3",
            string learnAimRef = "TEST123",
            string learnAimRefTypeDesc = "Test Desc",
            string providerCourseId = null,
            Action<CourseRun> configureRun = null)
        {
            var run = new CourseRun
            {
                CourseRunId = Guid.NewGuid(),
                CourseName = courseName,
                DeliveryMode = CourseDeliveryMode.Online,
                FlexibleStartDate = true,
                DurationUnit = CourseDurationUnit.Hours,
                SubRegionIds = Array.Empty<string>(),
                VenueId = venueId,
                ProviderCourseId = providerCourseId
            };

            configureRun?.Invoke(run);

            return new Course
            {
                CourseId = Guid.NewGuid(),
                LearnAimRef = learnAimRef,
                LearnAimRefTitle = "Test Qualification",
                LearnAimRefTypeDesc = learnAimRefTypeDesc,
                NotionalNVQLevelv2 = notionalNVQLevelv2,
                CourseRuns = new List<CourseRun> { run }
            };
        }
    }
}
