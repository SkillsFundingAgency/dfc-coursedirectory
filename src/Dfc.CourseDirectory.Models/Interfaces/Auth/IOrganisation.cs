using Dfc.CourseDirectory.Models.Models.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Interfaces.Auth
{
    public interface IOrganisation
    {
        Guid Id { get; }
        string Name { get; }
        OrgItem Category { get; }
        OrgItem Type { get; }
        int? URN { get; }
        int? UID { get; }
        int? UKPRN { get; }
        int? EstablishmentNumber { get; }
        OrgItem Status { get; }
        string ClosedOn { get; }
        string Telephone { get; }
        OrgItem Region { get; }
        OrgItem LocalAuthority { get; }
        OrgItem PhaseOfEducation { get; }
        int? StatutoryLowAge { get; }
        int? StatutoryHighAge { get; }
        int? LegacyId { get; }
        int? CompanyRegistrationNumber { get; }
    }
    public interface IOrgItem
    {
        string Id { get; }
        string Name { get; }
        int Code { get; }
    }

}
