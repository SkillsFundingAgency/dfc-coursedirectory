using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing;
using Dfc.CourseDirectory.WebV2.Features.Providers.EditProviderType;
using FluentAssertions;
using Moq;
using OneOf;
using OneOf.Types;
using Xunit;
using SqlQueries = Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.Providers
{
    public class EditProviderTypeTests : MvcTestBase
    {
        public EditProviderTypeTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Theory]
        [InlineData(TestUserType.ProviderSuperUser)]
        [InlineData(TestUserType.ProviderUser)]
        public async Task Get_UserCannotEditProviderType_ReturnsForbidden(TestUserType userType)
        {
            // Arrange
            var providerId = await TestData.CreateProvider();

            var request = new HttpRequestMessage(HttpMethod.Get, $"providers/provider-type?providerId={providerId}");

            await User.AsTestUser(userType, providerId);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Get_ProviderDoesNotExist_ReturnsRedirect()
        {
            // Arrange
            var providerId = Guid.NewGuid();

            var request = new HttpRequestMessage(HttpMethod.Get, $"providers/provider-type?providerId={providerId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Found);
        }

        [Fact]
        public async Task Get_ValidRequestNoneProviderType_RendersExpectedOutput()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(providerType: ProviderType.None);

            var request = new HttpRequestMessage(HttpMethod.Get, $"providers/provider-type?providerId={providerId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            doc.GetElementByTestId("apprenticeships").GetAttribute("checked").Should().NotBe("checked");
            doc.GetElementByTestId("fe").GetAttribute("checked").Should().NotBe("checked");
        }

        [Theory]
        [InlineData(ProviderType.Apprenticeships, new[] { "apprenticeships" })]
        [InlineData(ProviderType.FE, new[] { "fe" })]
        [InlineData(ProviderType.TLevels, new[] { "tLevels" })]
        [InlineData(ProviderType.Apprenticeships | ProviderType.FE, new[] { "fe", "apprenticeships" })]
        [InlineData(ProviderType.Apprenticeships | ProviderType.TLevels, new[] { "apprenticeships", "tLevels" })]
        [InlineData(ProviderType.FE | ProviderType.TLevels, new[] { "fe", "tLevels" })]
        [InlineData(ProviderType.Apprenticeships | ProviderType.FE | ProviderType.TLevels, new[] { "fe", "apprenticeships", "tLevels" })]

        public async Task Get_ValidRequest_RendersExpectedOutput(
            ProviderType providerType,
            IEnumerable<string> expectedCheckedTestIds)
        {
            // Arrange
            var providerId = await TestData.CreateProvider(providerType: providerType);

            var tLevelDefinitionIds = await Task.WhenAll(Enumerable.Range(0, 3).Select(_ => TestData.CreateTLevelDefinition()));

            var request = new HttpRequestMessage(HttpMethod.Get, $"providers/provider-type?providerId={providerId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();

            AssertElementWithTestIdHasExpectedCheckedValue("fe");
            AssertElementWithTestIdHasExpectedCheckedValue("apprenticeships");
            AssertElementWithTestIdHasExpectedCheckedValue("tLevels");

            void AssertElementWithTestIdHasExpectedCheckedValue(string testId)
            {
                var option = doc.GetElementByTestId(testId);
                var checkedAttr = option.GetAttribute("checked");

                var expectChecked = expectedCheckedTestIds.Contains(testId);

                if (expectChecked)
                {
                    checkedAttr.Should().Be("checked");
                }
                else
                {
                    checkedAttr.Should().NotBe("checked");
                }
            }
        }

        [Theory]
        [InlineData(ProviderType.FE, new int[0], new int[0], new int[0])]
        [InlineData(ProviderType.FE, new[] { 1, 2, 3 }, new int[0], new int[0])]
        [InlineData(ProviderType.FE, new[] { 1, 2, 3 }, new[] { 2, 3 }, new int[0])]
        [InlineData(ProviderType.TLevels, new[] { 1, 2, 3 }, new int[0], new int[0])]
        [InlineData(ProviderType.TLevels, new[] { 1, 2, 3 }, new[] { 1, 3 }, new[] { 1, 3 })]
        [InlineData(ProviderType.Apprenticeships | ProviderType.TLevels, new[] { 1, 2, 3 }, new int[0], new int[0])]
        [InlineData(ProviderType.Apprenticeships | ProviderType.TLevels, new[] { 1, 2, 3 }, new[] { 1, 2 }, new[] { 1, 2 })]
        public async Task Get_ValidRequestWithSelectedTLevelDefinitions_RendersExpectedOutput(
            ProviderType providerType,
            IEnumerable<int> tLevelDefinitionIds,
            IEnumerable<int> selectedTLevelDefinitionIds,
            IEnumerable<int> expectedSelectedTLevelDefinitionIds)
        {
            // Arrange
            var providerId = await TestData.CreateProvider(providerType: providerType);

            var parsedTLevelDefinitionIds = tLevelDefinitionIds.Select(ToGuid).ToArray();
            var parsedSelectedTLevelDefinitionIds = selectedTLevelDefinitionIds.Select(ToGuid).ToArray();
            var parsedExpectedSelectedTLevelDefinitionIds = expectedSelectedTLevelDefinitionIds.Select(ToGuid).ToArray();

            await Task.WhenAll(parsedTLevelDefinitionIds.Select(id => TestData.CreateTLevelDefinition(tLevelDefinitionId: id)));
            await TestData.SetProviderTLevelDefinitions(providerId, parsedSelectedTLevelDefinitionIds);

            var request = new HttpRequestMessage(HttpMethod.Get, $"providers/provider-type?providerId={providerId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();

            parsedExpectedSelectedTLevelDefinitionIds.All(id => IsChecked($"tLevelDefinition-{id}")).Should().BeTrue();
            parsedTLevelDefinitionIds.Except(parsedExpectedSelectedTLevelDefinitionIds).Any(id => IsChecked($"tLevelDefinition-{id}")).Should().BeFalse();

            bool IsChecked(string testId)
            {
                var option = doc.GetElementByTestId(testId);
                return option.GetAttribute("checked") == "checked";
            }

            Guid ToGuid(int value)
            {
                var bytes = new byte[16];
                BitConverter.GetBytes(value).CopyTo(bytes, 0);
                return new Guid(bytes);
            }
        }

        [Theory]
        [InlineData(TestUserType.ProviderSuperUser)]
        [InlineData(TestUserType.ProviderUser)]
        public async Task Post_UserCannotEditProviderType_ReturnsForbidden(TestUserType userType)
        {
            // Arrange
            var providerId = await TestData.CreateProvider();

            var content = new FormUrlEncodedContentBuilder()
                .Add("ProviderType", (int)ProviderType.FE)
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"providers/provider-type?providerId={providerId}")
            {
                Content = content
            };

            await User.AsTestUser(userType, providerId);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Post_ProviderDoesNotExist_ReturnsRedirect()
        {
            // Arrange
            var providerId = Guid.NewGuid();

            var content = new FormUrlEncodedContentBuilder()
                .Add("ProviderType", (int)ProviderType.FE)
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"providers/provider-type?providerId={providerId}")
            {
                Content = content
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Found);
        }

        [Theory]
        [InlineData(ProviderType.Apprenticeships)]
        [InlineData(ProviderType.FE)]
        [InlineData(ProviderType.Apprenticeships | ProviderType.FE)]
        public async Task Post_ValidRequest_UpdatesProviderTypeAndRedirects(ProviderType providerType)
        {
            // Arrange
            var providerId = await TestData.CreateProvider(providerType: ProviderType.None);

            var content = new FormUrlEncodedContentBuilder()
                .Add("ProviderType", (int)providerType)
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"providers/provider-type?providerId={providerId}")
            {
                Content = content
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.OriginalString.Should().Be($"/providers?providerId={providerId}");

            CosmosDbQueryDispatcher.VerifyExecuteQuery<UpdateProviderType, OneOf<NotFound, Success>>(q =>
                q.ProviderId == providerId && q.ProviderType == providerType);
        }

        [Theory]
        [InlineData(ProviderType.TLevels)]
        [InlineData(ProviderType.Apprenticeships | ProviderType.TLevels)]
        [InlineData(ProviderType.FE | ProviderType.TLevels)]
        [InlineData(ProviderType.Apprenticeships | ProviderType.FE | ProviderType.TLevels)]
        public async Task Post_WithTLevelsAndSelectedTLevelDefinitions_UpdatesProviderTypeAndSelectedTLevelDefinitionsAndRedirects(ProviderType providerType)
        {
            // Arrange
            var providerId = await TestData.CreateProvider(providerType: ProviderType.None);

            var tLevelDefinitionIds = (await Task.WhenAll(Enumerable.Range(0, 3).Select(_ => TestData.CreateTLevelDefinition())))
                .OrderBy(_ => Guid.NewGuid())
                .Take(2)
                .ToArray();

            var contentBuilder = new FormUrlEncodedContentBuilder()
                .Add(nameof(Command.ProviderType), (int)providerType);

            foreach (var tLevelDefinitionId in tLevelDefinitionIds)
            {
                contentBuilder.Add(nameof(Command.SelectedProviderTLevelDefinitionIds), tLevelDefinitionId);
            }

            var request = new HttpRequestMessage(HttpMethod.Post, $"providers/provider-type?providerId={providerId}")
            {
                Content = contentBuilder.ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.OriginalString.Should().Be($"/providers?providerId={providerId}");

            CosmosDbQueryDispatcher.VerifyExecuteQuery<UpdateProviderType, OneOf<NotFound, Success>>(q =>
                q.ProviderId == providerId && q.ProviderType == providerType);

            SqlQuerySpy.VerifyQuery<SqlQueries.SetAuthorizedTLevelDefinitionsForProvider, None>(query =>
                query.ProviderId == providerId
                && query.TLevelDefinitionIds.SequenceEqual(tLevelDefinitionIds));
        }

        [Theory]
        [InlineData(ProviderType.TLevels)]
        [InlineData(ProviderType.Apprenticeships | ProviderType.TLevels)]
        [InlineData(ProviderType.FE | ProviderType.TLevels)]
        [InlineData(ProviderType.Apprenticeships | ProviderType.FE | ProviderType.TLevels)]
        public async Task Post_WithTLevelsAndNoSelectedTLevelDefinitions_DoesNotUpdateProviderTypeOrSelectedTLevelDefinitionsAndReturnsViewWithErrorMessage(ProviderType providerType)
        {
            // Arrange
            var providerId = await TestData.CreateProvider(providerType: ProviderType.None);

            await Task.WhenAll(Enumerable.Range(0, 3).Select(_ => TestData.CreateTLevelDefinition()));

            var contentBuilder = new FormUrlEncodedContentBuilder()
                .Add(nameof(Command.ProviderType), (int)providerType);

            var request = new HttpRequestMessage(HttpMethod.Post, $"providers/provider-type?providerId={providerId}")
            {
                Content = contentBuilder.ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var doc = await response.GetDocument();
            doc.AssertHasError(nameof(Command.SelectedProviderTLevelDefinitionIds), "Select the T Levels this provider can offer");

            CosmosDbQueryDispatcher.VerifyExecuteQuery<UpdateProviderType, OneOf<NotFound, Success>>(q =>
                q.ProviderId == providerId && q.ProviderType == providerType, Times.Never());
        }

        [Fact]
        public async Task Post_WithTLevelsProviderAndInvalidTLevelDefinitionId_DoesNotUpdateProviderTypeOrSelectedTLevelDefinitionsAndReturnsViewWithErrorMessage()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(providerType: ProviderType.None);

            var tLevelDefinitionIds = await Task.WhenAll(Enumerable.Range(0, 3).Select(_ => TestData.CreateTLevelDefinition()));
            var selectedTLevelDefinitionIds = tLevelDefinitionIds.OrderBy(_ => Guid.NewGuid()).Take(2).ToArray();

            var contentBuilder = new FormUrlEncodedContentBuilder()
                .Add(nameof(Command.ProviderType), (int)ProviderType.TLevels);

            foreach (var tLevelDefinitionId in selectedTLevelDefinitionIds)
            {
                contentBuilder.Add(nameof(Command.SelectedProviderTLevelDefinitionIds), tLevelDefinitionId);
            }

            contentBuilder.Add(nameof(Command.SelectedProviderTLevelDefinitionIds), Guid.NewGuid());

            var request = new HttpRequestMessage(HttpMethod.Post, $"providers/provider-type?providerId={providerId}")
            {
                Content = contentBuilder.ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var doc = await response.GetDocument();
            doc.AssertHasError(nameof(Command.SelectedProviderTLevelDefinitionIds), "Select a valid T Level");

            CosmosDbQueryDispatcher.VerifyExecuteQuery<UpdateProviderType, OneOf<NotFound, Success>>(q =>
                q.ProviderId == providerId && q.ProviderType == ProviderType.TLevels, Times.Never());
        }

        [Theory]
        [InlineData(ProviderType.None)]
        [InlineData(ProviderType.Apprenticeships)]
        [InlineData(ProviderType.FE)]
        [InlineData(ProviderType.Apprenticeships | ProviderType.FE)]
        public async Task Post_WithTLevelsProviderWithSelectedTLevelDefinitions_UpdatesProviderTypeAndRemovesSelectedTLevelDefinitionsAndRedirects(ProviderType newProviderType)
        {
            // Arrange
            var providerId = await TestData.CreateProvider(providerType: ProviderType.TLevels);

            var tLevelDefinitionIds = (await Task.WhenAll(Enumerable.Range(0, 3).Select(_ => TestData.CreateTLevelDefinition())))
                .OrderBy(_ => Guid.NewGuid())
                .Take(2)
                .ToArray();

            await TestData.SetProviderTLevelDefinitions(providerId, tLevelDefinitionIds);

            var contentBuilder = new FormUrlEncodedContentBuilder()
                .Add(nameof(Command.ProviderType), (int)newProviderType);

            foreach (var tLevelDefinitionId in tLevelDefinitionIds)
            {
                contentBuilder.Add(nameof(Command.SelectedProviderTLevelDefinitionIds), tLevelDefinitionId);
            }

            var request = new HttpRequestMessage(HttpMethod.Post, $"providers/provider-type?providerId={providerId}")
            {
                Content = contentBuilder.ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.OriginalString.Should().Be($"/providers?providerId={providerId}");

            CosmosDbQueryDispatcher.VerifyExecuteQuery<UpdateProviderType, OneOf<NotFound, Success>>(q =>
                q.ProviderId == providerId && q.ProviderType == newProviderType);

            SqlQuerySpy.VerifyQuery<SqlQueries.SetAuthorizedTLevelDefinitionsForProvider, None>(query =>
                query.ProviderId == providerId
                && query.TLevelDefinitionIds.SequenceEqual(Enumerable.Empty<Guid>()));
        }
    }
}
