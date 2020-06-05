using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Models.Apprenticeships;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Interfaces.Apprenticeships
{
    public interface IStandardsAndFrameworks
    {
        //Standard Model
        int? StandardCode { get; }
        int? Version { get; }
        string StandardName { get; }
        string StandardSectorCode { get; }
        string URLLink { get; }
        string NotionalEndLevel { get; }
        string OtherBodyApprovalRequired { get; }

        //Generic
        ApprenticeshipType ApprenticeshipType { get; }
        Guid id { get; } // Cosmos DB id
        DateTime? EffectiveFrom { get; }
        DateTime? CreatedDateTimeUtc { get; }
        DateTime? ModifiedDateTimeUtc { get; }
        int? RecordStatusId { get; }
        bool AlreadyCreated { get; }

        //Framework Model
        int? FrameworkCode { get; }
        int? ProgType { get; }
        int? PathwayCode { get; }
        string PathwayName { get; }
        string NasTitle { get; }
        DateTime? EffectiveTo { get; }
        string SectorSubjectAreaTier1 { get; }
        string SectorSubjectAreaTier2 { get; }
        string ProgTypeDesc { get; }
        string ProgTypeDesc2 { get; }
    }
}
