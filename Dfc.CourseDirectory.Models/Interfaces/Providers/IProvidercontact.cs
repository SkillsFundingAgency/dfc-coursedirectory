using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Interfaces.Providers
{
    public interface IProvidercontact
    {
        string ContactType { get; set; }
        IContactaddress ContactAddress { get; set; }
        IContactpersonaldetails ContactPersonalDetails { get; set; }
        object ContactRole { get; set; }
        string ContactTelephone1 { get; set; }
        object ContactTelephone2 { get; set; }
        string ContactFax { get; set; }
        string ContactWebsiteAddress { get; set; }
        string ContactEmail { get; set; }
        DateTime LastUpdated { get; set; }
    }
}
