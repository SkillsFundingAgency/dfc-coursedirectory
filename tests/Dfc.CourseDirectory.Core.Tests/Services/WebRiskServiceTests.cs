using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;

namespace Dfc.CourseDirectory.Core.Tests.Services
{
    public class WebRiskServiceTests
    {

        [Fact]
        public async void SecureWebsite_WithMalware_FailsValidation()
        {
            // Arrange
            var options = Options.Create(new GoogleWebRiskOptions { ApiKey = "X" });

            var expectedData = "threat";
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            httpMessageHandlerMock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(expectedData)
            });
            httpClientFactoryMock.Setup(factory => factory.CreateClient()).Returns(new HttpClient(httpMessageHandlerMock.Object));

            var webRiskService = new WebRiskService(options, httpClientFactoryMock.Object);
            var website = "https://testsafebrowsing.appspot.com/s/malware.html";

            // Act
            var result = await webRiskService.CheckForSecureUri(website);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async void CheckForSecureUri_WithKnownThreat_FailsValidation()
        {
            // Arrange
            var options = Options.Create(new GoogleWebRiskOptions { ApiKey = "X" });

            var expectedData = "threat";
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var namedClient = new HttpClient(new FakeHttpMessageHandler(expectedData)); // Create a named client
            httpClientFactoryMock.Setup(factory => factory.CreateClient("myNamedClient")).Returns(namedClient); // Use the named client

            var webRiskService = new WebRiskService(options, httpClientFactoryMock.Object);
            var website = "https://testsafebrowsing.appspot.com/s/malware.html";

            // Act
            var result = await webRiskService.CheckForSecureUri(website); //inside this method, client is null so there is an error

            // Assert
            Assert.False(result);
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
