using Dfc.Providerportal.FindAnApprenticeship.Models.Enums;
using System;

namespace Dfc.Providerportal.FindAnApprenticeship.Interfaces.Models
{
    public interface IStandardsAndFrameworks
    {

        //Standard Model
        int? StandardCode { get; set; }
        string Version { get; set; }
        string StandardName { get; set; }
        string StandardSectorCode { get; set; }
        string URLLink { get; set; }
        string NotionalEndLevel { get; set; }
        string OtherBodyApprovalRequired  { get; set; }

        //Generic
        ApprenticeshipType ApprenticeshipType { get; set; }
        Guid Id { get; set; } // Cosmos DB id
        DateTime EffectiveFrom { get; set; }
        DateTime? CreatedDateTimeUtc { get; set; }
        DateTime? ModifiedDateTimeUtc { get; set; }
        int? RecordStatusId { get; set; }
        bool AlreadyCreated { get; set; }

        //Framework Model
        int? FrameworkCode { get; set; }
        int? ProgType { get; set; }
        int? PathwayCode { get; set; }
        string PathwayName { get; set; }
        string NasTitle { get; set; }
        DateTime EffectiveTo { get; set; }
        string SectorSubjectAreaTier1 { get; set; }
        string SectorSubjectAreaTier2 { get; set; }
        string ProgTypeDesc { get; set; }
        string ProgTypeDesc2 { get; set; }

    }
}
