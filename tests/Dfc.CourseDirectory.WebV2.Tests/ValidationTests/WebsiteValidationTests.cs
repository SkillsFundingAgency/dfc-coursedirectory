﻿using Dfc.CourseDirectory.Core.Validation;
using Dfc.CourseDirectory.Core.Services;
using FluentValidation;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.ValidationTests
{
    public class WebsiteValidationTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void EmptyUrlPassesValidation(string website)
        {
            // Arrange
            var model = new Model() { Website = website };
            var validator = new Validator();

            // Act
            var validationResult = validator.Validate(model);

            // Assert
            Assert.True(validationResult.IsValid);
        }

        [Fact]
        public void MissingProtocolPassesValidation()
        {
            // Arrange
            var website = "google.com";
            var model = new Model() { Website = website };
            var validator = new Validator();

            // Act
            var validationResult = validator.Validate(model);

            // Assert
            Assert.True(validationResult.IsValid);
        }

        [Fact]
        public void NonHttpProtocolFailsValidation()
        {
            // Arrange
            var website = "ftp://google.com";
            var model = new Model() { Website = website };
            var validator = new Validator();

            // Act
            var validationResult = validator.Validate(model);

            // Assert
            Assert.False(validationResult.IsValid);
        }

        [Fact]
        public void RelativeUrlFailsValidation()
        {
            // Arrange
            var website = "/foo";
            var model = new Model() { Website = website };
            var validator = new Validator();

            // Act
            var validationResult = validator.Validate(model);

            // Assert
            Assert.False(validationResult.IsValid);
        }

        [Fact]
        public void HttpUrlPassesValidation()
        {
            // Arrange
            var website = "http://google.com";
            var model = new Model() { Website = website };
            var validator = new Validator();

            // Act
            var validationResult = validator.Validate(model);

            // Assert
            Assert.True(validationResult.IsValid);
        }

        [Fact]
        public void HttpsUrlPassesValidation()
        {
            // Arrange
            var website = "https://google.com";
            var model = new Model() { Website = website };
            var validator = new Validator();

            // Act
            var validationResult = validator.Validate(model);

            // Assert
            Assert.True(validationResult.IsValid);
        }

        [Fact]
        public void WebsiteWithSpacesFailsValidation()
        {
            // Arrange
            var website = "https://google.com/t level";
            var model = new Model() { Website = website };
            var validator = new Validator();

            // Act
            var validationResult = validator.Validate(model);

            // Assert
            Assert.False(validationResult.IsValid);
        }

        private class Model
        {
            public string Website { get; set; }
        }

        private class Validator : AbstractValidator<Model>
        {
            public Validator()
            {
                RuleFor(m => m.Website).Apply(Rules.Website);
            }
        }
    }
}
