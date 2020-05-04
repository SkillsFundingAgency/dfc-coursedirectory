using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Models.Apprenticeships;
using Dfc.CourseDirectory.Web.ViewComponents.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dfc.CourseDirectory.Web.ViewComponents.Apprenticeships.ApprenticeshipSearchResult
{
    public class ApprenticeShipsSearchResultItemModel
    {
        public string ApprenticeshipTitle { get; set; }
        public string NotionalNVQLevelv2 { get; set; }
        public Guid id { get; set; }
        public int? StandardCode { get; set; }
        public int? Version { get; set; }
        public string StandardName { get; set; }
        public string StandardSectorCode { get; set; }
        public string URLLink { get; set; }
        public string OtherBodyApprovalRequired { get; set; }

        //Generic
        public ApprenticeshipType ApprenticeshipType { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? CreatedDateTimeUtc { get; set; }
        public DateTime? ModifiedDateTimeUtc { get; set; }
        public int? RecordStatusId { get; set; }
        public bool AlreadyCreated { get; set; }
        //Framework Model
        public int? FrameworkCode { get; set; }
        public int? ProgType { get; set; }
        public int? PathwayCode { get; set; }
        public string PathwayName { get; set; }
        public string NasTitle { get; set; }
        public DateTime EffectiveTo { get; set; }
        public string SectorSubjectAreaTier1 { get; set; }
        public string SectorSubjectAreaTier2 { get; set; }
        public string ProgTypeDesc { get; set; }
        public string ProgTypeDesc2 { get; set; }
        public bool IsFramework => FrameworkCode.HasValue;

    }
}