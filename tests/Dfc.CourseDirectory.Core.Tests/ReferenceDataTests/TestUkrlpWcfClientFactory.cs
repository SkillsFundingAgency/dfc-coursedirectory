using System.ServiceModel;
using Dfc.CourseDirectory.Core.ReferenceData.Ukrlp;
using UkrlpService;

namespace Dfc.CourseDirectory.Core.Tests.ReferenceDataTests
{
    public class TestUkrlpWcfClientFactory : IUkrlpWcfClientFactory
    {
        /// <summary>
        /// The URI the WCF client will connect to to get the XML.
        /// </summary>
        public string Endpoint { get; set; } = "http://localhost:9999/";

        public ProviderQueryPortTypeClient Build(WcfConfiguration configuration = null)
        {
            var client = new ProviderQueryPortTypeClient();
            client.Endpoint.Address = new EndpointAddress(Endpoint);
            return client;
        }
    }
}
