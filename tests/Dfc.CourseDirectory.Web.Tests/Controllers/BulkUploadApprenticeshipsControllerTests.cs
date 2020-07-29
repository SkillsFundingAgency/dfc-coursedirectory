using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using Dfc.CourseDirectory.Models.Models.Auth;
using Dfc.CourseDirectory.Services.Interfaces.ApprenticeshipService;
using Dfc.CourseDirectory.Services.Interfaces.BlobStorageService;
using Dfc.CourseDirectory.Services.Interfaces.BulkUploadService;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.ProviderService;
using Dfc.CourseDirectory.Web.Controllers;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.ViewModels.BulkUpload;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Dfc.CourseDirectory.Web.Tests.Controllers
{
    /// <summary>
    /// Characterization tests for the apprenticeships bulk upload controller behaviour in preparation
    /// for refactoring and modifying the behaviour.
    /// </summary>
    public class BulkUploadApprenticeshipsControllerTests
    {
        private const int ProviderUKPRN = 666;
        private readonly BulkUploadApprenticeshipsController _bulkUploadApprenticeshipsController;
        private readonly ISession _fakeSession = new FakeSession();
        private readonly Mock<IApprenticeshipBulkUploadService> _mockApprenticeshipBulkUploadService = new Mock<IApprenticeshipBulkUploadService>();
        private readonly Mock<IBlobStorageService> _mockBlobStorageService = new Mock<IBlobStorageService>();
        private readonly Mock<IFormFile> _mockFormFile = new Mock<IFormFile>();

        public BulkUploadApprenticeshipsControllerTests()
        {
            _bulkUploadApprenticeshipsController = BuildController();
            SetProvider();
        }

        [Fact]
        public async Task IndexPost_ValidFileSupplied()
        {
            SetupValidFileMetadata();
            SetupFileData("a,small,csv,file,here");
            SetupUploadService_Success();

            var result = await _bulkUploadApprenticeshipsController.Index(_mockFormFile.Object);

            AssertRedirect(result, "BulkUploadApprenticeships", "PublishYourFile");
            VerifyNotUploaded();
        }

        [Fact]
        public async Task IndexPost_ValidLargeFileSupplied()
        {
            // arrange
            SetupValidFileMetadata();
            const string csvData = "a,large,csv,file,here";
            SetupFileData(csvData);
            SetupUploadService_Success();
            SetupBackgroundProcessingTriggers();
            bool streamPassedThrough = false;
            var filePathPattern = $"^{ProviderUKPRN}" +
                                  @"\/Apprenticeship Bulk Upload\/Files\/[0-9]{6}-[0-9]{6}-bulkUploadFile.csv$";

            _mockBlobStorageService.Setup(
                    m => m.UploadFileAsync(It.IsRegex(filePathPattern), It.IsAny<Stream>()))
                .Returns(Task.CompletedTask)
                .Callback(
                    (string _, Stream stream) => streamPassedThrough = StreamContainsOriginalData(stream, csvData));

            // act
            var result = await _bulkUploadApprenticeshipsController.Index(_mockFormFile.Object);

            // assert
            AssertRedirect(result, null, "Pending");

            _mockBlobStorageService.Verify(
                m => m.UploadFileAsync(It.IsRegex(filePathPattern), It.IsAny<Stream>()),
                Times.Once);

            Assert.True(streamPassedThrough);
        }

        [Fact]
        public async Task IndexPost_NoProviderSelected()
        {
            UnSetProvider();
            var result = await _bulkUploadApprenticeshipsController.Index(_mockFormFile.Object);
            AssertRedirect(result, "Home", "Index");
            VerifyNotUploaded();
        }

        [Fact]
        public async Task IndexPost_NoFileSupplied()
        {
            _mockFormFile.Setup(m => m.Length).Returns(0);
            var result = await _bulkUploadApprenticeshipsController.Index(_mockFormFile.Object);
            AssertError("No file uploaded", result);
            VerifyNotUploaded();
        }

        [Fact]
        public async Task IndexPost_BadData_SemicolonSeparated()
        {
            // Characterization test for the handling of the BadDataException thrown when duplicates encountered,
            // which has a semicolon-separated error message:
            // https://github.com/SkillsFundingAgency/dfc-coursedirectory/blob/2cbb7d4056257a5f6f1ef4147d5fe2e8ff39db42/src/Dfc.CourseDirectory.Services/BulkUploadService/ApprenticeshipBulkUploadService.cs#L1148

            SetupValidFileMetadata();
            SetupFileData("Y");
            SetupUploadService_ThrowsBadData("oh;my;goodness");

            var result = await _bulkUploadApprenticeshipsController.Index(_mockFormFile.Object);

            AssertErrors(new List<string> {"oh", "my", "goodness"}, result);
            VerifyNotUploaded();
        }

        [Fact]
        public async Task IndexPost_HeaderException()
        {
            SetupValidFileMetadata();
            SetupFileData("Z");
            SetupUploadService_ThrowsHeaderException("Header error message. Subsequent sentence that is stripped out.");

            var result = await _bulkUploadApprenticeshipsController.Index(_mockFormFile.Object);

            AssertError("Header error message.", result);
            VerifyNotUploaded();
        }

        [Fact]
        public async Task IndexPost_Exception()
        {
            const string message = "This; is ... an intact message"; // semicolons and dots to check split code isn't being hit
            SetupValidFileMetadata();
            SetupFileData("F");
            SetupUploadService_ThrowsException(message);

            var result = await _bulkUploadApprenticeshipsController.Index(_mockFormFile.Object);

            AssertError(message, result);
            VerifyNotUploaded();
        }

        [Fact]
        public async Task IndexPost_InvalidFileSupplied()
        {
            SetupValidFileMetadata();
            SetupFileData(new byte[] {0x1});

            var result = await _bulkUploadApprenticeshipsController.Index(_mockFormFile.Object);

            AssertError("Invalid file content.", result);
            VerifyNotUploaded();
        }

        [Fact]
        public async Task IndexPost_Errors()
        {
            SetupValidFileMetadata();
            SetupFileData("Y");
            var errorsToReturn = new List<string>{"alpha", "tango"};
            SetupUploadService_Errors(errorsToReturn);

            var result = await _bulkUploadApprenticeshipsController.Index(_mockFormFile.Object);

            var redirect = AssertRedirect(result, "BulkUploadApprenticeships", "WhatDoYouWantToDoNext");
            Assert.Equal(errorsToReturn.Count, redirect.RouteValues["errorCount"]);
            const string expectedErrorMessage = "Your file contained 2 errors. You must resolve all errors before your apprenticeship training information can be published.";
            Assert.Equal(expectedErrorMessage,redirect.RouteValues["message"]);
            VerifyNotUploaded();
        }

        private static bool StreamContainsOriginalData(Stream stream, string originalData)
        {
            var reader = new StreamReader(stream);
            var actualCsv = reader.ReadToEnd();
            return originalData == actualCsv;
        }

        private BulkUploadApprenticeshipsController BuildController()
        {
            var mockUser = new Mock<ClaimsPrincipal>();
            var mockContext = new DefaultHttpContext
            {
                Session = _fakeSession,
                User = mockUser.Object,
            };

            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            mockHttpContextAccessor.SetupGet(m => m.HttpContext).Returns(mockContext);

            var bulkUploadApprenticeshipsController = new BulkUploadApprenticeshipsController(
                NullLogger<BulkUploadApprenticeshipsController>.Instance,
                mockHttpContextAccessor.Object,
                _mockApprenticeshipBulkUploadService.Object,
                new Mock<IApprenticeshipService>().Object,
                _mockBlobStorageService.Object,
                new Mock<ICourseService>().Object,
                new Mock<IHostingEnvironment>().Object,
                new Mock<IProviderService>().Object,
                new Mock<IUserHelper>().Object);
            bulkUploadApprenticeshipsController.ControllerContext.HttpContext = mockContext;
            return bulkUploadApprenticeshipsController;
        }

        private void SetProvider()
        {
            _fakeSession.SetInt32("UKPRN", ProviderUKPRN);
        }

        private void UnSetProvider()
        {
            _fakeSession.Remove("UKPRN");
        }

        private void SetupValidFileMetadata()
        {
            _mockFormFile.Setup(m => m.Length).Returns(1);
            _mockFormFile.Setup(m => m.FileName).Returns("bulkUploadFile.csv");
            _mockFormFile.Setup(m => m.Name).Returns("bulkUploadFile");
            _mockFormFile.Setup(m => m.ContentDisposition).Returns("a string with filename in it somewhere");
        }

        private void SetupFileData(string utf8FileData)
        {
            SetupFileData(Encoding.UTF8.GetBytes(utf8FileData));
        }

        private void SetupFileData(byte[] binaryData)
        {
            _mockFormFile.Setup(m => m.CopyTo(It.IsAny<Stream>()))
                .Callback((Stream stream) =>
                {
                    using var binaryStream = new MemoryStream(binaryData);
                    binaryStream.CopyTo(stream);
                });
        }

        private void SetupUploadService_Success()
        {
            var errors = new List<string>();
            _mockApprenticeshipBulkUploadService.Setup(
                    m => m.ValidateAndUploadCSV(It.IsAny<Stream>(), It.IsAny<AuthUserDetails>(), It.IsAny<bool>()))
                .ReturnsAsync(errors);
        }

        private void SetupUploadService_Errors(List<string> errors)
        {
            _mockApprenticeshipBulkUploadService.Setup(
                    m => m.ValidateAndUploadCSV(It.IsAny<Stream>(), It.IsAny<AuthUserDetails>(), It.IsAny<bool>()))
                .ReturnsAsync(errors);
        }

        private void SetupUploadService_ThrowsBadData(string message)
        {
            _mockApprenticeshipBulkUploadService.Setup(
                    m => m.ValidateAndUploadCSV(It.IsAny<Stream>(), It.IsAny<AuthUserDetails>(), It.IsAny<bool>()))
                .ThrowsAsync(new BadDataException(null, message));
        }

        private void SetupUploadService_ThrowsHeaderException(string message)
        {
            _mockApprenticeshipBulkUploadService.Setup(
                    m => m.ValidateAndUploadCSV(It.IsAny<Stream>(), It.IsAny<AuthUserDetails>(), It.IsAny<bool>()))
                .ThrowsAsync(new HeaderValidationException(null, null, null, message));
        }

        private void SetupUploadService_ThrowsException(string message)
        {
            _mockApprenticeshipBulkUploadService.Setup(
                    m => m.ValidateAndUploadCSV(It.IsAny<Stream>(), It.IsAny<AuthUserDetails>(), It.IsAny<bool>()))
                .ThrowsAsync(new Exception(message));
        }

        /// <summary>
        /// Configure mocks to trigger the `processInline` flag to end up as `false`
        /// </summary>
        private void SetupBackgroundProcessingTriggers()
        {
            _mockApprenticeshipBulkUploadService.Setup(m => m.CountCsvLines(It.IsAny<Stream>())).Returns(1000);
            _mockBlobStorageService.Setup(m => m.InlineProcessingThreshold).Returns(200);
        }

        private static void AssertError(string expectedError, IActionResult result)
        {
            AssertErrors(new List<string> {expectedError}, result);
        }

        private static void AssertErrors(IList<string> expectedErrors, IActionResult result)
        {
            var bulkUploadViewModel = ViewModelFrom(result);
            Assert.NotNull(bulkUploadViewModel.errors);
            var errors = bulkUploadViewModel.errors.ToList();
            Assert.Equal(expectedErrors, errors);
        }

        private static RedirectToActionResult AssertRedirect(IActionResult result, string expectedController, string expectedAction)
        {
            var redirect = Assert.IsAssignableFrom<RedirectToActionResult>(result);
            Assert.Equal(expectedController, redirect.ControllerName);
            Assert.Equal(expectedAction, redirect.ActionName);
            return redirect;
        }

        private void VerifyNotUploaded()
        {
            _mockBlobStorageService.Verify(
                m => m.UploadFileAsync(It.IsAny<string>(), It.IsAny<Stream>()),
                Times.Never);
        }

        private static BulkUploadViewModel ViewModelFrom(IActionResult result)
        {
            var viewResult = Assert.IsAssignableFrom<ViewResult>(result);
            var model = viewResult.Model;
            var bulkUploadViewModel = Assert.IsAssignableFrom<BulkUploadViewModel>(model);
            return bulkUploadViewModel;
        }
    }
}
