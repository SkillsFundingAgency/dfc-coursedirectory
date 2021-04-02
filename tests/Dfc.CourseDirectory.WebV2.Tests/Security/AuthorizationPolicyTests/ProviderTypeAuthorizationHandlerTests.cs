using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing;
using Dfc.CourseDirectory.WebV2.Security.AuthorizationPolicies;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Dfc.CourseDirectory.WebV2.Tests.Security.AuthorizationPolicyTests
{
    public class ProviderTypeAuthorizationHandlerTests :
        IClassFixture<ProviderTypeAuthorizationHandlerTestsFixture>,
        IAsyncLifetime,
        IDisposable
    {
        public ProviderTypeAuthorizationHandlerTests(ProviderTypeAuthorizationHandlerTestsFixture fixture)
        {
            Fixture = fixture;
            HttpClient = fixture.CreateClient();

            Fixture.OnTestStarting();
        }

        [Fact]
        public async Task CurrentProviderNotSet_ReturnsForbidden()
        {
            // Arrange
            await User.AsDeveloper();

            var request = new HttpRequestMessage(HttpMethod.Get, "ProviderTypeAuthorizationHandlerTests/FeOnly");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task CurrentProviderMatchesProviderType_ReturnsOk()
        {
            // Arrange
            var providerType = ProviderType.FE;

            var provider = await TestData.CreateProvider(providerType: providerType);

            await User.AsProviderUser(provider.ProviderId, providerType);

            var request = new HttpRequestMessage(HttpMethod.Get, "ProviderTypeAuthorizationHandlerTests/FeOnly");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task CurrentProviderContainsProviderType_ReturnsOk()
        {
            // Arrange
            var providerType = ProviderType.Apprenticeships | ProviderType.FE;

            var provider = await TestData.CreateProvider(providerType: providerType);

            await User.AsProviderUser(provider.ProviderId, providerType);

            var request = new HttpRequestMessage(HttpMethod.Get, "ProviderTypeAuthorizationHandlerTests/FeOnly");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Theory]
        [InlineData(ProviderType.Apprenticeships)]
        [InlineData(ProviderType.FE)]
        [InlineData(ProviderType.FE | ProviderType.Apprenticeships)]
        public async Task MultipleProviderTypesPermitted_AllowsEither(ProviderType providerType)
        {
            // Arrange
            var provider = await TestData.CreateProvider(providerType: providerType);

            await User.AsProviderUser(provider.ProviderId, providerType);

            var request = new HttpRequestMessage(HttpMethod.Get, "ProviderTypeAuthorizationHandlerTests/FeOrApprenticeship");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        public void Dispose() => Fixture.Dispose();

        public Task DisposeAsync() => Task.CompletedTask;

        public Task InitializeAsync() => Fixture.OnTestStartingAsync();

        private ProviderTypeAuthorizationHandlerTestsFixture Fixture { get; }

        private HttpClient HttpClient { get; }

        private TestData TestData => Fixture.TestData;

        private TestUserInfo User => Fixture.User;
    }

    public class ProviderTypeAuthorizationHandlerTestsFixture : CourseDirectoryApplicationFactory
    {
        public ProviderTypeAuthorizationHandlerTestsFixture(IMessageSink messageSink)
            : base(messageSink)
        {
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);

            builder.ConfigureServices(services =>
            {
                services.AddAuthorization(options =>
                {
                    options.AddPolicy(
                        "Tests:Fe",
                        policy => policy.AddRequirements(
                            new ProviderTypeRequirement(ProviderType.FE)));

                    options.AddPolicy(
                        "Tests:FeOrApprenticeship",
                        policy => policy.AddRequirements(
                            new ProviderTypeRequirement(ProviderType.FE | ProviderType.Apprenticeships)));
                });
            });
        }
    }

    public class ProviderTypeAuthorizationHandlerTestsController : Controller
    {
        [HttpGet("ProviderTypeAuthorizationHandlerTests/FeOnly")]
        [Authorize(Policy = "Tests:Fe")]
        public IActionResult FeOnly() => Ok();

        [HttpGet("ProviderTypeAuthorizationHandlerTests/FeOrApprenticeship")]
        [Authorize(Policy = "Tests:FeOrApprenticeship")]
        public IActionResult FeOrApprenticeship() => Ok();
    }
}
