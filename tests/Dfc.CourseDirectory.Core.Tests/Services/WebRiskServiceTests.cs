using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Services;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Dfc.CourseDirectory.Core.Tests.Services
{
    public class WebRiskServiceTests
    {

        [Fact]
        public async Task CheckForSecureUri_WithKnownThreat_FailsValidation()
        {
            // Arrange
            var options = Options.Create(new GoogleWebRiskSettings { ApiKey = "X" });

            var expectedData = "threat";
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var namedClient = new HttpClient(new FakeHttpMessageHandler(expectedData)); // Create a named client
            httpClientFactoryMock.Setup(factory => factory.CreateClient("namedClient")).Returns(namedClient); // Use the named client

            var webRiskService = new WebRiskService(options, httpClientFactoryMock.Object);
            var website = "https://testsafebrowsing.appspot.com/s/malware.html";

            // Act
            var result = await webRiskService.CheckForSecureUri(website);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task CheckForSecureUri_WithoutKnownThreat_PassesValidation()
        {
            // Arrange
            var options = Options.Create(new GoogleWebRiskSettings { ApiKey = "X" });

            var expectedData = "{}";
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var namedClient = new HttpClient(new FakeHttpMessageHandler(expectedData)); // Create a named client
            httpClientFactoryMock.Setup(factory => factory.CreateClient("namedClient")).Returns(namedClient); // Use the named client

            var webRiskService = new WebRiskService(options, httpClientFactoryMock.Object);
            var website = "https://www.google.com";

            // Act
            var result = await webRiskService.CheckForSecureUri(website);

            // Assert
            Assert.True(result);
        }
    }

    public class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly string _expectedResponse;

        public FakeHttpMessageHandler(string expectedResponse)
        {
            _expectedResponse = expectedResponse;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(_expectedResponse)
            };

            return await Task.FromResult(response);
        }
    }
}
