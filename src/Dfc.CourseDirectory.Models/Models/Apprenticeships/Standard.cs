using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Models.Enums;

namespace Dfc.CourseDirectory.Models.Models.Apprenticeships
{
    public class Standard
    {
        public Guid? id { get; set; } 
        public Contact Contact { get; set; }
        public List<LocationRef> Locations { get; set; }
        public string MarketingInfo { get; set; }
        public string StandardInfoUrl { get; set; }
        public int? StandardCode { get; set; }
        public int? Version { get; set; }
        public string StandardName { get; set; }
        public string StandardSectorCode { get; set; }
        public Guid? StandardSectorCodeId { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public string URLLink { get; set; }
        public decimal? SectorSubjectAreaTier1 { get; set; }
        public Guid? SectorSubjectAreaTier1Id { get; set; }
        public decimal? SectorSubjectAreaTier2 { get; set; }
        public Guid? SectorSubjectAreaTier2Id { get; set; }
        public string NotionalNVQLevelv2 { get; set; }
        public string OtherBodyApprovalRequired { get; set; }
        public RecordStatus RecordStatus { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }
    }
}
