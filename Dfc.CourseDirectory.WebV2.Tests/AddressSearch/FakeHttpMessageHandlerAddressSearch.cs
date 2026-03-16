using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.WebV2.Tests.AddressSearch
{
    public class FakeHttpMessageHandlerAddressSearch : HttpMessageHandler
    {
        private readonly object _expectedPayload;
        private readonly HttpStatusCode _expectedResponseCode;

        public FakeHttpMessageHandlerAddressSearch(object payload, HttpStatusCode responseCode = HttpStatusCode.OK)
        {
            _expectedPayload = payload;
            _expectedResponseCode = responseCode;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = null,
                DictionaryKeyPolicy = null
            };

            var response = new HttpResponseMessage(_expectedResponseCode)
            {
                Content = JsonContent.Create(_expectedPayload, options: jsonOptions),
               
            };
            return await Task.FromResult(response);
        }
    }
}
