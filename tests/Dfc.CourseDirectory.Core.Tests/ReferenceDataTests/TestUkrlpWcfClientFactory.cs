using System.ServiceModel;
using System.ServiceModel.Security;
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
            client.ClientCredentials.ServiceCertificate.SslCertificateAuthentication =
                new X509ServiceCertificateAuthentication
                {
                    CertificateValidationMode = X509CertificateValidationMode.None
                };

            return client;
        }
    }
}
