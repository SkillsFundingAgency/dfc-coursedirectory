using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using UkrlpService;
using Xunit;

namespace Dfc.CourseDirectory.Core.Tests.ReferenceDataTests
{
    public class UkrlpServiceTests
    {
        [Fact]
        public async Task DeserializesSingleProvider()
        {
            var ukrlpWcfClientBuilder = new TestUkrlpWcfClientFactory
            {
                Endpoint = GetEndpoint()
            };
            using var server = BuildServer(ukrlpWcfClientBuilder.Endpoint);
            var requestListener = MakeRequestListener(server);

            // Theoretically backgrounding the listener like this could result in a race if the server
            // isn't ready for the first request; but it hasn't come up yet. I haven't come up with
            // a nice way of preventing it because the HttpListener works by blocking till a request arrives.
            requestListener.Start();

            var ukrlpService = new ReferenceData.Ukrlp.UkrlpService(ukrlpWcfClientBuilder);
            ProviderRecordStructure returnedProviderData;
            try
            {
                returnedProviderData = await ukrlpService.GetProviderData(10040271);
            }
            catch (TimeoutException)
            {
                // TimeoutException is thrown if there's an exception thrown within the test HttpListener.
                // The actual exception in the Listener is only thrown when Wait is called, which
                // then allows us to see it in our test results.
                requestListener.Wait();

                // If Wait didn't throw then something else went wrong, re-throw the timeout:
                throw;
            }
            returnedProviderData.Should().NotBeNull();
            using (new AssertionScope())
            {
                returnedProviderData.ExpiryDate.Should().Be(default(DateTime));
                returnedProviderData.ExpiryDateSpecified.Should().BeFalse();
                returnedProviderData.ProviderAssociations.Should().BeNull();

                returnedProviderData.ProviderName.Should().Be("JOHN FRANK TRAINING LTD");
                returnedProviderData.ProviderStatus.Should().Be("Provider deactivated, not verified");
                returnedProviderData.ProviderVerificationDate.Should().Be(new DateTime(2020, 06, 11, 17, 17, 30, 411));
                returnedProviderData.ProviderVerificationDateSpecified.Should().BeTrue();
                returnedProviderData.UnitedKingdomProviderReferenceNumber.Should().Be("10040271");

                var alias = returnedProviderData.ProviderAliases.Should().ContainSingle().Subject;
                alias.ProviderAlias.Should().Be("John Frank Training");
                alias.LastUpdated.Should().BeNull();

                var verification = returnedProviderData.VerificationDetails.Should().ContainSingle().Subject;
                verification.VerificationAuthority.Should().Be("Companies House");
                verification.VerificationID.Should().Be("07891191");

                returnedProviderData.ProviderContact.Should().HaveCount(2);

                var contactL = returnedProviderData.ProviderContact.Should().ContainSingle(c => c.ContactType == "L").Subject;
                var contactLItem1 = contactL.ContactAddress.Items.Should().ContainSingle().Subject;
                contactLItem1.Should().Be("Chorley");
                contactL.ContactAddress.Locality.Should().BeNull();
                contactL.ContactAddress.PAON.Description.Should().Be("Wood Lane");
                contactL.ContactAddress.PostCode.Should().Be("PR7 5PA");
                contactL.ContactAddress.PostTown.Should().BeNull();
                contactL.ContactAddress.SAON.Description.Should().Be("Heskin Hall Farm");
                contactL.ContactAddress.StreetDescription.Should().Be("Heskin");
                contactL.ContactAddress.UniquePropertyReferenceNumber.Should().BeNull();
                contactL.ContactAddress.UniqueStreetReferenceNumber.Should().BeNull();
                contactL.ContactEmail.Should().BeNull();
                contactL.ContactFax.Should().BeNull();
                contactL.ContactPersonalDetails.Should().NotBeNull();
                var contactLPerson = contactL.ContactPersonalDetails;
                contactLPerson.PersonFamilyName.Should().BeNull();
                contactLPerson.PersonRequestedName.Should().BeNull();
                contactLPerson.PersonGivenName.Should().BeNull();
                contactLPerson.PersonNameTitle.Should().BeNull();
                contactLPerson.PersonNameSuffix.Should().BeNull();
                contactL.ContactRole.Should().BeNull();
                contactL.ContactTelephone1.Should().Be("07700 900213");
                contactL.ContactTelephone2.Should().BeNull();
                contactL.ContactWebsiteAddress.Should().BeNull();

                var contactP = returnedProviderData.ProviderContact.Should().ContainSingle(c => c.ContactType == "P").Subject;
                var contactPItem1 = contactP.ContactAddress.Items.Should().ContainSingle().Subject;
                contactPItem1.Should().Be("Ormskirk");
                contactP.ContactAddress.Locality.Should().Be("Burscough");
                contactP.ContactAddress.PAON.Description.Should().Be("Martland Mill");
                contactP.ContactAddress.PostCode.Should().Be("L40 0SD");
                contactP.ContactAddress.PostTown.Should().BeNull();
                contactP.ContactAddress.SAON.Description.Should().BeNull();
                contactP.ContactAddress.StreetDescription.Should().Be("Mart Lane");
                contactP.ContactAddress.UniquePropertyReferenceNumber.Should().BeNull();
                contactP.ContactAddress.UniqueStreetReferenceNumber.Should().BeNull();
                contactP.ContactEmail.Should().Be("fred.smith35@example.org");
                contactP.ContactFax.Should().BeNull();
                contactP.ContactPersonalDetails.Should().NotBeNull();
                var contactPPerson = contactP.ContactPersonalDetails;
                contactPPerson.PersonFamilyName.Should().Be("Smith");
                contactPPerson.PersonRequestedName.Should().BeNull();
                contactPPerson.PersonGivenName.Should().ContainSingle().Subject.Should().Be("Fred");
                contactPPerson.PersonNameTitle.Should().ContainSingle().Subject.Should().Be("Mr");
                contactPPerson.PersonNameSuffix.Should().BeNull();
                contactP.ContactRole.Should().Be("Managing Director");
                contactP.ContactTelephone1.Should().Be("07700 900835");
                contactP.ContactTelephone2.Should().BeNull();
                contactP.ContactWebsiteAddress.Should().BeNull();
            }
        }

        private static string GetEndpoint()
        {
            int port = 49178; // doesn't seem to be an easy way of finding a free port so picked a port in the dynamic port range
            // Allow overriding port number in case we have a clash (particularly in CI).
            if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("UkrlpTestServerPort")))
            {
                port = int.Parse(Environment.GetEnvironmentVariable("UkrlpTestServerPort"));
            }

            return $"http://localhost:{port}/";
        }

        /// <param name="listenerAddress">e.g. http://localhost:9999/</param>
        private static HttpListener BuildServer(string listenerAddress)
        {
            var server = new HttpListener();
            server.Prefixes.Add(listenerAddress);
            if (!HttpListener.IsSupported)
            {
                throw new Exception("HttpListener.IsSupported returned 'false'. The tests rely on this being available.");
            }
            server.Start();
            return server;
        }

        private static Task MakeRequestListener(HttpListener server)
        {
            return new Task(() =>
            {
                var context = WaitForRequest(server);
                RespondToRequest(context);
            });
        }

        private static void RespondToRequest(HttpListenerContext context)
        {
            var response = context.Response;
            response.AddHeader("Content-Type", "text/xml;charset=utf-8");
            var buffer = File.ReadAllBytes("ReferenceDataTests/UkrlpResponse.xml");
            response.ContentLength64 = buffer.Length;
            var output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();
        }

        private static HttpListenerContext WaitForRequest(HttpListener server)
        {
            return server.GetContext();
        }
    }
}
