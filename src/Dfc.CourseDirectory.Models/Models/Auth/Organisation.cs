using Dfc.CourseDirectory.Models.Interfaces.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Models.Auth
{
    public class Organisation : IOrganisation
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public OrgItem Category { get; set; }
        public OrgItem Type { get; set; }
        public int? URN { get; set; }
        public int? UID { get; set; }
        public int? UKPRN { get; set; }
        public int? EstablishmentNumber { get; set; }
        public OrgItem Status { get; set; }
        public string ClosedOn { get; set; }
        public string Telephone { get; set; }
        public OrgItem Region { get; set; }
        public OrgItem LocalAuthority { get; set; }
        public OrgItem PhaseOfEducation { get; set; }
        public int? StatutoryLowAge { get; set; }
        public int? StatutoryHighAge { get; set; }
        public int? LegacyId { get; set; }
        public int? CompanyRegistrationNumber { get; set; }
    }
    public class OrgItem : IOrgItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Code { get; set; }
    }
}
