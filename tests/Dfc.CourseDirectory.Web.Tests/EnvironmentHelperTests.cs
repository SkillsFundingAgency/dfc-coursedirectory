using Dfc.CourseDirectory.Web.Helpers;
using Xunit;
using FluentAssertions;
using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Models.Environment;
using Dfc.CourseDirectory.Models.Interfaces.Environment;
using Microsoft.Extensions.Options;

namespace Dfc.CourseDirectory.Web.Tests
{
    public class EnvironmentHelperTests
    {
        [Fact]
        public void When_Value_Is_Invalid_Return_Undefined()
        {
            var options = new OptionsWrapper<EnviromentSettings>(new EnviromentSettings
            {
                EnvironmentName = "2sdsfdsf"
            });

            // Arrange
            var helperUnderTest = new EnvironmentHelper(options);

            // Act
            var environmentType = helperUnderTest.GetEnvironmentType();

            // Assert

            environmentType.Should().Be(EnvironmentType.Undefined);
        }
        [Fact]
        public void When_Value_Is_Development_Return_Development()
        {
            var options = new OptionsWrapper<EnviromentSettings>(new EnviromentSettings
            {
                EnvironmentName = "Development"
            });
            // Arrange
            var helperUnderTest = new EnvironmentHelper(options);

            // Act
            var environmentType = helperUnderTest.GetEnvironmentType();

            // Assert

            environmentType.Should().Be(EnvironmentType.Development);
        }
        [Fact]
        public void When_Value_Is_Development_With_Incorrect_Case_Return_Development()
        {
            var options = new OptionsWrapper<EnviromentSettings>(new EnviromentSettings
            {
                EnvironmentName = "DeVeloPment"
            });
            // Arrange
            var helperUnderTest = new EnvironmentHelper(options);

            // Act
            var environmentType = helperUnderTest.GetEnvironmentType();

            // Assert

            environmentType.Should().Be(EnvironmentType.Development);
        }
        [Fact]
        public void When_Value_Is_SIT_Return_SIT()
        {
            var options = new OptionsWrapper<EnviromentSettings>(new EnviromentSettings
            {
                EnvironmentName = "SIT"
            });
            // Arrange
            var helperUnderTest = new EnvironmentHelper(options);

            // Act
            var environmentType = helperUnderTest.GetEnvironmentType();

            // Assert

            environmentType.Should().Be(EnvironmentType.SIT);
        }
        [Fact]
        public void When_Value_Is_PreProduction_Return_PreProduction()
        {
            var options = new OptionsWrapper<EnviromentSettings>(new EnviromentSettings
            {
                EnvironmentName = "PreProduction"
            });
            // Arrange
            var helperUnderTest = new EnvironmentHelper(options);

            // Act
            var environmentType = helperUnderTest.GetEnvironmentType();

            // Assert

            environmentType.Should().Be(EnvironmentType.PreProduction);
        }
        [Fact]
        public void When_Value_Is_Production_Return_Production()
        {
            var options = new OptionsWrapper<EnviromentSettings>(new EnviromentSettings
            {
                EnvironmentName = "Production"
            });
            // Arrange
            var helperUnderTest = new EnvironmentHelper(options);

            // Act
            var environmentType = helperUnderTest.GetEnvironmentType();

            // Assert

            environmentType.Should().Be(EnvironmentType.Production);
        }
    }
}
