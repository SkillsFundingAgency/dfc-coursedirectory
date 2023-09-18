using System;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Models
{
    public class ProviderContact
    {
        public Guid ProviderId { get; set; }
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

        public override bool Equals(Object obj)
        {
            var providerContact = obj as ProviderContact;
            if (ContactType == providerContact.ContactType &&
                AddressSaonDescription == providerContact.AddressSaonDescription &&
                AddressPaonDescription == providerContact.AddressPaonDescription &&
                AddressStreetDescription == providerContact.AddressStreetDescription &&
                AddressLocality == providerContact.AddressLocality &&
                AddressItems == providerContact.AddressItems &&
                AddressPostTown == providerContact.AddressPostTown &&
                AddressCounty == providerContact.AddressCounty &&
                AddressPostcode == providerContact.AddressPostcode &&
                PersonalDetailsPersonNameTitle == providerContact.PersonalDetailsPersonNameTitle &&
                PersonalDetailsPersonNameGivenName == providerContact.PersonalDetailsPersonNameGivenName &&
                PersonalDetailsPersonNameFamilyName == providerContact.PersonalDetailsPersonNameFamilyName &&
                Telephone1 == providerContact.Telephone1 &&
                Fax == providerContact.Fax &&
                WebsiteAddress == providerContact.WebsiteAddress &&
                Email == providerContact.Email)
            {
                return true;
            }
            else { return false; }
        }
    }

}
