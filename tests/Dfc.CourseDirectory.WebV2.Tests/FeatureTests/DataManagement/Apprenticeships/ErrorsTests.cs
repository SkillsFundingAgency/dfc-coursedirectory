using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Validation;
using Dfc.CourseDirectory.Testing;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.DataManagement.Apprenticeships
{
    public class ErrorsTests : MvcTestBase
    {
        public ErrorsTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        /// <summary>
        /// TODO: Uncomment published once PublishApprenticeshipUploadHandler has been created.
        /// </summary>
        /// <param name="uploadStatus"></param>
        /// <returns></returns>
        [Theory]
        [InlineData(null)]
        //[InlineData(UploadStatus.Published, Skip = "Test has been disabled until TestData.CreateApprenticeship can use PublishApprenticeshipUploadHandler")]
        [InlineData(UploadStatus.Abandoned)]
        public async Task Get_NoUnpublishedCourseUpload_ReturnsBadRequest(UploadStatus? uploadStatus)
        {
            // Arrange
            var provider = await TestData.CreateProvider(providerType: ProviderType.Apprenticeships);
            if (uploadStatus != null)
            {
                await TestData.CreateApprenticeshipUpload(provider.ProviderId, createdBy: User.ToUserInfo(), uploadStatus.Value, rb =>
                {
                    rb.AddValidRow();
                });
            }
            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload/apprenticeships/errors?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Get_ValidRequest_RendersExpectedContent()
        {
            // Arrange
            var provider = await TestData.CreateProvider(providerType: ProviderType.Apprenticeships);

            var learnAimRef = (await TestData.CreateLearningDelivery()).LearnAimRef;

            var (_, apprenticeshipUploadRows) = await TestData.CreateApprenticeshipUpload(
                           provider.ProviderId,
                           createdBy: User.ToUserInfo(),
                           UploadStatus.ProcessedWithErrors,
                           rowBuilder =>
                           {
                               for (int i = 0; i < 1; i++)
                               {
                                   rowBuilder.AddRow(1, 1, record =>
                                   {
                                       record.ApprenticeshipInformation = string.Empty;
                                       record.IsValid = false;
                                       record.Errors = new[]
                                       {
                                            ErrorRegistry.All["APPRENTICESHIP_INFORMATION_REQUIRED"].ErrorCode,
                                            ErrorRegistry.All["APPRENTICESHIP_EMAIL_FORMAT"].ErrorCode,
                                       };
                                   });
                               }
                           });

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload/apprenticeships/errors?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            using (new AssertionScope())
            {
                // Should only have one error row in the table
                var errorRows = doc.GetAllElementsByTestId("ApprenticeshipRow");

                doc.GetElementByTestId("ErrorCount").TextContent.Should().Be("2");

                errorRows.Count().Should().Be(1);

                var errors = errorRows.Single().GetElementByTestId("Errors").GetTrimmedTextContent();
                errors.Should().BeEquivalentTo(
                    Core.DataManagement.Errors.MapApprenticeshipErrorToFieldGroup("APPRENTICESHIP_INFORMATION_REQUIRED") +", "+
                    Core.DataManagement.Errors.MapApprenticeshipErrorToFieldGroup("APPRENTICESHIP_EMAIL_FORMAT")
                );

                doc.GetElementByTestId("ResolveOnScreenOption").Should().BeNull();
            }
        }

        [Fact]
        public async Task Get_DoesNotRender_ResolveOnScreenOption()
        {
            // Arrange
            var provider = await TestData.CreateProvider(providerType: ProviderType.Apprenticeships);

            var learnAimRef = (await TestData.CreateLearningDelivery()).LearnAimRef;

            var (_, apprenticeshipUploadRows) = await TestData.CreateApprenticeshipUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                rowBuilder =>
                {
                    for (int i = 0; i < 31; i++)
                    {
                        rowBuilder.AddRow(1,1, record =>
                        {
                            record.ApprenticeshipInformation = string.Empty;
                            record.IsValid = false;
                            record.Errors = new[]
                            {
                                ErrorRegistry.All["APPRENTICESHIP_INFORMATION_REQUIRED"].ErrorCode,
                            };
                        });
                    }
                });

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload/apprenticeships/errors?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            doc.GetElementByTestId("ResolveOnScreenOption").Should().BeNull();
        }

        [Fact]
        public async Task Post_MissingWhatNext_RendersError()
        {
            // Arrange
            var provider = await TestData.CreateProvider(providerType: ProviderType.Apprenticeships);
            await TestData.CreateApprenticeshipUpload(provider.ProviderId, createdBy: User.ToUserInfo(), UploadStatus.ProcessedWithErrors, rb =>
            {
                rb.AddValidRow();
            });

            var request = new HttpRequestMessage(HttpMethod.Post, $"/data-upload/apprenticeships/errors?providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var doc = await response.GetDocument();
            doc.AssertHasError("WhatNext", "Select what you want to do");
        }


        [Theory]
        [InlineData("UploadNewFile", "/data-upload/apprenticeships?providerId={0}")]
        [InlineData("DeleteUpload", "/data-upload/apprenticeships/delete?providerId={0}")]
        public async Task Post_ValidRequest_ReturnsRedirect(string selectedOption, string expectedLocation)
        {
            // Arrange
            var provider = await TestData.CreateProvider(providerType: ProviderType.Apprenticeships);

            await TestData.CreateApprenticeshipUpload(provider.ProviderId, createdBy: User.ToUserInfo(), UploadStatus.ProcessedWithErrors, rb =>
            {
                rb.AddValidRow();
            });

            var request = new HttpRequestMessage(HttpMethod.Post, $"/data-upload/apprenticeships/errors?providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("WhatNext", selectedOption)
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.OriginalString.Should().Be(string.Format(expectedLocation, provider.ProviderId));
        }
    }
}
