using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Services;
using Dfc.CourseDirectory.Web.Validation;
using Dfc.CourseDirectory.WebV2.ViewModels.EditCourse;
using FluentValidation.TestHelper;
using Moq;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.Validation
{
    public class EditCourseRunSaveViewModelValidatorTests
    {
        private readonly EditCourseRunSaveViewModelValidator _validator;
        private readonly Mock<IWebRiskService> _webRiskServiceMock;
        private readonly Mock<IClock> _clockMock;
        private readonly IReadOnlyCollection<Region> _allRegions;

        public EditCourseRunSaveViewModelValidatorTests()
        {
            _webRiskServiceMock = new Mock<IWebRiskService>();
            _clockMock = new Mock<IClock>();
            _allRegions = new List<Region> {
                new Region {
                    Id = "E12000001",
                    Name = "North East",
                    Latitude=52.3647,
                    Longitude=51.24789,
                    SubRegions = new List<Region>
                    {
                        new Region { Id = "E06000001", Name = "County Durham", Latitude=42.3647, Longitude=51.24789 },
                        new Region { Id = "E06000002", Name = "Darlington", Latitude=45.3647, Longitude=50.24789 },
                        new Region { Id = "E06000011", Name = "East Riding of Yorkshire", Latitude=48.3647, Longitude=46.24789 }
                    }
                }
            };
            _validator = new EditCourseRunSaveViewModelValidator(_allRegions, _clockMock.Object, _webRiskServiceMock.Object);
        }

        [Fact]
        public async Task ShouldHaveError_WhenDeliveryModeIsBlendedLearningAndAttendanceModeIsNull()
        {
            var model = new EditCourseRunSaveViewModel
            {
                CourseName = "Testing",
                Cost = "45.00",
                DurationLength = "10",
                DurationUnit = CourseDurationUnit.Days,
                AttendanceMode = null,
                DeliveryMode = CourseDeliveryMode.BlendedLearning,
                Url = "www.gov.uk"
            };
            var result = await _validator.TestValidateAsync(model);
            result.ShouldHaveValidationErrorFor(c => c.AttendanceMode);
        }

        [Fact]
        public async Task ShouldHaveError_WhenDeliveryModeIsClassroomBasedAndAttendanceModeIsNull()
        {
            var model = new EditCourseRunSaveViewModel
            {
                CourseName = "Testing",
                Cost = "45.00",
                DurationLength = "10",
                DurationUnit = CourseDurationUnit.Days,
                AttendanceMode = null,
                DeliveryMode = CourseDeliveryMode.ClassroomBased,
                Url = "www.gov.uk"
            };
            var result = await _validator.TestValidateAsync(model);
            result.ShouldHaveValidationErrorFor(c => c.AttendanceMode);
        }

        [Fact]
        public void ShouldNotHaveError_WhenCostIsValid()
        {
            var model = new EditCourseRunSaveViewModel
            {
                Cost = "100.00"
            };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(c => c.CostDecimal);
        }

        [Fact]
        public void ShouldHaveError_WhenCostIsInValid()
        {
            var model = new EditCourseRunSaveViewModel
            {
                CourseName = "Testing",
                Cost = "45re.00",
                DurationLength = "10",
                DurationUnit = CourseDurationUnit.Days,
                AttendanceMode = null,
                DeliveryMode = CourseDeliveryMode.ClassroomBased
            };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(c => c.CostDecimal);

            Assert.NotNull(result.Errors.FirstOrDefault(e => e.ErrorMessage == "Enter a real cost"));
        }

        [Fact]
        public void ShouldHaveError_WhenCostAndCostDescriptionBothAreNotProvided()
        {
            var model = new EditCourseRunSaveViewModel
            {
                CourseName = "Testing",
                DurationLength = "10",
                DurationUnit = CourseDurationUnit.Days,
                AttendanceMode = null,
                DeliveryMode = CourseDeliveryMode.ClassroomBased
            };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(c => c.CostDecimal);

            Assert.NotNull(result.Errors.FirstOrDefault(e => e.ErrorMessage == "Enter a cost or cost description"));
        }

        [Fact]
        public async Task ShouldHaveError_WhenCourseNameIsNull()
        {
            var model = new EditCourseRunSaveViewModel
            {
                CourseName = null,
                Cost = "45.00",
                DurationLength = "10",
                DurationUnit = CourseDurationUnit.Days,
                AttendanceMode = CourseAttendancePattern.Daytime,
                DeliveryMode = CourseDeliveryMode.BlendedLearning,
                Url = "www.gov.uk"
            };
            var result = await _validator.TestValidateAsync(model);
            result.ShouldHaveValidationErrorFor(c => c.CourseName);
        }

        [Fact]
        public async Task ShouldHaveError_WhenDeliveryModeIsInvalid()
        {
            var model = new EditCourseRunSaveViewModel
            {
                CourseName = "Test Course name",
                Cost = "45.00",
                DurationLength = "10",
                DurationUnit = CourseDurationUnit.Days,
                AttendanceMode = CourseAttendancePattern.Daytime,
                DeliveryMode = (CourseDeliveryMode)999, // Invalid Enum Value,
                Url = "www.gov.uk"
            };

            var result = await _validator.TestValidateAsync(model);
            result.ShouldHaveValidationErrorFor(c => c.DeliveryMode);
        }

        [Fact]
        public async Task ShouldHaveError_WhenDurationLengthIsInvalid()
        {
            var model = new EditCourseRunSaveViewModel
            {
                CourseName = "Test Course name",
                Cost = "45.00",
                DurationLength = "-5", // Invalid duration length
                DurationUnit = CourseDurationUnit.Days,
                AttendanceMode = CourseAttendancePattern.Daytime,
                DeliveryMode = CourseDeliveryMode.WorkBased,
                Url = "www.gov.uk"
            };
            var result = await _validator.TestValidateAsync(model);
            result.ShouldHaveValidationErrorFor(c => c.DurationLengthInt);
        }

        [Fact]
        public async Task ShouldNotHaveError_WhenUrlIsValid()
        {
            var model = new EditCourseRunSaveViewModel
            {
                CourseName = "Test Course name",
                Cost = "45.00",
                DurationLength = "5",
                DurationUnit = CourseDurationUnit.Days,
                National = true,
                DeliveryMode = CourseDeliveryMode.WorkBased,
                Url = "http://valid-url.com"
            };
            _webRiskServiceMock.Setup(m => m.CheckForSecureUri(It.IsAny<string>())).ReturnsAsync(true);

            var result = await _validator.TestValidateAsync(model);
            result.ShouldNotHaveValidationErrorFor(c => c.Url);
        }

        [Fact]
        public async Task ShouldNotHaveError_WhenDeliveryModeIsWorkBasedAndVenueIdIsEmpty()
        {
            var model = new EditCourseRunSaveViewModel
            {
                CourseName = "Test Course name",
                Cost = "45.00",
                DurationLength = "5",
                DurationUnit = CourseDurationUnit.Days,
                AttendanceMode = CourseAttendancePattern.Daytime,
                DeliveryMode = CourseDeliveryMode.WorkBased,
                Url = "http://valid-url.com",
                VenueId = Guid.Empty
            };
            var result = await _validator.TestValidateAsync(model);
            result.ShouldNotHaveValidationErrorFor(c => c.VenueId);
        }

        [Fact]
        public async Task ShouldHaveError_WhenDeliveryModeIsBlendedLearningAndVenueIdIsEmpty()
        {
            var model = new EditCourseRunSaveViewModel
            {
                CourseName = "Test Course name",
                Cost = "45.00",
                DurationLength = "5",
                DurationUnit = CourseDurationUnit.Days,
                AttendanceMode = CourseAttendancePattern.Daytime,
                DeliveryMode = CourseDeliveryMode.BlendedLearning,
                Url = "http://valid-url.com",
                VenueId = Guid.Empty
            };
            var result = await _validator.TestValidateAsync(model);
            result.ShouldHaveValidationErrorFor(c => c.VenueId);
        }

        [Fact]
        public async Task ShouldHaveError_WhenSelectedRegionsAreSpecifiedAndNationalIsTrue()
        {
            var model = new EditCourseRunSaveViewModel
            {
                CourseName = "Test Course name",
                Cost = "45.00",
                DurationLength = "5",
                DurationUnit = CourseDurationUnit.Days,
                AttendanceMode = CourseAttendancePattern.Daytime,
                National = true,
                DeliveryMode = CourseDeliveryMode.ClassroomBased,
                SelectedRegions = new string[] { "E06000001", "E06000002", "E06000011" },
                VenueId = Guid.NewGuid(),
                Url = "http://valid-url.com",
            };
            _webRiskServiceMock.Setup(m => m.CheckForSecureUri(It.IsAny<string>())).ReturnsAsync(true);

            var result = await _validator.TestValidateAsync(model);
            result.ShouldHaveValidationErrorFor(c => c.SelectedRegions);

            Assert.NotNull(result.Errors.FirstOrDefault(e => e.ErrorMessage == "You must remove National delivery"));
            Assert.NotNull(result.Errors.FirstOrDefault(e => e.ErrorMessage == "You must remove Sub regions"));
        }

        [Fact]
        public async Task ShouldHaveError_WhenSelectedRegionsAreNotSpecifiedAndDeliveryModeIsWorkBasedAndNationalIsFalse()
        {
            var model = new EditCourseRunSaveViewModel
            {
                CourseName = "Test Course name",
                Cost = "45.00",
                DurationLength = "5",
                DurationUnit = CourseDurationUnit.Days,
                AttendanceMode = CourseAttendancePattern.Daytime,
                National = false,
                DeliveryMode = CourseDeliveryMode.WorkBased,
                SelectedRegions = null,
                VenueId = Guid.NewGuid(),
                Url = "http://valid-url.com",
            };
            _webRiskServiceMock.Setup(m => m.CheckForSecureUri(It.IsAny<string>())).ReturnsAsync(true);

            var result = await _validator.TestValidateAsync(model);
            result.ShouldHaveValidationErrorFor(c => c.SelectedRegions);

            Assert.NotNull(result.Errors.FirstOrDefault(e => e.ErrorMessage == "Select at least one sub region"));
        }

        [Fact]
        public async Task ShouldHaveError_WhenOneOfTheSelectedRegionsIsInvalid()
        {
            var model = new EditCourseRunSaveViewModel
            {
                CourseName = "Test Course name",
                Cost = "45.00",
                DurationLength = "5",
                DurationUnit = CourseDurationUnit.Days,
                AttendanceMode = CourseAttendancePattern.Daytime,
                National = false,
                DeliveryMode = CourseDeliveryMode.WorkBased,
                SelectedRegions = new string[] { "E06000qw1", "E06000002", "E06000011" },
                VenueId = Guid.NewGuid(),
                Url = "http://valid-url.com",
            };
            _webRiskServiceMock.Setup(m => m.CheckForSecureUri(It.IsAny<string>())).ReturnsAsync(true);

            var result = await _validator.TestValidateAsync(model);
            result.ShouldHaveValidationErrorFor(c => c.SelectedRegions);

            Assert.NotNull(result.Errors.FirstOrDefault(e => e.ErrorMessage == "Enter a real Sub region"));
        }

        [Fact]
        public async Task ShouldNotHaveError_WhenValidRegionsAreSpecifiedAndDeliveryModeIsWorkBasedAndNationalIsFalse()
        {
            var model = new EditCourseRunSaveViewModel
            {
                CourseName = "Test Course name",
                Cost = "45.00",
                DurationLength = "5",
                DurationUnit = CourseDurationUnit.Days,
                AttendanceMode = CourseAttendancePattern.Daytime,
                National = false,
                DeliveryMode = CourseDeliveryMode.WorkBased,
                SelectedRegions = new string[] { "E06000001", "E06000002", "E06000011" },
                VenueId = Guid.NewGuid(),
                Url = "http://valid-url.com",
            };
            _webRiskServiceMock.Setup(m => m.CheckForSecureUri(It.IsAny<string>())).ReturnsAsync(true);

            var result = await _validator.TestValidateAsync(model);
            result.ShouldNotHaveValidationErrorFor(c => c.SelectedRegions);
        }

        [Fact]
        public async Task ShouldNotHaveError_WhenStudyModeIsValid()
        {
            var model = new EditCourseRunSaveViewModel
            {
                CourseName = "Test Course name",
                Cost = "45.00",
                DurationLength = "5",
                DurationUnit = CourseDurationUnit.Days,
                AttendanceMode = CourseAttendancePattern.Daytime,
                DeliveryMode = CourseDeliveryMode.ClassroomBased,
                StudyMode = CourseStudyMode.PartTime,
                Url = "http://valid-url.com",
                VenueId = Guid.NewGuid()
            };
            _webRiskServiceMock.Setup(m => m.CheckForSecureUri(It.IsAny<string>())).ReturnsAsync(true);

            var result = await _validator.TestValidateAsync(model);
            result.ShouldNotHaveValidationErrorFor(c => c.StudyMode);
        }

        [Fact]
        public async Task ShouldNotHaveError_WhenNationalDeliveryIsValid()
        {
            var model = new EditCourseRunSaveViewModel
            {
                CourseName = "Test Course name",
                Cost = "45.00",
                DurationLength = "5",
                DurationUnit = CourseDurationUnit.Days,
                AttendanceMode = CourseAttendancePattern.Daytime,
                National = true,
                DeliveryMode = CourseDeliveryMode.WorkBased
            };
            var result = await _validator.TestValidateAsync(model);
            result.ShouldNotHaveValidationErrorFor(c => c.National);
        }
    }
}
