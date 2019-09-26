using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;

namespace Dfc.CourseDirectory.Services.Tests.Unit.Mocks
{
    public class HttpClientMockFactory
    {
        public static HttpClient GetClient(string result, HttpStatusCode code)
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                // Setup the PROTECTED method to mock
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                // prepare the expected response of the mocked http call
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = code,
                    Content = new StringContent(result),
                })
                .Verifiable();
            return new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://test.com/"),
            };
        }
    }
}
