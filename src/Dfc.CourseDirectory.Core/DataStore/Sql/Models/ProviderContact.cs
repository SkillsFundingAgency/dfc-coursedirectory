namespace Dfc.CourseDirectory.Core.DataStore.Sql.Models
{
    public class ProviderContact
    {
        public int ProviderId { get; set; }
        public int ProviderContactId { get; set; }
        public int ProviderContactIndex { get; set; }
        public string ContactType { get; set; }
        public string ContactRole { get; set; }
        public string AddressSaonDescription { get; set; }
        public string AddressPaonDescription { get; set; }
        public string AddressStreetDescription { get; set; }
        public string AddressLocality { get; set; }
        public string AddressItems { get; set; }
        public string AddressPostTown { get; set; }
        public string AddressCounty { get; set; }
        public string AddressPostcode { get; set; }
        public string PersonalDetailsPersonNameTitle { get; set; }
        public string PersonalDetailsPersonNameGivenName { get; set; }
        public string PersonalDetailsPersonNameFamilyName { get; set; }
        public string Telephone1 { get; set; }
        public string Telephone2 { get; set; }
        public string Fax { get; set; }
        public string WebsiteAddress { get; set; }
        public string Email { get; set; }
    }
}
