﻿using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Models.Auth;
using Dfc.CourseDirectory.Services.BulkUploadService;
using Dfc.CourseDirectory.Services.Tests.Unit.Helpers;
using Dfc.CourseDirectory.Services.Tests.Unit.Mocks;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Xunit;
namespace Dfc.CourseDirectory.Services.Tests.Unit
{
    /// <summary>
    /// Unit tests for the ApprenticeshipBulkUploadService
    /// </summary>
    public class ApprenticeshipBulkUploadServiceTests
    {
        private readonly AuthUserDetails _authUserDetails;
        public ApprenticeshipBulkUploadServiceTests()
        {
           _authUserDetails  = new AuthUserDetails(
            
                userId: Guid.NewGuid(),
                email : "email@testEmail.com",
                nameOfUser : "Test User",
                providerType : "Provider",
                roleId : Guid.NewGuid(),
                roleName : "Developer",
                ukPrn : "12345678",
                userName: "email@testEmail.com",
                providerId: Guid.NewGuid()
            );
        }
        public class CountCSVLines
        {
            
            [Fact]
            public void When_File_Is_Empty_Then_Return0()
            {
                // Arrange
                
                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(null);
                var venueMock = VenueServiceMockFactory.GetVenueService(null);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);
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
                var venueMock = VenueServiceMockFactory.GetVenueService(null);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);
                Stream stream = CsvStreams.Two_Rows();

                // Act

                var count = serviceUnderTest.CountCsvLines(stream);

                // Assert

                count.Should().Be(2);
            }
        }

        public class ValidateCSVFormat
        {
            private readonly AuthUserDetails _authUserDetails;
            public ValidateCSVFormat()
            {
               _authUserDetails = new AuthUserDetails(
            
                    userId: Guid.NewGuid(),
                    email : "email@testEmail.com",
                    nameOfUser : "Test User",
                    providerType : "Provider",
                    roleId : Guid.NewGuid(),
                    roleName : "Developer",
                    ukPrn : "12345678",
                    userName: "email@testEmail.com",
                    providerId: Guid.NewGuid()
                );
            }
            [Fact]
            public void When_File_Is_Null_Then_ThrowException()
            {
                // Arrange

                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(null);
                var venueMock = VenueServiceMockFactory.GetVenueService(null);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);
                Stream stream = null;

                // Act

                Action act = () => serviceUnderTest.ValidateAndUploadCSV(stream, _authUserDetails);

                // Assert

                act.Should().Throw<ArgumentNullException>().WithMessage("*Parameter name: stream*");
            }

            [Fact]
            public void When_File_Is_Empty_Then_ReturnError()
            {
                // Arrange

                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(null);
                var venueMock = VenueServiceMockFactory.GetVenueService(null);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);
                Stream stream = new MemoryStream();

                List<string> errors = new List<string>();
                // Act
                try
                {
                    errors = serviceUnderTest.ValidateAndUploadCSV(stream, _authUserDetails);
                }
                catch (Exception e)
                {

                    errors.Add(e.Message);

                }

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
                var venueMock = VenueServiceMockFactory.GetVenueService(null);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);
                Stream stream = CsvStreams.No_Header_Row();
                List<string> errors = new List<string>();
                // Act
                try
                {
                    errors = serviceUnderTest.ValidateAndUploadCSV(stream, _authUserDetails);
                }
                catch (Exception e)
                {

                    errors.Add($"Invalid header row. {e.Message.FirstSentence()}");

                }

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
                var venueMock = VenueServiceMockFactory.GetVenueService(null);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);
                Stream stream = CsvStreams.Only_Header_Row();

                List<string> errors = new List<string>();
                // Act
                try
                {
                    errors = serviceUnderTest.ValidateAndUploadCSV(stream, _authUserDetails);
                }
                catch (Exception e)
                {

                    errors.Add(e.Message);

                }

                // Assert

                errors.Should().NotBeNull();
                errors.Should().HaveCount(1);
                errors[0].Should().Be("The selected file is empty");
            }

           
        }

        public class ValidateCSVContents
        {
            private readonly AuthUserDetails _authUserDetails;

            public ValidateCSVContents()
            {
               _authUserDetails  = new AuthUserDetails(
            
                    userId: Guid.NewGuid(),
                    email : "email@testEmail.com",
                    nameOfUser : "Test User",
                    providerType : "Provider",
                    roleId : Guid.NewGuid(),
                    roleName : "Developer",
                    ukPrn : "12345678",
                    userName : "email@testEmail.com",
                    providerId : Guid.NewGuid()
                );
                
            }
            [Fact]
            public void When_Field_SUBREGION_Is_PresentAndIncorrectCase_Then_ReturnNoError()
            {
                // Arrange
                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(null);
                var venueMock = VenueServiceMockFactory.GetVenueService(null);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);

                Stream stream = CsvStreams.AppBUEmployer_Standard_ValidSubRegions();

                List<string> errors = new List<string>();
                // Act
                try
                {
                    errors = serviceUnderTest.ValidateAndUploadCSV(stream, _authUserDetails);
                }
                catch (Exception e)
                {

                    errors.Add(e.Message);

                }

                // Assert

                errors.Should().BeNullOrEmpty();
            }
            [Fact]
            public void When_Field_STANDARD_CODE_Is_PresentAndNonNumeric_Then_ReturnError()
            {
                // Arrange
                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(null);
                var venueMock = VenueServiceMockFactory.GetVenueService(null);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);

                Stream stream = CsvStreams.InvalidField_STANDARD_CODE_MustBeNumericIfPresent();

                List<string> errors = new List<string>();
                // Act
                try
                {
                    errors = serviceUnderTest.ValidateAndUploadCSV(stream, _authUserDetails);
                }
                catch (Exception e)
                {

                    errors.Add(e.Message);

                }

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
                var venueMock = VenueServiceMockFactory.GetVenueService(null);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);
                Stream stream = CsvStreams.InvalidField_STANDARD_VERSION_MustBeNumericIfPresent();

                List<string> errors = new List<string>();
                // Act
                try
                {
                    errors = serviceUnderTest.ValidateAndUploadCSV(stream, _authUserDetails);
                }
                catch (Exception e)
                {

                    errors.Add(e.Message);

                }

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
                var venueMock = VenueServiceMockFactory.GetVenueService(null);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);
                Stream stream = CsvStreams.InvalidField_FRAMEWORK_CODE_MustBeNumericIfPresent();

                List<string> errors = new List<string>();
                // Act
                try
                {
                    errors = serviceUnderTest.ValidateAndUploadCSV(stream, _authUserDetails);
                }
                catch (Exception e)
                {
                    
                    errors.Add(e.Message);
                    
                }
                

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
                var venueMock = VenueServiceMockFactory.GetVenueService(null);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);
                Stream stream = CsvStreams.InvalidField_FRAMEWORK_PROG_TYPE_MustBeNumericIfPresent();

                List<string> errors = new List<string>();
                // Act
                try
                {
                    errors = serviceUnderTest.ValidateAndUploadCSV(stream, _authUserDetails);
                }
                catch (Exception e)
                {

                    errors.Add(e.Message);

                }

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
                var venueMock = VenueServiceMockFactory.GetVenueService(null);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);
                Stream stream = CsvStreams.InvalidField_FRAMEWORK_PATHWAY_CODE_MustBeNumericIfPresent();

                List<string> errors = new List<string>();
                // Act
                try
                {
                    errors = serviceUnderTest.ValidateAndUploadCSV(stream, _authUserDetails);
                }
                catch (Exception e)
                {

                    errors.Add(e.Message);

                }

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
                var venueMock = VenueServiceMockFactory.GetVenueService(null);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);
                Stream stream = CsvStreams.InvalidRow_StandardAndFrameworkValuesMissing();

                List<string> errors = new List<string>();
                // Act
                try
                {
                    errors = serviceUnderTest.ValidateAndUploadCSV(stream, _authUserDetails);
                }
                catch (Exception e)
                {

                    errors.Add(e.Message);

                }

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
                var venueClient = HttpClientMockFactory.GetClient(SampleJsons.SuccessfulVenueFile(), HttpStatusCode.OK);
                var venueMock = VenueServiceMockFactory.GetVenueService(venueClient);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);
                Stream stream = CsvStreams.InvalidField_APPRENTICESHIP_INFORMATION_Missing();

                // Act

                var errors = serviceUnderTest.ValidateAndUploadCSV(stream, _authUserDetails);

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
                var venueClient = HttpClientMockFactory.GetClient(SampleJsons.SuccessfulVenueFile(), HttpStatusCode.OK);
                var venueMock = VenueServiceMockFactory.GetVenueService(venueClient);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);
                Stream stream = CsvStreams.InvalidField_APPRENTICESHIP_INFORMATION_751Chars();

                // Act

                var errors = serviceUnderTest.ValidateAndUploadCSV(stream, _authUserDetails);

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
                var venueClient = HttpClientMockFactory.GetClient(SampleJsons.SuccessfulVenueFile(), HttpStatusCode.OK);
                var venueMock = VenueServiceMockFactory.GetVenueService(venueClient);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);
                Stream stream = CsvStreams.InvalidField_APPRENTICESHIP_WEBPAGE_Regex_Error_Invalid_Character();

                // Act

                var errors = serviceUnderTest.ValidateAndUploadCSV(stream, _authUserDetails);

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
                var venueClient = HttpClientMockFactory.GetClient(SampleJsons.SuccessfulVenueFile(), HttpStatusCode.OK);
                var venueMock = VenueServiceMockFactory.GetVenueService(venueClient);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);
                Stream stream = CsvStreams.InvalidField_APPRENTICESHIP_WEBPAGE_256Chars();

                // Act

                var errors = serviceUnderTest.ValidateAndUploadCSV(stream, _authUserDetails);

                // Assert

                errors.Should().NotBeNull();
                errors.Should().HaveCount(2);
                errors[0].Should().Be("Validation error on row 2. Field APPRENTICESHIP_WEBPAGE format of URL is incorrect.");
                errors[1].Should().Be("Validation error on row 2. Field APPRENTICESHIP_WEBPAGE maximum length is 255 characters.");
            }
            [Fact]
            public void When_Field_APPRENTICESHIP_WEBPAGE_Is_Empty_Then_ReturnNoErrors()
            {
                // Arrange

                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(null);
                var venueClient = HttpClientMockFactory.GetClient(SampleJsons.SuccessfulVenueFile(), HttpStatusCode.OK);
                var venueMock = VenueServiceMockFactory.GetVenueService(venueClient);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);
                Stream stream = CsvStreams.Valid_Row_Empty_APPRENTICESHIP_WEBPAGE();

                // Act

                var errors = serviceUnderTest.ValidateAndUploadCSV(stream, _authUserDetails);

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
                var venueClient = HttpClientMockFactory.GetClient(SampleJsons.SuccessfulVenueFile(), HttpStatusCode.OK);
                var venueMock = VenueServiceMockFactory.GetVenueService(venueClient);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);
                Stream stream = CsvStreams.InvalidField_CONTACT_EMAIL_Missing();

                // Act

                var errors = serviceUnderTest.ValidateAndUploadCSV(stream, _authUserDetails);

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
                var venueClient = HttpClientMockFactory.GetClient(SampleJsons.SuccessfulVenueFile(), HttpStatusCode.OK);
                var venueMock = VenueServiceMockFactory.GetVenueService(venueClient);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);
                Stream stream = CsvStreams.InvalidField_CONTACT_EMAIL_256_Chars();

                // Act

                var errors = serviceUnderTest.ValidateAndUploadCSV(stream, _authUserDetails);

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
                var venueClient = HttpClientMockFactory.GetClient(SampleJsons.SuccessfulVenueFile(), HttpStatusCode.OK);
                var venueMock = VenueServiceMockFactory.GetVenueService(venueClient);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);
                Stream stream = CsvStreams.InvalidField_CONTACT_EMAIL_Regex_Invalid_character();

                // Act

                var errors = serviceUnderTest.ValidateAndUploadCSV(stream, _authUserDetails);

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
                var venueClient = HttpClientMockFactory.GetClient(SampleJsons.SuccessfulVenueFile(), HttpStatusCode.OK);
                var venueMock = VenueServiceMockFactory.GetVenueService(venueClient);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);
                Stream stream = CsvStreams.InvalidField_CONTACT_PHONE_Missing();

                // Act

                var errors = serviceUnderTest.ValidateAndUploadCSV(stream, _authUserDetails);

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
                var venueClient = HttpClientMockFactory.GetClient(SampleJsons.SuccessfulVenueFile(), HttpStatusCode.OK);
                var venueMock = VenueServiceMockFactory.GetVenueService(venueClient);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);
                Stream stream = CsvStreams.InvalidField_CONTACT_PHONE_Longer_Than_30_Chars();

                // Act

                var errors = serviceUnderTest.ValidateAndUploadCSV(stream, _authUserDetails);

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
                var venueClient = HttpClientMockFactory.GetClient(SampleJsons.SuccessfulVenueFile(), HttpStatusCode.OK);
                var venueMock = VenueServiceMockFactory.GetVenueService(venueClient);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);
                Stream stream = CsvStreams.InvalidField_CONTACT_PHONE_NonNumeric();

                // Act

                var errors = serviceUnderTest.ValidateAndUploadCSV(stream, _authUserDetails);

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
                var venueClient = HttpClientMockFactory.GetClient(SampleJsons.SuccessfulVenueFile(), HttpStatusCode.OK);
                var venueMock = VenueServiceMockFactory.GetVenueService(venueClient);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);
                Stream stream = CsvStreams.Valid_Row_Empty_CONTRACT_URL();

                // Act

                var errors = serviceUnderTest.ValidateAndUploadCSV(stream, _authUserDetails);

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
                var venueClient = HttpClientMockFactory.GetClient(SampleJsons.SuccessfulVenueFile(), HttpStatusCode.OK);
                var venueMock = VenueServiceMockFactory.GetVenueService(venueClient);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);
                Stream stream = CsvStreams.InvalidField_CONTACT_URL_256_Chars();

                // Act

                var errors = serviceUnderTest.ValidateAndUploadCSV(stream, _authUserDetails);

                // Assert

                errors.Should().NotBeNull();
                errors.Should().HaveCount(2);

                errors[0].Should().Be("Validation error on row 2. Field CONTACT_URL maximum length is 255 characters.");
                errors[1].Should().Be("Validation error on row 2. Field CONTACT_URL format of URL is incorrect.");
            }
            [Fact]
            public void When_Field_CONTACT_URL_Is_Contains_Space_Then_Return_Error()
            {
                // Arrange
                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(null);
                var venueClient = HttpClientMockFactory.GetClient(SampleJsons.SuccessfulVenueFile(), HttpStatusCode.OK);
                var venueMock = VenueServiceMockFactory.GetVenueService(venueClient);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);
                Stream stream = CsvStreams.InvalidField_CONTACT_URL_Invalid_URL_Space();

                // Act
                var errors = serviceUnderTest.ValidateAndUploadCSV(stream, _authUserDetails);

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
                var venueClient = HttpClientMockFactory.GetClient(SampleJsons.SuccessfulVenueFile(), HttpStatusCode.OK);
                var venueMock = VenueServiceMockFactory.GetVenueService(venueClient);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);
                Stream stream = CsvStreams.InvalidField_CONTACT_URL_Invalid_URL_Format();

                // Act
                var errors = serviceUnderTest.ValidateAndUploadCSV(stream, _authUserDetails);

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
                var venueMock = VenueServiceMockFactory.GetVenueService(null);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);
                Stream stream = CsvStreams.InvalidField_DELIVERY_METHOD_Missing();

                // Act

                var errors = serviceUnderTest.ValidateAndUploadCSV(stream, _authUserDetails);

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
                var venueMock = VenueServiceMockFactory.GetVenueService(null);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);
                Stream stream = CsvStreams.InvalidField_DELIVERY_METHOD_Invalid();

                // Act

                var errors = serviceUnderTest.ValidateAndUploadCSV(stream, _authUserDetails);

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
                var venueClient = HttpClientMockFactory.GetClient(SampleJsons.SuccessfulVenueFile(), HttpStatusCode.OK);
                var venueMock = VenueServiceMockFactory.GetVenueService(venueClient);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);
                Stream stream = CsvStreams.Valid_Row_DELIVERY_METHOD_Case_Insensitive_Correct_Values();

                // Act

                var errors = serviceUnderTest.ValidateAndUploadCSV(stream, _authUserDetails);

                // Assert

                errors.Should().BeNullOrEmpty();
                errors.Should().HaveCount(0);
            }
            [Fact]
            public void When_Field_VENUE_Is_Empty_And_DeliveryMode_IsEmployer_Return_Success()
            {
                // Arrange
                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(null);
                var venueMock = VenueServiceMockFactory.GetVenueService(null);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);
                Stream stream = CsvStreams.Employer_DELIVERY_METHOD_For_VENUE();

                // Act

                var errors = serviceUnderTest.ValidateAndUploadCSV(stream, _authUserDetails);

                // Assert

                errors.Should().BeNullOrEmpty();
                errors.Should().HaveCount(0);
            }
            [Fact]
            public void When_Field_VENUE_Is_Empty_And_Classroom_Or_Both_Is_Return_Error()
            {
                // Arrange
                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(null);
                var venueMock = VenueServiceMockFactory.GetVenueService(null);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);
                Stream stream = CsvStreams.Invalid_Row_No_VENUE_Valid_DELIVERY_MODE();

                // Act

                var errors = serviceUnderTest.ValidateAndUploadCSV(stream, _authUserDetails);

                // Assert
                errors.Should().NotBeNull();
                errors.Should().HaveCount(1);
                errors[0].Should().Be("Validation error on row 2. Field VENUE is required.");
            }
            [Fact]
            public void When_Field_VENUE_Is_Over_255Characters_Then_Return_Error()
            {
                // Arrange
                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(null);
                var venueClient = HttpClientMockFactory.GetClient(SampleJsons.SuccessfulVenueFile(), HttpStatusCode.OK);
                var venueMock = VenueServiceMockFactory.GetVenueService(venueClient);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);
                Stream stream = CsvStreams.Valid_Row_DELIVERY_METHOD_Case_Insensitive_Correct_Values();

                // Act

                var errors = serviceUnderTest.ValidateAndUploadCSV(stream, _authUserDetails);

                // Assert

                errors.Should().BeNullOrEmpty();
                errors.Should().HaveCount(0);
            }
            [Fact]
            public void When_Field_RADIUS_Contains_Invalid_Characters_Return_Error()
            {
                // Arrange
                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(null);
                var venueMock = VenueServiceMockFactory.GetVenueService(null);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);
                Stream stream = CsvStreams.InvalidField_RADIUS_MustBeNumericIfPresent();

                List<string> errors = new List<string>();
                // Act
                try
                {
                    errors = serviceUnderTest.ValidateAndUploadCSV(stream, _authUserDetails);
                }
                catch (Exception e)
                {

                    errors.Add(e.Message);

                }

                // Assert


                errors.Should().NotBeNull();
                errors.Should().HaveCount(1);
                errors[0].Should().Be("Validation error on row 2. Field RADIUS must be numeric if present.");
            }
            [Fact]
            public void When_Field_RADIUS_Is_Negative_Number_Return_Error()
            {
                // Arrange
                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(null);
                var venueClient = HttpClientMockFactory.GetClient(SampleJsons.SuccessfulVenueFile(), HttpStatusCode.OK);
                var venueMock = VenueServiceMockFactory.GetVenueService(venueClient);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);
                Stream stream = CsvStreams.InvalidField_RADIUS_NegativeNumber();

                // Act

                var errors = serviceUnderTest.ValidateAndUploadCSV(stream, _authUserDetails);

                // Assert


                errors.Should().NotBeNull();
                errors.Should().HaveCount(1);
                errors[0].Should().Be("Validation error on row 2. Field RADIUS must be a valid number");
            }            
            [Theory]
            [InlineData("157,1,,,,STANDARD APPRENTICESHIP,HTTP://WWW.TETS.CO.UK,TEST@TEST.COM,1213456789,HTTP://WWW.CONTACTUS.COM,EMPLOYER,,100,,NO,NO,,")]
            public void When_REGION_SUB_REGION_Are_Blank_Return_Error(params string[] csvLine)
            {
                // Arrange
                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(null);
                var venueClient = HttpClientMockFactory.GetClient(SampleJsons.SuccessfulVenueFile(), HttpStatusCode.OK);
                var venueMock = VenueServiceMockFactory.GetVenueService(venueClient);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);
                Stream stream = CsvStreams.StringArrayToStream(csvLine);

                // Act

                var errors = serviceUnderTest.ValidateAndUploadCSV(stream, _authUserDetails);

                // Assert


                errors.Should().NotBeNull();
                errors.Should().HaveCount(1);
                errors[0].Should().Be($"Validation error on row 2. Fields REGION/SUB_REGION are mandatory");
            }
            [Fact]
            public void When_Field_RADIUS_Is_Greater_Than_874_Return_Error()
            {
                // Arrange
                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(null);
                var venueClient = HttpClientMockFactory.GetClient(SampleJsons.SuccessfulVenueFile(), HttpStatusCode.OK);
                var venueMock = VenueServiceMockFactory.GetVenueService(venueClient);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);
                Stream stream = CsvStreams.InvalidField_RADIUS_875();

                // Act

                var errors = serviceUnderTest.ValidateAndUploadCSV(stream, _authUserDetails);

                // Assert


                errors.Should().NotBeNull();
                errors.Should().HaveCount(1);
                errors[0].Should().Be("Validation error on row 2. Field RADIUS must be between 1 and 874");
            }
            [Fact]
            public void When_ACROSS_ENGLAND_Is_False_Return_Radius_User_Value()
            {
                // Arrange
                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(null);
                var venueClient = HttpClientMockFactory.GetClient(SampleJsons.SuccessfulVenueFile(), HttpStatusCode.OK);
                var venueMock = VenueServiceMockFactory.GetVenueService(venueClient);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);
                Stream stream = CsvStreams.ValidRow_ACROSS_ENGLAND_FALSE();

                // Act

                var errors = serviceUnderTest.ValidateAndUploadCSV(stream, _authUserDetails);


                // Assert
                errors.Should().BeNullOrEmpty();
                errors.Should().HaveCount(0);
            }
            [Fact]
            public void When_ACROSS_ENGLAND_Is_Invalid_Return_Error()
            {
                // Arrange
                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(null);
                var venueClient = HttpClientMockFactory.GetClient(SampleJsons.SuccessfulVenueFile(), HttpStatusCode.OK);
                var venueMock = VenueServiceMockFactory.GetVenueService(venueClient);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);
                Stream stream = CsvStreams.InvalidRow_ACROSS_ENGLAND_Invalid();

                // Act

                var errors = serviceUnderTest.ValidateAndUploadCSV(stream, _authUserDetails);


                // Assert
                errors.Should().NotBeNull();
                errors.Should().HaveCount(1);
                errors[0].Should().Be("Validation error on row 2. Field ACROSS_ENGLAND must contain a value when Delivery Method is 'Both'");
            }
            [Fact]
            public void When_ACROSS_ENGLAND_Is_True_Return_Radius_600()
            {

                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(null);
                var venueClient = HttpClientMockFactory.GetClient(SampleJsons.SuccessfulVenueFile(), HttpStatusCode.OK);
                var venueMock = VenueServiceMockFactory.GetVenueService(venueClient);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);
           
                Stream stream = CsvStreams.ValidRow_ACROSS_ENGLAND_TRUE();

                // Act

                var errors = serviceUnderTest.ValidateAndUploadCSV(stream, _authUserDetails);


                // Assert
                errors.Should().BeNullOrEmpty();
                errors.Should().HaveCount(0);
            }
            [Fact]
            public void When_ACROSS_ENGLAND_Is_True_Return_No_Error()
            {

                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(null);
                var venueClient = HttpClientMockFactory.GetClient(SampleJsons.SuccessfulVenueFile(), HttpStatusCode.OK);
                var venueMock = VenueServiceMockFactory.GetVenueService(venueClient);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);

                Stream stream = CsvStreams.ValidRow_ACROSS_ENGLAND_Standard_TRUE();

                // Act

                var errors = serviceUnderTest.ValidateAndUploadCSV(stream, _authUserDetails);


                // Assert
                errors.Should().BeNullOrEmpty();
                errors.Should().HaveCount(0);
            }
            [Fact]
            public void When_NATIONAL_DELIVERY_Is_Invalid_Return_Error()
            {
                // Arrange
                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(null);
                var venueMock = VenueServiceMockFactory.GetVenueService(null);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);
                Stream stream = CsvStreams.InvalidRow_NATIONAL_DELIVERY_Invalid();

                // Act

                var errors = serviceUnderTest.ValidateAndUploadCSV(stream, _authUserDetails);

                // Assert
                errors.Should().NotBeNull();
                errors.Should().HaveCount(1);
                errors[0].Should().Be("Validation error on row 2. Field NATIONAL_DELIVERY must contain a value when Delivery Method is 'Employer'");
            }
            [Fact]
            public void When_DELIVERY_MODE_Is_Invalid_Return_Error()
            {
                // Arrange
                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var httpClient = HttpClientMockFactory.GetClient(SampleJsons.SuccessfulStandardFile(), HttpStatusCode.OK);
                var venueClient = HttpClientMockFactory.GetClient(SampleJsons.SuccessfulVenueFile(), HttpStatusCode.OK);
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(httpClient);

                var venueMock = VenueServiceMockFactory.GetVenueService(venueClient);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);
                Stream stream = CsvStreams.InvalidField_DELIVERY_MODE_Invalid_Option();

                // Act

                var errors = serviceUnderTest.ValidateAndUploadCSV(stream, _authUserDetails);

                // Assert
                errors.Should().NotBeNull();
                errors.Should().HaveCount(1);
                errors[0].Should().Be("Validation error on row 2. Field DELIVERY_MODE must be a valid Delivery Mode");
            }
            [Fact]
            public void When_DELIVERY_MODE_Has_Duplicates_Return_Error()
            {
                // Arrange
                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var httpClient = HttpClientMockFactory.GetClient(SampleJsons.SuccessfulStandardFile(), HttpStatusCode.OK);
                var venueClient = HttpClientMockFactory.GetClient(SampleJsons.SuccessfulVenueFile(), HttpStatusCode.OK);
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(httpClient);

                var venueMock = VenueServiceMockFactory.GetVenueService(venueClient);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);
                Stream stream = CsvStreams.InvalidField_DELIVERY_MODE_Duplicate_Option();

                // Act

                var errors = serviceUnderTest.ValidateAndUploadCSV(stream, _authUserDetails);

                // Assert
                errors.Should().NotBeNull();
                errors.Should().HaveCount(1);
                errors[0].Should().Be("Validation error on row 2. Field DELIVERY_MODE must contain unique Delivery Modes");
            }
            [Fact]
            public void When_REGION_Is_Invalid_Return_Error()
            {
                // Arrange
                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var httpClient = HttpClientMockFactory.GetClient(SampleJsons.SuccessfulStandardFile(), HttpStatusCode.OK);
                var venueClient = HttpClientMockFactory.GetClient(SampleJsons.SuccessfulVenueFile(), HttpStatusCode.OK);
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(httpClient);

                var venueMock = VenueServiceMockFactory.GetVenueService(venueClient);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);
                Stream stream = CsvStreams.InvalidRow_Invalid_REGION();

                // Act

                var errors = serviceUnderTest.ValidateAndUploadCSV(stream, _authUserDetails);

                // Assert
                errors.Should().NotBeNull();
                errors.Should().HaveCount(2);
                errors[0].Should().Be("Validation error on row 2. Field REGION contains invalid Region names");
            }
            [Fact]
            public void When_SUB_REGION_Is_Invalid_Return_Error()
            {
                // Arrange
                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var httpClient = HttpClientMockFactory.GetClient(SampleJsons.SuccessfulStandardFile(), HttpStatusCode.OK);
                var venueClient = HttpClientMockFactory.GetClient(SampleJsons.SuccessfulVenueFile(), HttpStatusCode.OK);
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(httpClient);

                var venueMock = VenueServiceMockFactory.GetVenueService(venueClient);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);
                Stream stream = CsvStreams.InvalidRow_Invalid_SUBREGION();

                // Act

                var errors = serviceUnderTest.ValidateAndUploadCSV(stream, _authUserDetails);

                // Assert
                errors.Should().NotBeNull();
                errors.Should().HaveCount(1);
                errors[0].Should().Be("Validation error on row 2. Field SUB_REGION contains invalid SubRegion names");
            }
            [Fact]
            public void When_REGION_AND_SUBREGION_Are_Valid_Return_Success()
            {
                // Arrange
                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var httpClient = HttpClientMockFactory.GetClient(SampleJsons.SuccessfulStandardFile(), HttpStatusCode.OK);
                var venueClient = HttpClientMockFactory.GetClient(SampleJsons.SuccessfulVenueFile(), HttpStatusCode.OK);
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(httpClient);

                var venueMock = VenueServiceMockFactory.GetVenueService(venueClient);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);
                Stream stream = CsvStreams.ValidRow_REGION_And_SUBREGION();

                // Act

                var errors = serviceUnderTest.ValidateAndUploadCSV(stream, _authUserDetails);;

                // Assert
                errors.Should().BeNullOrEmpty();
                errors.Should().HaveCount(0);
            }
            [Fact]
            public void When_VENUE_Is_Invalid_Return_Error()
            {
                // Arrange
                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var httpClient = HttpClientMockFactory.GetClient(SampleJsons.SuccessfulStandardFile(), HttpStatusCode.OK);
                
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(httpClient);
                var venueClient = HttpClientMockFactory.GetClient(SampleJsons.SuccessfulVenueFile(), HttpStatusCode.OK);
                var venueMock = VenueServiceMockFactory.GetVenueService(venueClient);

                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);
                Stream stream = CsvStreams.InvalidRow_Invalid_VENUE();

                // Act

                var errors = serviceUnderTest.ValidateAndUploadCSV(stream, _authUserDetails);

                // Assert
                errors.Should().NotBeNull();
                errors.Should().HaveCount(1);
                errors[0].Should().Be("Validation error on row 2. Field VENUE is invalid.");
            }
            [Fact]
            public void When_Multiple_VENUE_Returned_Return_Error()
            {
                // Arrange
                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var httpClient = HttpClientMockFactory.GetClient(SampleJsons.SuccessfulStandardFile(), HttpStatusCode.OK);
                var venueClient = HttpClientMockFactory.GetClient(SampleJsons.MultipleVenueFile(), HttpStatusCode.OK);
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(httpClient);

                var venueMock = VenueServiceMockFactory.GetVenueService(venueClient);

                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);
                Stream stream = CsvStreams.InvalidRow_Multiple_VENUE();

                // Act

                var errors = serviceUnderTest.ValidateAndUploadCSV(stream, _authUserDetails);

                // Assert
                errors.Should().NotBeNull();
                errors.Should().HaveCount(1);
                errors[0].Should().Be("Validation error on row 2. Field VENUE is invalid. Multiple venues identified with value entered.");
            }
        }

        public class CheckForDuplicatesTest
        {
            private readonly AuthUserDetails _authUserDetails;

            public CheckForDuplicatesTest()
            {
               _authUserDetails  = new AuthUserDetails(
            
                    userId: Guid.NewGuid(),
                    email : "email@testEmail.com",
                    nameOfUser : "Test User",
                    providerType : "Provider",
                    roleId : Guid.NewGuid(),
                    roleName : "Developer",
                    ukPrn : "12345678",
                    userName: "email@testEmail.com",
                    providerId: Guid.NewGuid()
                );
            }
            [Fact]
            public void When_Duplicate_StandardCodes_Exist_Return_Error()
            {
                // Arrange
                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var httpClient = HttpClientMockFactory.GetClient(SampleJsons.SuccessfulStandardFile(), HttpStatusCode.OK);
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(httpClient);
                var venueClient = HttpClientMockFactory.GetClient(SampleJsons.SuccessfulVenueFile(), HttpStatusCode.OK);
                var venueMock = VenueServiceMockFactory.GetVenueService(venueClient);

                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);
                Stream stream = CsvStreams.InvalidFile_Duplicate_STANDARD_CODES_SameDeliveryMethod_Same_Venue();

                List<string> errors = new List<string>();
                // Act
                try
                {
                    errors = serviceUnderTest.ValidateAndUploadCSV(stream, _authUserDetails);
                }
                catch (Exception e)
                {

                    errors.Add(e.Message);

                }

                // Assert
                errors.Should().NotBeNull();
                errors.Should().HaveCount(1);
                errors[0].Should().Be("Duplicate entries detected on rows 2, and 4.");
            }
            [Fact]
            public void When_Multiple_Duplicate_StandardCodes_Exist_Return_Error()
            {
                // Arrange
                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var httpClient = HttpClientMockFactory.GetClient(SampleJsons.SuccessfulStandardFile(), HttpStatusCode.OK);
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(httpClient);
                var venueClient = HttpClientMockFactory.GetClient(SampleJsons.SuccessfulVenueFile(), HttpStatusCode.OK);
                var venueMock = VenueServiceMockFactory.GetVenueService(venueClient);

                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);
                Stream stream = CsvStreams.InvalidFile_Multiple_Duplicate_STANDARD_CODES_SameDeliveryMethod_Same_Venue();

                List<string> errors = new List<string>();
                // Act
                try
                {
                    errors = serviceUnderTest.ValidateAndUploadCSV(stream, _authUserDetails);
                }
                catch (Exception e)
                {

                    errors.Add(e.Message);

                }

                // Assert
                errors.Should().NotBeNull();
                errors.Should().HaveCount(1);
                errors[0].Should().Be("Duplicate entries detected on rows 2, and 5.;Duplicate entries detected on rows 3, and 6.");
            }
            [Fact]
            public void When_Duplicate_FrameworkCodes_Exist_Return_Error()
            {
                // Arrange
                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var httpClient = HttpClientMockFactory.GetClient(SampleJsons.SuccessfulFrameworkFile(), HttpStatusCode.OK);
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(httpClient);

                var venueClient = HttpClientMockFactory.GetClient(SampleJsons.SuccessfulVenueFile(), HttpStatusCode.OK);
                var venueMock = VenueServiceMockFactory.GetVenueService(venueClient);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);
                Stream stream = CsvStreams.InvalidRow_FrameworkCodes_DuplicateRows();

                List<string> errors = new List<string>();
                // Act
                try
                {
                    errors = serviceUnderTest.ValidateAndUploadCSV(stream, _authUserDetails);
                }
                catch (Exception e)
                {

                    errors.Add(e.Message);

                }

                // Assert
                errors.Should().NotBeNull();
                errors.Should().HaveCount(1);
                errors[0].Should().Be("Duplicate entries detected on rows 2, and 4.");
            }
        }
        public class StandardsAndFrameworksTests
        {
            private readonly AuthUserDetails _authUserDetails;
            public StandardsAndFrameworksTests()
            {
               _authUserDetails  = new AuthUserDetails(
            
                    userId: Guid.NewGuid(),
                    email : "email@testEmail.com",
                    nameOfUser : "Test User",
                    providerType : "Provider",
                    roleId : Guid.NewGuid(),
                    roleName : "Developer",
                    ukPrn : "12345678",
                    userName: "email@testEmail.com",
                    providerId: Guid.NewGuid()
                );
            }
            [Fact]
            public void When_StandardCode_Is_Not_Valid_Return_Error()
            {
                // Arrange
                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var httpClient = HttpClientMockFactory.GetClient(SampleJsons.EmptyFile(), HttpStatusCode.NoContent);
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(httpClient);

                var venueMock = VenueServiceMockFactory.GetVenueService(null);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);
                Stream stream = CsvStreams.InvalidField_STANDARD_CODE_InvalidNumber();

                List<string> errors = new List<string>();
                // Act
                try
                {
                    errors = serviceUnderTest.ValidateAndUploadCSV(stream, _authUserDetails);
                }
                catch (Exception e)
                {

                    errors.Add(e.Message);

                }

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

                var venueClient = HttpClientMockFactory.GetClient(SampleJsons.SuccessfulVenueFile(), HttpStatusCode.OK);
                var venueMock = VenueServiceMockFactory.GetVenueService(venueClient);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);
                Stream stream = CsvStreams.ValidField_STANDARD_CODES();

                // Act

                var errors = serviceUnderTest.ValidateAndUploadCSV(stream, _authUserDetails);

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

                var venueClient = HttpClientMockFactory.GetClient(SampleJsons.SuccessfulVenueFile(), HttpStatusCode.OK);
                var venueMock = VenueServiceMockFactory.GetVenueService(venueClient);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);
                Stream stream = CsvStreams.InvalidField_STANDARD_CODE_InvalidNumber();

                List<string> errors = new List<string>();
                // Act
                try
                {
                    errors = serviceUnderTest.ValidateAndUploadCSV(stream, _authUserDetails);
                }
                catch (Exception e)
                {

                    errors.Add(e.Message);

                }

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

                var venueMock = VenueServiceMockFactory.GetVenueService(null);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);
                Stream stream = CsvStreams.InvalidField_FRAMEWORK_Values_Invalid();

                List<string> errors = new List<string>();
                // Act
                try
                {
                    errors = serviceUnderTest.ValidateAndUploadCSV(stream, _authUserDetails);
                }
                catch (Exception e)
                {

                    errors.Add(e.Message);

                }

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
                var venueClient = HttpClientMockFactory.GetClient(SampleJsons.SuccessfulVenueFile(), HttpStatusCode.OK);
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(httpClient);

                var venueMock = VenueServiceMockFactory.GetVenueService(venueClient);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);
                Stream stream = CsvStreams.ValidField_FrameworkCodes_CODES();

                // Act

                var errors = serviceUnderTest.ValidateAndUploadCSV(stream, _authUserDetails);

                // Assert
                errors.Should().BeNullOrEmpty();
                errors.Should().HaveCount(0);
            }
            [Fact]
            public void When_StandardCode_Version_DeliveryMethod_Are_Same_Are_Valid_Return_Success()
            {
                // Arrange
                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var httpClient = HttpClientMockFactory.GetClient(SampleJsons.SuccessfulStandardFile(), HttpStatusCode.OK);
                var venueClient = HttpClientMockFactory.GetClient(SampleJsons.SuccessfulVenueFile_Individual_Venues(), HttpStatusCode.OK);
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(httpClient);

                var venueMock = VenueServiceMockFactory.GetVenueService(venueClient);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);
                Stream stream = CsvStreams.ValidRow_StandardCode_Different_Venues();

                // Act

                var errors = serviceUnderTest.ValidateAndUploadCSV(stream, _authUserDetails); ;

                // Assert
                errors.Should().BeNullOrEmpty();
                errors.Should().HaveCount(0);
            }
            [Theory]
            //COUR-2096
            [
                InlineData(
                    ",,3,4,5,STANDARD APPRENTICESHIP,www.test.co.uk,test@tes.com,123456789012,www.contus.com,CLASSROOM,DUDLEY ,,BLOCK,,,,",
                    ",,3,4,5,STANDARD APPRENTICESHIP,www.test.co.uk,test@tes.com,123456789012,www.contus.com,CLASSROOM,DUDLEY 1,,DAY,,,,",
                    ",,3,4,5,STANDARD APPRENTICESHIP,www.test.co.uk,test@tes.com,123456789012,www.contus.com,CLASSROOM,DUDLEY 2,,DAY;BLOCK,,,,"
                ),
                //COUR-2094
                InlineData(",,3,4,5,STANDARD APPRENTICESHIP,http://www.tests.co.uk,test@testing.com,123456789012,http://www.tests.co.uk,CLASSROOM,DUDLEY ,,Day,,,,")
            ]
            public void Various_Frameworks_All_Should_Pass(params string[] csvLines)
            {
                // Arrange
                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var httpClient = HttpClientMockFactory.GetClient(SampleJsons.SuccessfulFrameworkFile(), HttpStatusCode.OK);
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(httpClient);

                var venueClient = HttpClientMockFactory.GetClient(SampleJsons.SuccessfulVenueFile(), HttpStatusCode.OK);
                var venueMock = VenueServiceMockFactory.GetVenueService(venueClient);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);
                Stream stream = CsvStreams.StringArrayToStream(csvLines);

                List<string> errors = new List<string>();
                // Act
                try
                {
                    errors = serviceUnderTest.ValidateAndUploadCSV(stream, _authUserDetails);
                }
                catch (Exception e)
                {

                    errors.Add(e.Message);

                }

                // Assert
                errors.Should().BeNullOrEmpty();

            }
            [Theory]
            //COUR-2094
            [
                InlineData("157,1,,,,STANDARD APPRENTICESHIP,http://www.tests.co.uk,TEST@TEST.COM,12345678901,http://www.tests.co.uk,CLASSROOM,DUDLEY,,DAY,,,,"),
                //COUR-2101
                InlineData(
                    "157,1,,,,STANDARD APPRENTICESHIP 1,HTTP://WWW.TETS.CO.UK,TEST@TEST.COM,12134567890,HTTP://WWW.CONTACTUS.COM,BOTH,DUDLEY 1,100,EMPLOYER;DAY,NO,,,",
                    "157,1,,,,STANDARD APPRENTICESHIP 2,HTTP://WWW.TETS.CO.UK,TEST@TEST.COM,12134567890,HTTP://WWW.CONTACTUS.COM,BOTH,DUDLEY 2,100,EMPLOYER;BLOCK,NO,,,",
                    "157,1,,,,STANDARD APPRENTICESHIP 3,HTTP://WWW.TETS.CO.UK,TEST@TEST.COM,12134567890,HTTP://WWW.CONTACTUS.COM,BOTH,DUDLEY 3,100,EMPLOYER;DAY;BLOCK,NO,,,"),

            ]
            public void Various_Standards_All_Should_Pass(params string[] csvLines)
            {
                // Arrange
                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipBulkUploadService>.Instance;
                var httpClient = HttpClientMockFactory.GetClient(SampleJsons.SuccessfulStandardFile(), HttpStatusCode.OK);
                var apprenticeMock = ApprenticeshipServiceMockFactory.GetApprenticeshipService(httpClient);

                var venueClient = HttpClientMockFactory.GetClient(SampleJsons.SuccessfulVenueFile(), HttpStatusCode.OK);
                var venueMock = VenueServiceMockFactory.GetVenueService(venueClient);
                var serviceUnderTest = new ApprenticeshipBulkUploadService(logger, apprenticeMock, venueMock);
                Stream stream = CsvStreams.StringArrayToStream(csvLines);

                List<string> errors = new List<string>();
                // Act
                try
                {
                    errors = serviceUnderTest.ValidateAndUploadCSV(stream, _authUserDetails);
                }
                catch (Exception e)
                {

                    errors.Add(e.Message);

                }

                // Assert
                errors.Should().BeNullOrEmpty();

            }
        }
    }
}
