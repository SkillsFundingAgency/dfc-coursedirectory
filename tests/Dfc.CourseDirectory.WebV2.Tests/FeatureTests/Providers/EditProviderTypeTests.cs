using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing;
using Dfc.CourseDirectory.WebV2.Features.Providers.EditProviderType;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Moq;
using OneOf;
using OneOf.Types;
using Xunit;

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

        [Fact]
        public async Task Post_ProviderIdAndProviderContextProviderIdDoNotMatch_ReturnsBadRequest()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(providerType: ProviderType.None);

            var content = new FormUrlEncodedContentBuilder()
                .Add("ProviderType", (int)ProviderType.FE)
                .Add("ProviderId", Guid.NewGuid())
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"providers/provider-type?providerId={providerId}")
            {
                Content = content
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
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

            SqlQuerySpy.VerifyQuery<SetProviderTLevelDefinitions, (IReadOnlyCollection<Guid>, IReadOnlyCollection<Guid>)>(query =>
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

            SqlQuerySpy.VerifyQuery<SetProviderTLevelDefinitions, (IReadOnlyCollection<Guid>, IReadOnlyCollection<Guid>)>(query =>
                query.ProviderId == providerId
                && query.TLevelDefinitionIds.SequenceEqual(Enumerable.Empty<Guid>()));
        }

        [Theory]
        [InlineData(ProviderType.None)]
        [InlineData(ProviderType.Apprenticeships)]
        [InlineData(ProviderType.FE)]
        [InlineData(ProviderType.FE | ProviderType.Apprenticeships)]
        public async Task Post_TLevelAccessIsRemoved_ReturnsConfirmWithExpectedContent(ProviderType newProviderType)
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray());

            var venueId = (await TestData.CreateVenue(providerId)).Id;

            var tLevels = await Task.WhenAll(
                TestData.CreateTLevel(
                    providerId,
                    tLevelDefinitions.First().TLevelDefinitionId,
                    locationVenueIds: new[] { venueId },
                    createdBy: User.ToUserInfo()),
                TestData.CreateTLevel(
                    providerId,
                    tLevelDefinitions.First().TLevelDefinitionId,
                    locationVenueIds: new[] { venueId },
                    createdBy: User.ToUserInfo(),
                    startDate: new DateTime(2022, 01, 01)),
                TestData.CreateTLevel(
                    providerId,
                    tLevelDefinitions.Skip(1).First().TLevelDefinitionId,
                    locationVenueIds: new[] { venueId },
                    createdBy: User.ToUserInfo())
                );

            var content = new FormUrlEncodedContentBuilder()
                .Add("ProviderType", (int)newProviderType)
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"providers/provider-type?providerId={providerId}")
            {
                Content = content
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status200OK);

            var doc = await response.GetDocument();

            doc.GetElementByTestId("provider-id").GetAttribute("value").Should().Be(providerId.ToString());
            doc.GetElementByTestId("provider-type").GetAttribute("value").Should().Be(((int)newProviderType).ToString());
            doc.GetAllElementsByTestId("confirm-affected-tLevel-id").Select(e => e.GetAttribute("value")).Should().BeEquivalentTo(
                tLevels.Select(t => t.TLevelId.ToString()));
            doc.GetAllElementsByTestId("affected-item").Select(i => i.TextContent).Should().Equal(
                tLevels.GroupBy(t => t.TLevelDefinition.TLevelDefinitionId, t => t).OrderBy(g => g.First().TLevelDefinition.Name).Select(g => $"{g.Count()} {g.First().TLevelDefinition.Name}"));

            foreach (var tLevel in tLevels)
            {
                (await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(
                    new GetTLevel() { TLevelId = tLevel.TLevelId }))).Should().NotBeNull();
            }
        }

        [Theory]
        [InlineData(ProviderType.None)]
        [InlineData(ProviderType.Apprenticeships)]
        [InlineData(ProviderType.FE)]
        [InlineData(ProviderType.FE | ProviderType.Apprenticeships)]
        public async Task Post_FromConfirm_TLevelAccessIsRemoved_WithConfirmNull_ReturnsConfirmErrorWithExpectedContent(ProviderType newProviderType)
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray());

            var venueId = (await TestData.CreateVenue(providerId)).Id;

            var tLevels = await Task.WhenAll(
                TestData.CreateTLevel(
                    providerId,
                    tLevelDefinitions.First().TLevelDefinitionId,
                    locationVenueIds: new[] { venueId },
                    createdBy: User.ToUserInfo()),
                TestData.CreateTLevel(
                    providerId,
                    tLevelDefinitions.First().TLevelDefinitionId,
                    locationVenueIds: new[] { venueId },
                    createdBy: User.ToUserInfo(),
                    startDate: new DateTime(2022, 01, 01)),
                TestData.CreateTLevel(
                    providerId,
                    tLevelDefinitions.Skip(1).First().TLevelDefinitionId,
                    locationVenueIds: new[] { venueId },
                    createdBy: User.ToUserInfo())
                );

            var content = new FormUrlEncodedContentBuilder()
                .Add("ProviderType", (int)newProviderType)
                .Add("ConfirmAffectedTLevelIds", tLevels.Select(t => t.TLevelId))
                .Add("Confirm", null)
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"providers/provider-type?providerId={providerId}")
            {
                Content = content
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

            var doc = await response.GetDocument();

            doc.GetElementByTestId("confirm-error").TextContent.Should().Be("Select yes to permanently delete");
            doc.GetElementByTestId("provider-id").GetAttribute("value").Should().Be(providerId.ToString());
            doc.GetElementByTestId("provider-type").GetAttribute("value").Should().Be(((int)newProviderType).ToString());
            doc.GetAllElementsByTestId("confirm-affected-tLevel-id").Select(e => e.GetAttribute("value")).Should().BeEquivalentTo(
                tLevels.Select(t => t.TLevelId.ToString()));
            doc.GetAllElementsByTestId("affected-item").Select(i => i.TextContent).Should().Equal(
                tLevels.GroupBy(t => t.TLevelDefinition.TLevelDefinitionId, t => t).OrderBy(g => g.First().TLevelDefinition.Name).Select(g => $"{g.Count()} {g.First().TLevelDefinition.Name}"));

            foreach (var tLevel in tLevels)
            {
                (await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(
                    new GetTLevel() { TLevelId = tLevel.TLevelId }))).Should().NotBeNull();
            }
        }

        [Theory]
        [InlineData(ProviderType.None)]
        [InlineData(ProviderType.Apprenticeships)]
        [InlineData(ProviderType.FE)]
        [InlineData(ProviderType.FE | ProviderType.Apprenticeships)]
        public async Task Post_FromConfirm_TLevelAccessIsRemoved_WithConfirmFalse_Redirects(ProviderType newProviderType)
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray());

            var venueId = (await TestData.CreateVenue(providerId)).Id;

            var tLevels = await Task.WhenAll(
                TestData.CreateTLevel(
                    providerId,
                    tLevelDefinitions.First().TLevelDefinitionId,
                    locationVenueIds: new[] { venueId },
                    createdBy: User.ToUserInfo()),
                TestData.CreateTLevel(
                    providerId,
                    tLevelDefinitions.First().TLevelDefinitionId,
                    locationVenueIds: new[] { venueId },
                    createdBy: User.ToUserInfo(),
                    startDate: new DateTime(2022, 01, 01)),
                TestData.CreateTLevel(
                    providerId,
                    tLevelDefinitions.Skip(1).First().TLevelDefinitionId,
                    locationVenueIds: new[] { venueId },
                    createdBy: User.ToUserInfo())
                );

            var content = new FormUrlEncodedContentBuilder()
                .Add("ProviderType", (int)newProviderType)
                .Add("ConfirmAffectedTLevelIds", tLevels.Select(t => t.TLevelId))
                .Add("Confirm", false)
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"providers/provider-type?providerId={providerId}")
            {
                Content = content
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status302Found);
            response.Headers.Location.OriginalString.Should().Be($"/providers/provider-type?providerId={providerId}");

            foreach (var tLevel in tLevels)
            {
                (await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(
                    new GetTLevel() { TLevelId = tLevel.TLevelId }))).Should().NotBeNull();
            }
        }

        [Theory]
        [InlineData(ProviderType.None)]
        [InlineData(ProviderType.Apprenticeships)]
        [InlineData(ProviderType.FE)]
        [InlineData(ProviderType.FE | ProviderType.Apprenticeships)]
        public async Task Post_FromConfirm_TLevelAccessIsRemoved_WithAffectedTLevelsChanged_ReturnsConfirmErrorWithExpectedContent(ProviderType newProviderType)
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray());

            var venueId = (await TestData.CreateVenue(providerId)).Id;

            var tLevels = await Task.WhenAll(
                TestData.CreateTLevel(
                    providerId,
                    tLevelDefinitions.First().TLevelDefinitionId,
                    locationVenueIds: new[] { venueId },
                    createdBy: User.ToUserInfo()),
                TestData.CreateTLevel(
                    providerId,
                    tLevelDefinitions.First().TLevelDefinitionId,
                    locationVenueIds: new[] { venueId },
                    createdBy: User.ToUserInfo(),
                    startDate: new DateTime(2022, 01, 01)),
                TestData.CreateTLevel(
                    providerId,
                    tLevelDefinitions.Skip(1).First().TLevelDefinitionId,
                    locationVenueIds: new[] { venueId },
                    createdBy: User.ToUserInfo())
                );

            var content = new FormUrlEncodedContentBuilder()
                .Add("ProviderType", (int)newProviderType)
                .Add("ConfirmAffectedTLevelIds", tLevels.Skip(1).Select(t => t.TLevelId))
                .Add("Confirm", true)
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"providers/provider-type?providerId={providerId}")
            {
                Content = content
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

            var doc = await response.GetDocument();

            doc.GetElementByTestId("affected-item-counts-error").TextContent.Should().Be("The affected T Levels have changed");
            doc.GetElementByTestId("provider-id").GetAttribute("value").Should().Be(providerId.ToString());
            doc.GetElementByTestId("provider-type").GetAttribute("value").Should().Be(((int)newProviderType).ToString());
            doc.GetAllElementsByTestId("confirm-affected-tLevel-id").Select(e => e.GetAttribute("value")).Should().BeEquivalentTo(
                tLevels.Select(t => t.TLevelId.ToString()));
            doc.GetAllElementsByTestId("affected-item").Select(i => i.TextContent).Should().Equal(
                tLevels.GroupBy(t => t.TLevelDefinition.TLevelDefinitionId, t => t).OrderBy(g => g.First().TLevelDefinition.Name).Select(g => $"{g.Count()} {g.First().TLevelDefinition.Name}"));

            foreach (var tLevel in tLevels)
            {
                (await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(
                    new GetTLevel() { TLevelId = tLevel.TLevelId }))).Should().NotBeNull();
            }
        }

        [Theory]
        [InlineData(ProviderType.None)]
        [InlineData(ProviderType.Apprenticeships)]
        [InlineData(ProviderType.FE)]
        [InlineData(ProviderType.FE | ProviderType.Apprenticeships)]
        public async Task Post_FromConfirm_TLevelAccessIsRemoved_DeletesExistingLiveTLevelsAndRedirects(ProviderType newProviderType)
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray());

            var venueId = (await TestData.CreateVenue(providerId)).Id;

            var tLevels = await Task.WhenAll(
                TestData.CreateTLevel(
                    providerId,
                    tLevelDefinitions.First().TLevelDefinitionId,
                    locationVenueIds: new[] { venueId },
                    createdBy: User.ToUserInfo()),
                TestData.CreateTLevel(
                    providerId,
                    tLevelDefinitions.First().TLevelDefinitionId,
                    locationVenueIds: new[] { venueId },
                    createdBy: User.ToUserInfo(),
                    startDate: new DateTime(2022, 01, 01)),
                TestData.CreateTLevel(
                    providerId,
                    tLevelDefinitions.Skip(1).First().TLevelDefinitionId,
                    locationVenueIds: new[] { venueId },
                    createdBy: User.ToUserInfo())
                );

            var content = new FormUrlEncodedContentBuilder()
                .Add("ProviderType", (int)newProviderType)
                .Add("ConfirmAffectedTLevelIds", tLevels.Select(t => t.TLevelId))
                .Add("Confirm", true)
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"providers/provider-type?providerId={providerId}")
            {
                Content = content
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status302Found);
            response.Headers.Location.OriginalString.Should().Be($"/providers?providerId={providerId}");

            foreach (var tLevel in tLevels)
            {
                (await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(
                    new GetTLevel() { TLevelId = tLevel.TLevelId }))).Should().BeNull();
            }
        }

        [Theory]
        [InlineData(ProviderType.TLevels)]
        [InlineData(ProviderType.TLevels | ProviderType.Apprenticeships)]
        [InlineData(ProviderType.TLevels | ProviderType.FE)]
        [InlineData(ProviderType.TLevels | ProviderType.FE | ProviderType.Apprenticeships)]
        public async Task Post_TLevelAccessIsMaintained_RedirectsAndDoesNotDeleteTLevels(ProviderType newProviderType)
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var providerTLevelDefinitionIds = tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray();

            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: providerTLevelDefinitionIds);

            var venueId = (await TestData.CreateVenue(providerId)).Id;

            var tLevel = await TestData.CreateTLevel(
                providerId,
                tLevelDefinitions.First().TLevelDefinitionId,
                locationVenueIds: new[] { venueId },
                createdBy: User.ToUserInfo());

            var content = new FormUrlEncodedContentBuilder()
                .Add("ProviderType", (int)newProviderType)
                .Add("SelectedProviderTLevelDefinitionIds", providerTLevelDefinitionIds)
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"providers/provider-type?providerId={providerId}")
            {
                Content = content
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status302Found);
            response.Headers.Location.OriginalString.Should().Be($"/providers?providerId={providerId}");

            tLevel = await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(
                new GetTLevel() { TLevelId = tLevel.TLevelId }));

            tLevel.Should().NotBeNull();
        }

        [Fact]
        public async Task Post_TLevelAccessIsRemovedForSpecificTLevelDefinition_WithNoAffectedTLevels_RemovesAccessToTLevelDefinitionAndRedirects()
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray());

            var venueId = (await TestData.CreateVenue(providerId)).Id;

            var keepingTLevelDefinitionId = tLevelDefinitions.First().TLevelDefinitionId;
            var removingTLevelDefinitionId = tLevelDefinitions.Last().TLevelDefinitionId;
            keepingTLevelDefinitionId.Should().NotBe(removingTLevelDefinitionId);

            var tLevel1 = await TestData.CreateTLevel(
                providerId,
                keepingTLevelDefinitionId,
                locationVenueIds: new[] { venueId },
                createdBy: User.ToUserInfo());

            var content = new FormUrlEncodedContentBuilder()
                .Add("ProviderType", (int)ProviderType.TLevels)
                .Add("SelectedProviderTLevelDefinitionIds", keepingTLevelDefinitionId)
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"providers/provider-type?providerId={providerId}")
            {
                Content = content
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status302Found);
            response.Headers.Location.OriginalString.Should().Be($"/providers?providerId={providerId}");

            tLevel1 = await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(
                new GetTLevel() { TLevelId = tLevel1.TLevelId }));
            tLevel1.Should().NotBeNull();
        }

        [Fact]
        public async Task Post_TLevelAccessIsRemovedForSpecificTLevelDefinition_ReturnsConfirmWithExpectedContent()
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray());

            var venueId = (await TestData.CreateVenue(providerId)).Id;

            var keepingTLevelDefinitionId = tLevelDefinitions.First().TLevelDefinitionId;
            var removingTLevelDefinitionId = tLevelDefinitions.Last().TLevelDefinitionId;
            keepingTLevelDefinitionId.Should().NotBe(removingTLevelDefinitionId);

            var tLevel1 = await TestData.CreateTLevel(
                providerId,
                keepingTLevelDefinitionId,
                locationVenueIds: new[] { venueId },
                createdBy: User.ToUserInfo());

            var tLevel2 = await TestData.CreateTLevel(
                providerId,
                removingTLevelDefinitionId,
                locationVenueIds: new[] { venueId },
                createdBy: User.ToUserInfo());

            var content = new FormUrlEncodedContentBuilder()
                .Add("ProviderType", (int)ProviderType.TLevels)
                .Add("SelectedProviderTLevelDefinitionIds", keepingTLevelDefinitionId)
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"providers/provider-type?providerId={providerId}")
            {
                Content = content
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status200OK);

            var doc = await response.GetDocument();

            doc.GetElementByTestId("provider-id").GetAttribute("value").Should().Be(providerId.ToString());
            doc.GetElementByTestId("provider-type").GetAttribute("value").Should().Be(((int)ProviderType.TLevels).ToString());
            doc.GetAllElementsByTestId("confirm-affected-tLevel-id").Select(e => e.GetAttribute("value")).Should().BeEquivalentTo(
                new[] { tLevel2.TLevelId.ToString() });
            doc.GetAllElementsByTestId("selected-provider-tlevel-definition-id").Select(e => e.GetAttribute("value")).Should().BeEquivalentTo(
                new[] { keepingTLevelDefinitionId.ToString() });
            doc.GetAllElementsByTestId("affected-item").Select(i => i.TextContent).Should().Equal(
                new[] { tLevel2 }.GroupBy(t => t.TLevelDefinition.TLevelDefinitionId, t => t).OrderBy(g => g.First().TLevelDefinition.Name).Select(g => $"{g.Count()} {g.First().TLevelDefinition.Name}"));

            tLevel1 = await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(
                new GetTLevel() { TLevelId = tLevel1.TLevelId }));
            tLevel2 = await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(
                new GetTLevel() { TLevelId = tLevel2.TLevelId }));

            using (new AssertionScope())
            {
                tLevel1.Should().NotBeNull();
                tLevel2.Should().NotBeNull();
            }
        }

        [Fact]
        public async Task Post_FromConfirm_TLevelAccessIsRemovedForSpecificTLevelDefinition_WithConfirmNull_ReturnsConfirmErrorWithExpectedContent()
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray());

            var venueId = (await TestData.CreateVenue(providerId)).Id;

            var keepingTLevelDefinitionId = tLevelDefinitions.First().TLevelDefinitionId;
            var removingTLevelDefinitionId = tLevelDefinitions.Last().TLevelDefinitionId;
            keepingTLevelDefinitionId.Should().NotBe(removingTLevelDefinitionId);

            var tLevel1 = await TestData.CreateTLevel(
                providerId,
                keepingTLevelDefinitionId,
                locationVenueIds: new[] { venueId },
                createdBy: User.ToUserInfo());

            var tLevel2 = await TestData.CreateTLevel(
                providerId,
                removingTLevelDefinitionId,
                locationVenueIds: new[] { venueId },
                createdBy: User.ToUserInfo());

            var content = new FormUrlEncodedContentBuilder()
                .Add("ProviderType", (int)ProviderType.TLevels)
                .Add("SelectedProviderTLevelDefinitionIds", keepingTLevelDefinitionId)
                .Add("ConfirmAffectedTLevelIds", new[] { tLevel2.TLevelId })
                .Add("Confirm", null)
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"providers/provider-type?providerId={providerId}")
            {
                Content = content
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

            var doc = await response.GetDocument();

            doc.GetElementByTestId("confirm-error").TextContent.Should().Be("Select yes to permanently delete");
            doc.GetElementByTestId("provider-id").GetAttribute("value").Should().Be(providerId.ToString());
            doc.GetElementByTestId("provider-type").GetAttribute("value").Should().Be(((int)ProviderType.TLevels).ToString());
            doc.GetAllElementsByTestId("confirm-affected-tLevel-id").Select(e => e.GetAttribute("value")).Should().BeEquivalentTo(
                new[] { tLevel2.TLevelId.ToString() });
            doc.GetAllElementsByTestId("selected-provider-tlevel-definition-id").Select(e => e.GetAttribute("value")).Should().BeEquivalentTo(
                new[] { keepingTLevelDefinitionId.ToString() });
            doc.GetAllElementsByTestId("affected-item").Select(i => i.TextContent).Should().Equal(
                new[] { tLevel2 }.GroupBy(t => t.TLevelDefinition.TLevelDefinitionId, t => t).OrderBy(g => g.First().TLevelDefinition.Name).Select(g => $"{g.Count()} {g.First().TLevelDefinition.Name}"));

            tLevel1 = await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(
                new GetTLevel() { TLevelId = tLevel1.TLevelId }));
            tLevel2 = await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(
                new GetTLevel() { TLevelId = tLevel2.TLevelId }));

            using (new AssertionScope())
            {
                tLevel1.Should().NotBeNull();
                tLevel2.Should().NotBeNull();
            }
        }

        [Fact]
        public async Task Post_FromConfirm_TLevelAccessIsRemovedForSpecificTLevelDefinition_WithConfirmFalse_Redirects()
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray());

            var venueId = (await TestData.CreateVenue(providerId)).Id;

            var keepingTLevelDefinitionId = tLevelDefinitions.First().TLevelDefinitionId;
            var removingTLevelDefinitionId = tLevelDefinitions.Last().TLevelDefinitionId;
            keepingTLevelDefinitionId.Should().NotBe(removingTLevelDefinitionId);

            var tLevel1 = await TestData.CreateTLevel(
                providerId,
                keepingTLevelDefinitionId,
                locationVenueIds: new[] { venueId },
                createdBy: User.ToUserInfo());

            var tLevel2 = await TestData.CreateTLevel(
                providerId,
                removingTLevelDefinitionId,
                locationVenueIds: new[] { venueId },
                createdBy: User.ToUserInfo());

            var content = new FormUrlEncodedContentBuilder()
                .Add("ProviderType", (int)ProviderType.TLevels)
                .Add("SelectedProviderTLevelDefinitionIds", keepingTLevelDefinitionId)
                .Add("ConfirmAffectedTLevelIds", new[] { tLevel2.TLevelId })
                .Add("Confirm", false)
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"providers/provider-type?providerId={providerId}")
            {
                Content = content
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status302Found);
            response.Headers.Location.OriginalString.Should().Be($"/providers/provider-type?providerId={providerId}");

            tLevel1 = await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(
                new GetTLevel() { TLevelId = tLevel1.TLevelId }));
            tLevel2 = await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(
                new GetTLevel() { TLevelId = tLevel2.TLevelId }));

            using (new AssertionScope())
            {
                tLevel1.Should().NotBeNull();
                tLevel2.Should().NotBeNull();
            }
        }

        [Fact]
        public async Task Post_FromConfirm_TLevelAccessIsRemovedForSpecificTLevelDefinition_WithAffectedTLevelsChanged_ReturnsConfirmErrorWithExpectedContent()
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray());

            var venueId = (await TestData.CreateVenue(providerId)).Id;

            var keepingTLevelDefinitionId = tLevelDefinitions.First().TLevelDefinitionId;
            var removingTLevelDefinitionId = tLevelDefinitions.Last().TLevelDefinitionId;
            keepingTLevelDefinitionId.Should().NotBe(removingTLevelDefinitionId);

            var tLevel1 = await TestData.CreateTLevel(
                providerId,
                keepingTLevelDefinitionId,
                locationVenueIds: new[] { venueId },
                createdBy: User.ToUserInfo());

            var tLevel2 = await TestData.CreateTLevel(
                providerId,
                removingTLevelDefinitionId,
                locationVenueIds: new[] { venueId },
                createdBy: User.ToUserInfo());

            var content = new FormUrlEncodedContentBuilder()
                .Add("ProviderType", (int)ProviderType.TLevels)
                .Add("SelectedProviderTLevelDefinitionIds", keepingTLevelDefinitionId)
                .Add("ConfirmAffectedTLevelIds", new[] { Guid.NewGuid() })
                .Add("Confirm", true)
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"providers/provider-type?providerId={providerId}")
            {
                Content = content
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

            var doc = await response.GetDocument();

            doc.GetElementByTestId("affected-item-counts-error").TextContent.Should().Be("The affected T Levels have changed");
            doc.GetElementByTestId("provider-id").GetAttribute("value").Should().Be(providerId.ToString());
            doc.GetElementByTestId("provider-type").GetAttribute("value").Should().Be(((int)ProviderType.TLevels).ToString());
            doc.GetAllElementsByTestId("confirm-affected-tLevel-id").Select(e => e.GetAttribute("value")).Should().BeEquivalentTo(
                new[] { tLevel2.TLevelId.ToString() });
            doc.GetAllElementsByTestId("selected-provider-tlevel-definition-id").Select(e => e.GetAttribute("value")).Should().BeEquivalentTo(
                new[] { keepingTLevelDefinitionId.ToString() });
            doc.GetAllElementsByTestId("affected-item").Select(i => i.TextContent).Should().Equal(
                new[] { tLevel2 }.GroupBy(t => t.TLevelDefinition.TLevelDefinitionId, t => t).OrderBy(g => g.First().TLevelDefinition.Name).Select(g => $"{g.Count()} {g.First().TLevelDefinition.Name}"));

            tLevel1 = await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(
                new GetTLevel() { TLevelId = tLevel1.TLevelId }));
            tLevel2 = await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(
                new GetTLevel() { TLevelId = tLevel2.TLevelId }));

            using (new AssertionScope())
            {
                tLevel1.Should().NotBeNull();
                tLevel2.Should().NotBeNull();
            }
        }

        [Fact]
        public async Task Post_FromConfirm_TLevelAccessIsRemovedForSpecificTLevelDefinition_DeletesExistingLiveTLevelsForThatDefinitionOnlyAndRedirects()
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray());

            var venueId = (await TestData.CreateVenue(providerId)).Id;

            var keepingTLevelDefinitionId = tLevelDefinitions.First().TLevelDefinitionId;
            var removingTLevelDefinitionId = tLevelDefinitions.Last().TLevelDefinitionId;
            keepingTLevelDefinitionId.Should().NotBe(removingTLevelDefinitionId);

            var tLevel1 = await TestData.CreateTLevel(
                providerId,
                keepingTLevelDefinitionId,
                locationVenueIds: new[] { venueId },
                createdBy: User.ToUserInfo());

            var tLevel2 = await TestData.CreateTLevel(
                providerId,
                removingTLevelDefinitionId,
                locationVenueIds: new[] { venueId },
                createdBy: User.ToUserInfo());

            var content = new FormUrlEncodedContentBuilder()
                .Add("ProviderType", (int)ProviderType.TLevels)
                .Add("SelectedProviderTLevelDefinitionIds", keepingTLevelDefinitionId)
                .Add("ConfirmAffectedTLevelIds", new[] { tLevel2.TLevelId })
                .Add("Confirm", true)
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"providers/provider-type?providerId={providerId}")
            {
                Content = content
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status302Found);
            response.Headers.Location.OriginalString.Should().Be($"/providers?providerId={providerId}");

            tLevel1 = await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(
                new GetTLevel() { TLevelId = tLevel1.TLevelId }));
            tLevel2 = await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(
                new GetTLevel() { TLevelId = tLevel2.TLevelId }));

            using (new AssertionScope())
            {
                tLevel1.Should().NotBeNull();
                tLevel2.Should().BeNull();
            }
        }
    }
}
