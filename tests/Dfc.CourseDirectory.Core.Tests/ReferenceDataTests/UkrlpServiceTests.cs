using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Dfc.CourseDirectory.Core.Tests.ReferenceDataTests
{
    public class UkrlpServiceTests
    {
        [Fact]
        public async Task DeserializesSingleProvider()
        {
            // Arrange

            if (!int.TryParse(Environment.GetEnvironmentVariable("UkrlpTestServerPort"), out var port))
            {
                port = 49178;
            }

            var host = $"http://localhost:{port}";
            var endpoint = "UkrlpProviderQueryWS6/ProviderQueryServiceV6";

            using var server = WebHost.CreateDefaultBuilder()
                .UseUrls(host)
                .Configure(app =>
                {
                    app.UseRouting();

                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapPost(endpoint, async httpContext =>
                        {
                            var response = File.ReadAllBytes("ReferenceDataTests/UkrlpResponse.xml");

                            httpContext.Response.Headers["Content-Type"] = "text/xml;charset=utf-8";
                            await httpContext.Response.Body.WriteAsync(response);
                        });
                    });
                })
                .Build();

            await server.StartAsync();

            var ukrlpWcfClientBuilder = new TestUkrlpWcfClientFactory
            {
                Endpoint = $"{host}/{endpoint}"
            };

            var ukrlpService = new ReferenceData.Ukrlp.UkrlpService(ukrlpWcfClientBuilder, NullLogger<ReferenceData.Ukrlp.UkrlpService>.Instance);

            // Act
            var returnedProviderData = (await ukrlpService.GetProviderData(new[] { 10040271 })).Values.SingleOrDefault();

            // Assert
            returnedProviderData.Should().NotBeNull();
            using (new AssertionScope())
            {
                returnedProviderData.ExpiryDate.Should().Be(default(DateTime));
                returnedProviderData.ExpiryDateSpecified.Should().BeFalse();
                returnedProviderData.ProviderAssociations.Should().BeNull();

                returnedProviderData.ProviderName.Should().Be("JOHN FRANK TRAINING LTD");
                returnedProviderData.AccessibleProviderName.Should().Be("Accessible Legal Name");
                returnedProviderData.ProviderStatus.Should().Be("Provider deactivated, not verified");
                // returnedProviderData.ProviderVerificationDate - ignoring, not in use
                returnedProviderData.ProviderVerificationDateSpecified.Should().BeTrue();
                returnedProviderData.UnitedKingdomProviderReferenceNumber.Should().Be("10040271");

                var alias = returnedProviderData.ProviderAliases.Should().ContainSingle().Subject;
                alias.ProviderAlias.Should().Be("John Frank Training");
                alias.LastUpdated.Should().BeNull();

                var verification = returnedProviderData.VerificationDetails.Should().ContainSingle().Subject;
                verification.VerificationAuthority.Should().Be("Companies House");
                verification.VerificationID.Should().Be("07891191");
                verification.PrimaryVerificationSource.Should().Be("true");

                returnedProviderData.ProviderContact.Should().HaveCount(2);

                var contactL = returnedProviderData.ProviderContact.Should().ContainSingle(c => c.ContactType == "L").Subject;
                contactL.ContactAddress.Address1.Should().Be("Heskin Hall Farm");
                contactL.ContactAddress.Address2.Should().Be("Wood Lane");
                contactL.ContactAddress.Address3.Should().Be("Heskin");
                contactL.ContactAddress.Address4.Should().BeNull();
                contactL.ContactAddress.Town.Should().Be("Chorley");
                contactL.ContactAddress.County.Should().Be("Lancashire");
                contactL.ContactAddress.PostCode.Should().Be("PR7 5PA");
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
                // contactL.LastUpdated  - ignoring, not in use

                var contactP = returnedProviderData.ProviderContact.Should().ContainSingle(c => c.ContactType == "P").Subject;
                contactP.ContactAddress.Address1.Should().BeNull();
                contactP.ContactAddress.Address2.Should().Be("Martland Mill");
                contactP.ContactAddress.Address3.Should().Be("Mart Lane");
                contactP.ContactAddress.Address4.Should().Be("Burscough");
                contactP.ContactAddress.Town.Should().Be("Ormskirk");
                contactP.ContactAddress.County.Should().Be("Lancashire");
                contactP.ContactAddress.PostCode.Should().Be("L40 0SD");
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
                // contactP.LastUpdated  - ignoring, not in use
            }
        }
    }
}
