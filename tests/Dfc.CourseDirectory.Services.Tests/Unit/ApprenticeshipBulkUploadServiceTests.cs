﻿using Dfc.CourseDirectory.Services.BulkUploadService;
using Dfc.CourseDirectory.Services.Tests.Unit.Helpers;
using FluentAssertions;
using System;
using System.IO;
using Xunit;

namespace Dfc.CourseDirectory.Services.Tests
{
    /// <summary>
    /// Unit tests for the ApprenticeshipBulkUploadService
    /// </summary>
    public class ApprenticeshipBulkUploadServiceTests
    {
        public class CountCSVLines
        {
            [Fact]
            public void When_File_Is_Empty_Then_Return0()
            {
                // Arrange

                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger);
                Stream stream = new MemoryStream();

                // Act

                var count = serviceUnderTest.CountCsvLines(stream);

                // Assert

                count.Should().Be(0);
            }

            [Fact]
            public void When_File_Has_2Rows_Then_Return2()
            {
                // Arrange

                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger);
                Stream stream = CsvStreams.Two_Rows();

                // Act

                var count = serviceUnderTest.CountCsvLines(stream);

                // Assert

                count.Should().Be(2);
            }
        }

        public class ValidateCSVFormat
        {
            [Fact]
            public void When_File_Is_Null_Then_ThrowException()
            {
                // Arrange

                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger);
                Stream stream = null;

                // Act

                Action act = () => serviceUnderTest.ValidateCSVFormat(stream);

                // Assert

                act.Should().Throw<ArgumentNullException>().WithMessage("*Parameter name: stream*");
            }

            [Fact]
            public void When_File_Is_Empty_Then_ReturnError()
            {
                // Arrange

                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger);
                Stream stream = new MemoryStream();

                // Act

                var errors = serviceUnderTest.ValidateCSVFormat(stream);

                // Assert

                errors.Should().NotBeNull();
                errors.Should().HaveCount(1);
                errors[0].Should().Be("File is empty.");
            }

            [Fact]
            public void When_File_Has_NoHeaderRow_Then_ReturnError()
            {
                // Arrange

                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger);
                Stream stream = CsvStreams.No_Header_Row();

                // Act

                var errors = serviceUnderTest.ValidateCSVFormat(stream);

                // Assert

                errors.Should().NotBeNull();
                errors.Should().HaveCount(1);
                errors[0].Should().Be("Invalid header row. Header with name 'STANDARD_CODE' was not found.");
            }

            [Fact]
            public void When_File_Has_OnlyHeaderRow_Then_ReturnError()
            {
                // Arrange

                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger);
                Stream stream = CsvStreams.Only_Header_Row();

                // Act

                var errors = serviceUnderTest.ValidateCSVFormat(stream);

                // Assert

                errors.Should().NotBeNull();
                errors.Should().HaveCount(1);
                errors[0].Should().Be("No apprenticeship data present in the file.");
            }

            [Fact]
            public void When_Field_STANDARD_CODE_Is_PresentAndNonNumeric_Then_ReturnError()
            {
                // Arrange

                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger);
                Stream stream = CsvStreams.InvalidField_STANDARD_CODE_MustBeNumericIfPresent();

                // Act

                var errors = serviceUnderTest.ValidateCSVFormat(stream);

                // Assert

                errors.Should().NotBeNull();
                errors.Should().HaveCount(1);
                errors[0].Should().Be("Validation error on row 2. Field STANDARD_CODE must be numeric if present.");
            }

            [Fact]
            public void When_Field_STANDARD_VERSION_Is_PresentAndNonNumeric_Then_ReturnError()
            {
                // Arrange

                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger);
                Stream stream = CsvStreams.InvalidField_STANDARD_VERSION_MustBeNumericIfPresent();

                // Act

                var errors = serviceUnderTest.ValidateCSVFormat(stream);

                // Assert

                errors.Should().NotBeNull();
                errors.Should().HaveCount(1);
                errors[0].Should().Be("Validation error on row 2. Field STANDARD_VERSION must be numeric if present.");
            }

            [Fact]
            public void When_Field_FRAMEWORK_CODE_Is_PresentAndNonNumeric_Then_ReturnError()
            {
                // Arrange

                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger);
                Stream stream = CsvStreams.InvalidField_FRAMEWORK_CODE_MustBeNumericIfPresent();

                // Act

                var errors = serviceUnderTest.ValidateCSVFormat(stream);

                // Assert

                errors.Should().NotBeNull();
                errors.Should().HaveCount(1);
                errors[0].Should().Be("Validation error on row 2. Field FRAMEWORK_CODE must be numeric if present.");
            }

            [Fact]
            public void When_Field_FRAMEWORK_PROG_TYPE_Is_PresentAndNonNumeric_Then_ReturnError()
            {
                // Arrange

                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger);
                Stream stream = CsvStreams.InvalidField_FRAMEWORK_PROG_TYPE_MustBeNumericIfPresent();

                // Act

                var errors = serviceUnderTest.ValidateCSVFormat(stream);

                // Assert

                errors.Should().NotBeNull();
                errors.Should().HaveCount(1);
                errors[0].Should().Be("Validation error on row 2. Field FRAMEWORK_PROG_TYPE must be numeric if present.");
            }

            [Fact]
            public void When_Field_FRAMEWORK_PATHWAY_CODE_Is_PresentAndNonNumeric_Then_ReturnError()
            {
                // Arrange

                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger);
                Stream stream = CsvStreams.InvalidField_FRAMEWORK_PATHWAY_CODE_MustBeNumericIfPresent();

                // Act

                var errors = serviceUnderTest.ValidateCSVFormat(stream);

                // Assert

                errors.Should().NotBeNull();
                errors.Should().HaveCount(1);
                errors[0].Should().Be("Validation error on row 2. Field FRAMEWORK_PATHWAY_CODE must be numeric if present.");
            }

            [Fact]
            public void When_Row_Has_StandardAndFrameworkValuesPresent_Then_ReturnError()
            {
                // Arrange

                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger);
                Stream stream = CsvStreams.InvalidRow_StandardAndFrameworkValuesMissing();

                // Act

                var errors = serviceUnderTest.ValidateCSVFormat(stream);

                // Assert

                errors.Should().NotBeNull();
                errors.Should().HaveCount(1);
                errors[0].Should().Be("Validation error on row 2. Values for Both Standard AND Framework cannot be present in the same row.");
            }

            [Fact]
            public void When_Field_APPRENTICESHIP_INFORMATION_Is_Missing_Then_ReturnError()
            {
                // Arrange

                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger);
                Stream stream = CsvStreams.InvalidField_APPRENTICESHIP_INFORMATION_Missing();

                // Act

                var errors = serviceUnderTest.ValidateCSVFormat(stream);

                // Assert

                errors.Should().NotBeNull();
                errors.Should().HaveCount(1);
                errors[0].Should().Be("Validation error on row 2. Field APPRENTICESHIP_INFORMATION is required.");
            }

            [Fact]
            public void When_Field_APPRENTICESHIP_INFORMATION_Is_LongerThan750Chars_Then_ReturnError()
            {
                // Arrange

                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger);
                Stream stream = CsvStreams.InvalidField_APPRENTICESHIP_INFORMATION_751Chars();

                // Act

                var errors = serviceUnderTest.ValidateCSVFormat(stream);

                // Assert

                errors.Should().NotBeNull();
                errors.Should().HaveCount(1);
                errors[0].Should().Be("Validation error on row 2. Field APPRENTICESHIP_INFORMATION maximum length is 750 characters.");
            }
        }
    }
}
