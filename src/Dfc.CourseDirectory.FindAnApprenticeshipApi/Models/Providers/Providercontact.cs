using System;

namespace Dfc.Providerportal.FindAnApprenticeship.Models.Providers
{
    public class Providercontact
    {
        public string ContactType { get; set; }
        public Contactaddress ContactAddress { get; set; }
        public Contactpersonaldetails ContactPersonalDetails { get; set; }
        public object ContactRole { get; set; }
        public string ContactTelephone1 { get; set; }
        public string ContactTelephone2 { get; set; }
        public string ContactFax { get; set; }
        public string ContactWebsiteAddress { get; set; }
        public string ContactEmail { get; set; }
        public DateTime? LastUpdated { get; set; }
    }
}