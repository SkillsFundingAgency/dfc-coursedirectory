using Dfc.CourseDirectory.Services.BulkUploadService;
using Dfc.CourseDirectory.Services.Tests.Unit.Helpers;
using FluentAssertions;
using Moq;
using System;
using System.IO;
using System.Net;
using Dfc.CourseDirectory.Services.Interfaces.ApprenticeshipService;
using Dfc.CourseDirectory.Services.Tests.Unit.Mocks;
using Xunit;
namespace Dfc.CourseDirectory.Services.Tests.Unit
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
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(null);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock);
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
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(null);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock);
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
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(null);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock);
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
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(null);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock);
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
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(null);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock);
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
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(null);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock);
                Stream stream = CsvStreams.Only_Header_Row();

                // Act

                var errors = serviceUnderTest.ValidateCSVFormat(stream);

                // Assert

                errors.Should().NotBeNull();
                errors.Should().HaveCount(1);
                errors[0].Should().Be("No apprenticeship data present in the file.");
            }

           
        }

        public class ValidateCSVContents
        {
            [Fact]
            public void When_Field_STANDARD_CODE_Is_PresentAndNonNumeric_Then_ReturnError()
            {
                // Arrange
                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(null);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock);
                
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
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(null);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock);
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
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(null);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock);
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
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(null);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock);
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
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(null);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock);
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
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(null);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock);
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
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(null);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock);
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
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(null);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock);
                Stream stream = CsvStreams.InvalidField_APPRENTICESHIP_INFORMATION_751Chars();

                // Act

                var errors = serviceUnderTest.ValidateCSVFormat(stream);

                // Assert

                errors.Should().NotBeNull();
                errors.Should().HaveCount(1);
                errors[0].Should().Be("Validation error on row 2. Field APPRENTICESHIP_INFORMATION maximum length is 750 characters.");
            }
            [Fact]
            public void When_Field_APPRENTICESHIP_WEBPAGE_Fails_Regex_InvalidCharacter_Then_ReturnError()
            {
                // Arrange

                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService( null);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock);
                Stream stream = CsvStreams.InvalidField_APPRENTICESHIP_WEBPAGE_Regex_Error_Invalid_Character();

                // Act

                var errors = serviceUnderTest.ValidateCSVFormat(stream);

                // Assert

                errors.Should().NotBeNull();
                errors.Should().HaveCount(1);
                errors[0].Should().Be("Validation error on row 2. Field APPRENTICESHIP_WEBPAGE format of URL is incorrect.");
            }
            [Fact]
            public void When_Field_APPRENTICESHIP_WEBPAGE_Is_LongerThan255Chars_Then_ReturnError()
            {
                // Arrange

                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(null);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock);
                Stream stream = CsvStreams.InvalidField_APPRENTICESHIP_WEBPAGE_256Chars();

                // Act

                var errors = serviceUnderTest.ValidateCSVFormat(stream);

                // Assert

                errors.Should().NotBeNull();
                errors.Should().HaveCount(1);
                errors[0].Should().Be("Validation error on row 2. Field APPRENTICESHIP_WEBPAGE maximum length is 255 characters.");
            }
            [Fact]
            public void When_Field_APPRENTICESHIP_WEBPAGE_Is_Empty_Then_ReturnNoErrors()
            {
                // Arrange

                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(null);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock);
                Stream stream = CsvStreams.Valid_Row_Empty_APPRENTICESHIP_WEBPAGE();

                // Act

                var errors = serviceUnderTest.ValidateCSVFormat(stream);

                // Assert

                errors.Should().BeNullOrEmpty();
                errors.Should().HaveCount(0);
            }
            [Fact]
            public void When_Field_CONTACT_EMAIL_Is_Missing_Then_ReturnError()
            {
                // Arrange

                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(null);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock);
                Stream stream = CsvStreams.InvalidField_CONTACT_EMAIL_Missing();

                // Act

                var errors = serviceUnderTest.ValidateCSVFormat(stream);

                // Assert

                errors.Should().NotBeNull();
                errors.Should().HaveCount(1);
                errors.Should().Contain("Validation error on row 2. Field CONTACT_EMAIL is required.");
            }
            [Fact]
            public void When_Field_CONTACT_EMAIL_Is_LongerThan255Chars_Then_ReturnError()
            {
                // Arrange

                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(null);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock);
                Stream stream = CsvStreams.InvalidField_CONTACT_EMAIL_256_Chars();

                // Act

                var errors = serviceUnderTest.ValidateCSVFormat(stream);

                // Assert

                errors.Should().NotBeNull();
                errors.Should().HaveCount(1);
                errors.Should().Contain("Validation error on row 2. Field CONTACT_EMAIL maximum length is 255 characters.");
            }
            [Fact]
            public void When_Field_CONTACT_EMAIL_Is_Fails_Regex_Then_ReturnError()
            {
                // Arrange

                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(null);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock);
                Stream stream = CsvStreams.InvalidField_CONTACT_EMAIL_Regex_Invalid_character();

                // Act

                var errors = serviceUnderTest.ValidateCSVFormat(stream);

                // Assert

                errors.Should().NotBeNull();
                errors.Should().HaveCount(1);
                errors[0].Should().Be("Validation error on row 2. Field CONTACT_EMAIL needs a valid email.");
            }
            [Fact]
            public void When_Field_CONTACT_PHONE_Is_Missing_Then_ReturnError()
            {
                // Arrange

                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(null);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock);
                Stream stream = CsvStreams.InvalidField_CONTACT_PHONE_Missing();

                // Act

                var errors = serviceUnderTest.ValidateCSVFormat(stream);

                // Assert

                errors.Should().NotBeNull();
                errors.Should().HaveCount(1);
                errors.Should().Contain("Validation error on row 2. Field CONTACT_PHONE is required.");
            }
            [Fact]
            public void When_Field_CONTACT_PHONE_Is_LongerThan30Chars_Then_ReturnError()
            {
                // Arrange

                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(null);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock);
                Stream stream = CsvStreams.InvalidField_CONTACT_PHONE_Longer_Than_30_Chars();

                // Act

                var errors = serviceUnderTest.ValidateCSVFormat(stream);

                // Assert

                errors.Should().NotBeNull();
                errors.Should().HaveCount(1);
                errors.Should().Contain("Validation error on row 2. Field CONTACT_PHONE maximum length is 30 characters.");
            }
            [Fact]
            public void When_Field_CONTACT_PHONE_Is_NonNumerical_Then_ReturnError()
            {
                // Arrange

                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(null);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock);
                Stream stream = CsvStreams.InvalidField_CONTACT_PHONE_NonNumeric();

                // Act

                var errors = serviceUnderTest.ValidateCSVFormat(stream);

                // Assert

                errors.Should().NotBeNull();
                errors.Should().HaveCount(1);
                errors[0].Should().Be("Validation error on row 2. Field CONTACT_PHONE must be numeric if present.");
            }
            [Fact]
            public void When_Field_CONTACT_URL_Is_Empty_Then_ReturnSuccess()
            {
                // Arrange

                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(null);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock);
                Stream stream = CsvStreams.Valid_Row_Empty_CONTRACT_URL();

                // Act

                var errors = serviceUnderTest.ValidateCSVFormat(stream);

                // Assert

                errors.Should().BeNullOrEmpty();
                errors.Should().HaveCount(0);
            }
            [Fact]
            public void When_Field_CONTACT_URL_Is_Over_255Characters_Then_Return_Error()
            {
                // Arrange

                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(null);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock);
                Stream stream = CsvStreams.InvalidField_CONTACT_URL_256_Chars();

                // Act

                var errors = serviceUnderTest.ValidateCSVFormat(stream);

                // Assert

                errors.Should().NotBeNull();
                errors.Should().HaveCount(1);
                errors[0].Should().Be("Validation error on row 2. Field CONTACT_URL maximum length is 255 characters.");
            }
            [Fact]
            public void When_Field_CONTACT_URL_Is_Contains_Space_Then_Return_Error()
            {
                // Arrange
                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(null);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock);
                Stream stream = CsvStreams.InvalidField_CONTACT_URL_Invalid_URL_Space();

                // Act
                var errors = serviceUnderTest.ValidateCSVFormat(stream);

                // Assert
                errors.Should().NotBeNull();
                errors.Should().HaveCount(1);
                errors[0].Should().Be("Validation error on row 2. Field CONTACT_URL format of URL is incorrect.");
            }
            [Fact]
            public void When_Field_CONTACT_URL_Invalid_Format_Then_Return_Error()
            {
                // Arrange
                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(null);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock);
                Stream stream = CsvStreams.InvalidField_CONTACT_URL_Invalid_URL_Format();

                // Act
                var errors = serviceUnderTest.ValidateCSVFormat(stream);

                // Assert
                errors.Should().NotBeNull();
                errors.Should().HaveCount(1);
                errors[0].Should().Be("Validation error on row 2. Field CONTACT_URL format of URL is incorrect.");
            }
            [Fact]
            public void When_Field_DELIVERY_METHOD_Is_Missing_Then_ReturnError()
            {
                // Arrange

                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(null);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock);
                Stream stream = CsvStreams.InvalidField_DELIVERY_METHOD_Missing();

                // Act

                var errors = serviceUnderTest.ValidateCSVFormat(stream);

                // Assert

                errors.Should().NotBeNull();
                errors.Should().HaveCount(1);
                errors[0].Should().Be("Validation error on row 2. Field DELIVERY_METHOD is required.");
            }
            [Fact]
            public void When_Field_DELIVERY_METHOD_Is_Invalid_Then_ReturnError()
            {
                // Arrange
                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(null);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock);
                Stream stream = CsvStreams.InvalidField_DELIVERY_METHOD_Invalid();

                // Act

                var errors = serviceUnderTest.ValidateCSVFormat(stream);

                // Assert

                errors.Should().NotBeNull();
                errors.Should().HaveCount(1);
                errors[0].Should().Be("Validation error on row 2. Field DELIVERY_METHOD is invalid.");
            }
            [Fact]
            public void When_Field_DELIVERY_METHOD_Is_valid_Then_Return_No_Error()
            {
                // Arrange
                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(null);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock);
                Stream stream = CsvStreams.Valid_Row_DELIVERY_METHOD_Case_Insensitive_Correct_Values();

                // Act

                var errors = serviceUnderTest.ValidateCSVFormat(stream);

                // Assert

                errors.Should().BeNullOrEmpty();
                errors.Should().HaveCount(0);
            }
            [Fact]
            public void When_Field_VENUE_Is_Empty_Return_Success()
            {
                // Arrange
                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(null);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock);
                Stream stream = CsvStreams.Valid_Row_No_VENUE_Correct_Values();

                // Act

                var errors = serviceUnderTest.ValidateCSVFormat(stream);

                // Assert

                errors.Should().BeNullOrEmpty();
                errors.Should().HaveCount(0);
            }
            [Fact]
            public void When_Field_VENUE_Is_Over_255Characters_Then_Return_Error()
            {
                // Arrange
                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(null);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock);
                Stream stream = CsvStreams.Valid_Row_DELIVERY_METHOD_Case_Insensitive_Correct_Values();

                // Act

                var errors = serviceUnderTest.ValidateCSVFormat(stream);

                // Assert

                errors.Should().BeNullOrEmpty();
                errors.Should().HaveCount(0);
            }
        }

        public class StandardsAndFrameworksTests
        {
            [Fact]
            public void When_StandardCode_Is_Not_Valid_Return_Error()
            {
                // Arrange
                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var httpClient = HttpClientMockFactory.GetClient(SampleJsons.EmptyFile(), HttpStatusCode.NoContent);
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(httpClient);
                
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock);
                Stream stream = CsvStreams.InvalidField_STANDARD_CODE_InvalidNumber();

                // Act

                var errors = serviceUnderTest.ValidateCSVFormat(stream);

                // Assert
                errors.Should().NotBeNull();
                errors.Should().HaveCount(1);
                errors[0].Should().Be("Validation error on row 2. Invalid Standard Code or Version Number. Standard not found.");
            }
            [Fact]
            public void When_Duplicate_StandardCodes_Exist_Return_Error()
            {
                // Arrange
                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var httpClient = HttpClientMockFactory.GetClient(SampleJsons.EmptyFile(), HttpStatusCode.NoContent);
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(httpClient);
                
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock);
                Stream stream = CsvStreams.InvalidFile_Duplicate_STANDARD_CODES();

                // Act

                var errors = serviceUnderTest.ValidateCSVFormat(stream);

                // Assert
                errors.Should().NotBeNull();
                errors.Should().HaveCount(1);
                errors[0].Should().Be("Validation error on row 2. Invalid Standard Code or Version Number. Standard not found.");
            }
            [Fact]
            public void When_StandardCode_Is_Valid_Return_Success()
            {
                // Arrange
                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var httpClient = HttpClientMockFactory.GetClient(SampleJsons.SuccessfulStandardFile(), HttpStatusCode.OK);
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(httpClient);
                
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock);
                Stream stream = CsvStreams.ValidField_STANDARD_CODES();

                // Act

                var errors = serviceUnderTest.ValidateCSVFormat(stream);

                // Assert
                errors.Should().BeNullOrEmpty();
                errors.Should().HaveCount(0);
            }
            [Theory]
            [InlineData(HttpStatusCode.BadRequest, "Validation error on row 2. Invalid Standard Code or Version Number. Standard not found.")]
            public void GetStandardByCode_BadRequest_Exeception(HttpStatusCode code, string expectedError)
            {
                // Arrange
                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var httpClient = HttpClientMockFactory.GetClient(SampleJsons.EmptyFile(), code);
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(httpClient);
                
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock);
                Stream stream = CsvStreams.InvalidField_STANDARD_CODE_InvalidNumber();

                // Act

                var errors = serviceUnderTest.ValidateCSVFormat(stream);

                // Assert
                errors.Should().NotBeNull();
                errors.Should().HaveCount(1);
                errors[0].Should().Be(expectedError);
            }
            [Fact]
            public void When_FrameworkValues_Are_Not_Valid_Return_Error()
            {
                // Arrange
                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var httpClient = HttpClientMockFactory.GetClient(SampleJsons.EmptyFile(), HttpStatusCode.NoContent);
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(httpClient);
                
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock);
                Stream stream = CsvStreams.InvalidField_FRAMEWORK_Values_Invalid();

                // Act

                var errors = serviceUnderTest.ValidateCSVFormat(stream);

                // Assert
                errors.Should().NotBeNull();
                errors.Should().HaveCount(1);
                errors[0].Should().Be("Validation error on row 2. Invalid Framework Code, Prog Type, or Pathway Code. Framework not found.");
            }
            [Fact]
            public void When_FrameworkValues_Are_Valid_Return_Success()
            {
                // Arrange
                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var httpClient = HttpClientMockFactory.GetClient(SampleJsons.SuccessfulFrameworkFile(), HttpStatusCode.OK);
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(httpClient);
                
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock);
                Stream stream = CsvStreams.ValidField_FrameworkCodes_CODES();

                // Act

                var errors = serviceUnderTest.ValidateCSVFormat(stream);

                // Assert
                errors.Should().BeNullOrEmpty();
                errors.Should().HaveCount(0);
            }
        }
    }
}
