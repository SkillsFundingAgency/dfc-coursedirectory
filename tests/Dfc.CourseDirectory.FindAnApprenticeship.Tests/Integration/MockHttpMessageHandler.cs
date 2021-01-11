using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Dfc.ProviderPortal.FindAnApprenticeship.UnitTests.Integration
{
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _send;

        public MockHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send)
        {
            _send = send ?? throw new ArgumentNullException(nameof(send));
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return _send(request, cancellationToken);
        }
    }
}