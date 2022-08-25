using System;
using System.Collections.Generic;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class UpsertLarsLearningDeliveries : ISqlQuery<None>
    {
        public IEnumerable<UpsertLarsLearningDeliveriesRecord> Records { get; set; }
    }

    public class UpsertLarsLearningDeliveriesRecord
    {
        public string LearnAimRef { get; set; }
        public string EffectiveFrom { get; set; }
        public string LearnAimRefTitle { get; set; }
        public string LearnAimRefType { get; set; }
        public string NotionalNVQLevel { get; set; }
        public string NotionalNVQLevelv2 { get; set; }
        public string AwardOrgAimRef { get; set; }
        public string OperationalStartDate { get; set; }
        public DateTime? OperationalEndDate { get; set; }
        public string EnglandFEHEStatus { get; set; }
        public string CreditBasedFwkType { get; set; }
        public string QltyAssAgencyType { get; set; }
        public string OfQualGlhMin { get; set; }
        public string OfQualGlhMax { get; set; }
        public string FrameworkCommonComponent { get; set; }
        public string EntrySubLevel { get; set; }
        public string SuccessRateMapCode { get; set; }
        public string EnglPrscID { get; set; }
        public string AwardOrgCode { get; set; }
        public string UnitType { get; set; }
        public string LearningDeliveryGenre { get; set; }
        public string OfQualOfferedEngland { get; set; }
        public string RgltnStartDate { get; set; }
        public string SourceQualType { get; set; }
        public string SourceSystemRef { get; set; }
        public string SourceURLRef { get; set; }
        public string SourceURLLinkType { get; set; }
        public string OccupationalIndicator { get; set; }
        public string AccessHEIndicator { get; set; }
        public string KeySkillsIndicator { get; set; }
        public string FunctionalSkillsIndicator { get; set; }
        public string GCEIndicator { get; set; }
        public string GCSEIndicator { get; set; }
        public string ASLevelIndicator { get; set; }
        public string A2LevelIndicator { get; set; }
        public string ALevelIndicator { get; set; }
        public string QCFIndicator { get; set; }
        public string QCFDiplomaIndicator { get; set; }
        public string QCFCertificateIndicator { get; set; }
        public string EFACOFType { get; set; }
        public string SFAFundedIndicator { get; set; }
        public string DanceAndDramaIndicator { get; set; }
        public string Note { get; set; }
        public string LearnDirectClassSystemCode1 { get; set; }
        public string LearnDirectClassSystemCode2 { get; set; }
        public string LearnDirectClassSystemCode3 { get; set; }
        public string RegulatedCreditValue { get; set; }
        public string SectorSubjectAreaTier1 { get; set; }
        public string SectorSubjectAreaTier2 { get; set; }
        public string MI_NotionalNVQLevel { get; set; }
        public string MI_NotionalNVQLevelv2 { get; set; }
        public string GuidedLearningHours { get; set; }
        public string TotalQualificationTime { get; set; }
        public string RecognisedHEForOfSFundingPurposes { get; set; }
        public string Created_On { get; set; }
        public string Created_By { get; set; }
        public string Modified_By { get; set; }
        public DateTime? Modified_On { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public DateTime? CertificationEndDate { get; set; }
    }
}
