using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing;
using Dfc.CourseDirectory.WebV2.Features.Apprenticeships.DeleteApprenticeship;
using FluentAssertions;
using FluentAssertions.Execution;
using FormFlow;
using OneOf;
using OneOf.Types;
using Xunit;
using SqlQueries = Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.Apprenticeships.DeleteApprenticeship
{
    public class DeleteApprenticeshipTests : MvcTestBase
    {
        public DeleteApprenticeshipTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task Get_ApprenticeshipDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var ApprenticeshipId = Guid.NewGuid();

            var request = new HttpRequestMessage(HttpMethod.Get, $"/apprenticeships/delete/{ApprenticeshipId}");                                                                          
                                                                                            
            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        
        /*
        [Theory]
        [InlineData(TestUserType.ProviderSuperUser)]
        [InlineData(TestUserType.ProviderUser)]
        public async Task Get_UserCannotAccessApprenticeship_ReturnsForbidden(TestUserType userType)
        {
        // Arrange
        var anotherProvider = await TestData.CreateProvider(
            providerType: ProviderType.Apprenticeships);

        var provider = await TestData.CreateProvider(
            providerType: ProviderType.Apprenticeships);

        var apprenticeshipTitle = await TestData.CreateStandard(standardName: "");

        var apprenticeship = await TestData.CreateApprenticeship(
            provider.ProviderId,
            apprenticeshipTitle,
            createdBy: User.ToUserInfo());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/apprenticeships/delete/{apprenticeship.ApprenticeshipId}");

        await User.AsTestUser(userType, anotherProvider.ProviderId);

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
        */

        [Fact]
        public async Task Get_ValidRequest_RendersExpectedOutput()
        {
            // Arrange
            var apprenticeshipTitle = await TestData.CreateStandard(standardName: "");

            var provider = await TestData.CreateProvider(
                providerType: ProviderType.Apprenticeships);

            var apprenticeship = await TestData.CreateApprenticeship(
                provider.ProviderId,
                apprenticeshipTitle,
                createdBy: User.ToUserInfo());

            var request = new HttpRequestMessage(HttpMethod.Get, $"/apprenticeships/delete/{apprenticeship.ApprenticeshipId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        } 
        
        [Fact]
        public async Task Post_CourseDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var apprenticeshipId = Guid.NewGuid();

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("Confirm", "true")
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"/apprenticeships/delete/{apprenticeshipId}")
            {
                Content = requestContent
            };

            CreateJourneyInstance(apprenticeshipId);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        } 
        
        /*
        [Theory]
        [InlineData(TestUserType.ProviderSuperUser)]
        [InlineData(TestUserType.ProviderUser)]
        public async Task Post_UserCannotAccessCourse_ReturnsForbidden(TestUserType userType)
        {
            // Arrange
            var anotherProvider = await TestData.CreateProvider(
                providerType: ProviderType.Apprenticeships);

            var provider = await TestData.CreateProvider(
                providerType: ProviderType.Apprenticeships);

            var apprenticeshipTitle = await TestData.CreateStandard(standardName:"");

            var ApprenticeshId = Guid.NewGuid();

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("Confirm", "true")
                .ToContent();

            var apprenticeship = await TestData.CreateApprenticeship(
                provider.ProviderId,
                apprenticeshipTitle,
                createdBy: User.ToUserInfo());

            var request = new HttpRequestMessage(HttpMethod.Post, $"/apprenticeships/delete/{ApprenticeshId}")
            {
                Content = requestContent
            };

            CreateJourneyInstance(apprenticeship.ApprenticeshipId);

            await User.AsTestUser(userType, anotherProvider.ProviderId);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        } 
        /*

        [Fact]
        public async Task Post_NotConfirmed_ReturnsError()
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                providerType: ProviderType.Apprenticeships);

            var apprenticeshipTitle = await TestData.CreateStandard(standardName: "");

            var apprenticeship = await TestData.CreateApprenticeship(
                provider.ProviderId,
                apprenticeshipTitle,
                createdBy: User.ToUserInfo());

            var requestContent = new FormUrlEncodedContentBuilder()
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"/apprenticeships/delete/{apprenticeship.ApprenticeshipId}")
            {
                Content = requestContent
            };

            CreateJourneyInstance(apprenticeship.ApprenticeshipId);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            var doc = await response.GetDocument();
            doc.AssertHasError("Confirm", "Confirm you want to delete the Apprenticeship"); 
        }
        
        /*
        [Fact]
        public async Task Post_ValidRequest_DeletesApprenticeshipAndRedirects()
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                providerType: ProviderType.Apprenticeships);

            var apprenticeshipTitle = await TestData.CreateStandard(standardName: "");

            var apprenticeship = await TestData.CreateApprenticeship(
                provider.ProviderId,
                apprenticeshipTitle,
                createdBy: User.ToUserInfo());

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("Confirm", "true")
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"/apprenticeships/delete/{apprenticeship.ApprenticeshipId}")
            {
                Content = requestContent
            };

            CreateJourneyInstance(apprenticeship.ApprenticeshipId);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Found);

            response.Headers.Location.OriginalString.Should()
                .Be($"/apprenticeships/delete/{apprenticeship.ApprenticeshipId}/success?providerId={provider.ProviderId}");

            SqlQuerySpy.VerifyQuery<SqlQueries.DeleteApprenticeship, OneOf<NotFound, Success>>(q => q.ApprenticeshipId == apprenticeship.ApprenticeshipId);
        }
        */

        [Fact]
        public async Task GetDeleted_RendersExpectedApprenticeshipName()
        {
            // Arrange
            var anotherProvider = await TestData.CreateProvider(
                providerType: ProviderType.Apprenticeships);

            var provider = await TestData.CreateProvider(
                providerType: ProviderType.Apprenticeships);

            var apprenticeshipTitle = await TestData.CreateStandard(standardName: "");

            var ApprenticeshId = Guid.NewGuid();

            var apprenticeship = await TestData.CreateApprenticeship(
                provider.ProviderId,
                apprenticeshipTitle,
                createdBy: User.ToUserInfo());


            await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(
                new SqlQueries.DeleteApprenticeship()
                {
                    ApprenticeshipId = apprenticeship.ApprenticeshipId,
                    DeletedOn = Clock.UtcNow,
                    DeletedBy = User.ToUserInfo()
                }));

            var request = new HttpRequestMessage(
                HttpMethod.Get,
               $"/apprenticeships/delete/{apprenticeship.ApprenticeshipId}/success?providerId={provider.ProviderId}");

            await User.AsProviderUser(provider.ProviderId, ProviderType.FE);

            var journeyInstance = CreateJourneyInstance(
                apprenticeship.ApprenticeshipId,
                new JourneyModel()
                {
                    ApprenticeshipTitle = apprenticeship.Standard.StandardName,
                    ProviderId = provider.ProviderId,
                    NotionalNVQLevelv2 = apprenticeship.Standard.NotionalNVQLevelv2
                });

            journeyInstance.Complete();

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();
            doc.GetElementByTestId("ApprenticeshipTitle").TextContent.Should().Be(apprenticeship.Standard.StandardName);
        }

        [Fact]
        public async Task GetDeleted_NoOtherLiveApprenticeships_DoesNotRenderViewEditLink()
        {
            // Arrange
            var anotherProvider = await TestData.CreateProvider(
                 providerType: ProviderType.Apprenticeships);

            var provider = await TestData.CreateProvider(
                providerType: ProviderType.Apprenticeships);

            var apprenticeshipTitle = await TestData.CreateStandard(standardName: "");

            var ApprenticeshId = Guid.NewGuid();

            var apprenticeship = await TestData.CreateApprenticeship(
                provider.ProviderId,
                apprenticeshipTitle,
                createdBy: User.ToUserInfo());

            await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(
                new SqlQueries.DeleteApprenticeship()
                {
                    ApprenticeshipId = apprenticeship.ApprenticeshipId,
                    DeletedOn = Clock.UtcNow,
                    DeletedBy = User.ToUserInfo()
                }));

            var request = new HttpRequestMessage(
                HttpMethod.Get,
               $"/apprenticeships/delete/{apprenticeship.ApprenticeshipId}/success?providerId={provider.ProviderId}");

            await User.AsProviderUser(provider.ProviderId, ProviderType.FE);

            var journeyInstance = CreateJourneyInstance(
                apprenticeship.ApprenticeshipId,
                new JourneyModel()
                {
                    ApprenticeshipTitle = apprenticeship.Standard.StandardName,
                    ProviderId = provider.ProviderId,
                    NotionalNVQLevelv2 = apprenticeship.Standard.NotionalNVQLevelv2
                });

            journeyInstance.Complete();

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();
            doc.GetElementByTestId("ApprenticeshipProviderLink").Should().BeNull();
        }

        [Fact]
        public async Task GetDeleted_HasOtherLiveCourseRuns_DoesRenderViewEditCopyLink()
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                providerType: ProviderType.Apprenticeships);

            var apprenticeshipTitle = await TestData.CreateStandard(standardName: "");

            var apprenticeship = await TestData.CreateApprenticeship(
                provider.ProviderId,
                apprenticeshipTitle,
                createdBy: User.ToUserInfo());

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/apprenticeships/delete/{apprenticeship.ApprenticeshipId}/success?providerId={provider.ProviderId}");

            await User.AsProviderUser(provider.ProviderId, ProviderType.FE);

            var journeyInstance = CreateJourneyInstance(
                apprenticeship.ApprenticeshipId,
                new JourneyModel()
                {
                    ApprenticeshipTitle = apprenticeship.Standard.StandardName,
                    ProviderId = provider.ProviderId,
                    NotionalNVQLevelv2 = apprenticeship.Standard.NotionalNVQLevelv2
                });
            journeyInstance.Complete();

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();
            doc.GetElementByTestId("ViewEditLink").Should().NotBeNull();
        }

        private JourneyInstance<JourneyModel> CreateJourneyInstance(
            Guid apprenticeshipId,
            JourneyModel journeyModel = null)
        {
            return CreateJourneyInstance(
                "DeleteApprenticeship",
                configureKeys: keysBuilder => keysBuilder
                    .With("ApprenticeshipId", apprenticeshipId),
                journeyModel ?? new JourneyModel());
        }
    }
}

