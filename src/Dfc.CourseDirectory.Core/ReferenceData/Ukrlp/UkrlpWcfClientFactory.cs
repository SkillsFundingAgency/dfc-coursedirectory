using UkrlpService;

namespace Dfc.CourseDirectory.Core.ReferenceData.Ukrlp
{
    public class UkrlpWcfClientFactory : IUkrlpWcfClientFactory
    {
        public ProviderQueryPortTypeClient Build(WcfConfiguration configuration = null)
        {
            var client = new ProviderQueryPortTypeClient();
            Configure(configuration, client);
            return client;
        }

        private static void Configure(WcfConfiguration configuration, ProviderQueryPortTypeClient client)
        {
            if (configuration == null)
            {
                return;
            }

            if (configuration.SendTimeout != null)
            {
                client.ChannelFactory.Endpoint.Binding.SendTimeout = configuration.SendTimeout;
            }

            if (configuration.ReceiveTimeout != null)
            {
                client.ChannelFactory.Endpoint.Binding.ReceiveTimeout = configuration.ReceiveTimeout;
            }
        }
    }
}
