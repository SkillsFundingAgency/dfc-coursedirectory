using Dfc.CourseDirectory.Models.Models.Providers;
using Xunit;
namespace Dfc.CourseDirectory.Models.Tests
{
    public class ProviderTests
    {
        [Fact]
        public void Creating_And_Assigning_Values_To_Provider()
        {
            Contactaddress address = new Contactaddress();
            Contactpersonaldetails details = new Contactpersonaldetails();
            Provideralias[] alias = new Provideralias[1];
            Verificationdetail[] verDetail = new Verificationdetail[1];
            Providercontact[] contact = new Providercontact[1];
            Provider prov = new Provider(contact, alias, verDetail);

           // Assert.NotNull(prov);

        }
    }
}