using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using Dfc.CourseDirectory.Services.ApprenticeshipBulkUploadService;
using Dfc.CourseDirectory.Services.ApprenticeshipService;
using Dfc.CourseDirectory.Services.BlobStorageService;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Models.Auth;
using Dfc.CourseDirectory.Services.ProviderService;
using Dfc.CourseDirectory.Web.Controllers;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.ViewModels.BulkUpload;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        public async Task UploadPost_ValidFileSupplied()
        {
            SetupValidFileMetadata();
            SetupFileData("a,small,csv,file,here");
            SetupUploadService_Success();

            var result = await _bulkUploadApprenticeshipsController.Upload(_mockFormFile.Object);

            AssertRedirect(result, "BulkUploadApprenticeships", "PublishYourFile");
            VerifyNotUploaded();
        }

        [Fact]
        public async Task UploadPost_NoProviderSelected()
        {
            UnSetProvider();
            var result = await _bulkUploadApprenticeshipsController.Upload(_mockFormFile.Object);
            AssertRedirect(result, "Home", "Index");
            VerifyNotUploaded();
        }

        [Fact]
        public async Task UploadPost_NoFileSupplied()
        {
            _mockFormFile.Setup(m => m.Length).Returns(0);
            var result = await _bulkUploadApprenticeshipsController.Upload(_mockFormFile.Object);
            AssertError("No file uploaded", result);
            VerifyNotUploaded();
        }

        [Fact]
        public async Task UploadPost_BadData_SemicolonSeparated()
        {
            // Characterization test for the handling of the BadDataException thrown when duplicates encountered,
            // which has a semicolon-separated error message:
            // https://github.com/SkillsFundingAgency/dfc-coursedirectory/blob/2cbb7d4056257a5f6f1ef4147d5fe2e8ff39db42/src/Dfc.CourseDirectory.Services/BulkUploadService/ApprenticeshipBulkUploadService.cs#L1148

            SetupValidFileMetadata();
            SetupFileData("Y");
            SetupUploadService_ThrowsBadData("oh;my;goodness");

            var result = await _bulkUploadApprenticeshipsController.Upload(_mockFormFile.Object);

            AssertErrors(new List<string> {"oh", "my", "goodness"}, result);
            VerifyNotUploaded();
        }

        [Fact]
        public async Task UploadPost_HeaderException()
        {
            SetupValidFileMetadata();
            SetupFileData("Z");
            SetupUploadService_ThrowsHeaderException("Header error message. Subsequent sentence that is stripped out.");

            var result = await _bulkUploadApprenticeshipsController.Upload(_mockFormFile.Object);

            AssertError("Header error message.", result);
            VerifyNotUploaded();
        }

        [Fact]
        public async Task UploadPost_Exception()
        {
            const string message = "This; is ... an intact message"; // semicolons and dots to check split code isn't being hit
            SetupValidFileMetadata();
            SetupFileData("F");
            SetupUploadService_ThrowsException(message);

            var result = await _bulkUploadApprenticeshipsController.Upload(_mockFormFile.Object);

            AssertError(message, result);
            VerifyNotUploaded();
        }

        [Fact]
        public async Task UploadPost_InvalidFileSupplied()
        {
            SetupValidFileMetadata();
            SetupFileData(new byte[] {0x1});

            var result = await _bulkUploadApprenticeshipsController.Upload(_mockFormFile.Object);

            AssertError("Invalid file content.", result);
            VerifyNotUploaded();
        }

        [Fact]
        public async Task UploadPost_Errors()
        {
            SetupValidFileMetadata();
            SetupFileData("Y");
            var errorsToReturn = new List<string>{"alpha", "tango"};
            SetupUploadService_Errors(errorsToReturn);

            var result = await _bulkUploadApprenticeshipsController.Upload(_mockFormFile.Object);

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
                _mockApprenticeshipBulkUploadService.Object,
                new Mock<IApprenticeshipService>().Object,
                _mockBlobStorageService.Object,
                new Mock<ICourseService>().Object,
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

        private void SetupUploadService_Success(bool processedSynchronously = true)
        {
            var errors = new List<string>();
            _mockApprenticeshipBulkUploadService.Setup(
                    m => m.ValidateAndUploadCSV(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<AuthUserDetails>()))
                .ReturnsAsync(ApprenticeshipBulkUploadResult.Success(processedSynchronously));
        }

        private void SetupUploadService_Errors(List<string> errors)
        {
            _mockApprenticeshipBulkUploadService.Setup(
                    m => m.ValidateAndUploadCSV(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<AuthUserDetails>()))
                .ReturnsAsync(ApprenticeshipBulkUploadResult.Failed(errors));
        }

        private void SetupUploadService_ThrowsBadData(string message)
        {
            _mockApprenticeshipBulkUploadService.Setup(
                    m => m.ValidateAndUploadCSV(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<AuthUserDetails>()))
                .ThrowsAsync(new BadDataException(null, message));
        }

        private void SetupUploadService_ThrowsHeaderException(string message)
        {
            _mockApprenticeshipBulkUploadService.Setup(
                    m => m.ValidateAndUploadCSV(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<AuthUserDetails>()))
                .ThrowsAsync(new HeaderValidationException(null, null, null, message));
        }

        private void SetupUploadService_ThrowsException(string message)
        {
            _mockApprenticeshipBulkUploadService.Setup(
                    m => m.ValidateAndUploadCSV(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<AuthUserDetails>()))
                .ThrowsAsync(new Exception(message));
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
