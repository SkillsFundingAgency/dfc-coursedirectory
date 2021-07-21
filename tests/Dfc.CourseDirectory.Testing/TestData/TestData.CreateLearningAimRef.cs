using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Testing
{
    public partial class TestData
    {
        public Task<string> CreateLearningAimRef(DateTime? expiredLars = null
            ) => WithSqlQueryDispatcher(async dispatcher =>
            {
                var learnAimRef = new Random().Next(100000, 109999).ToString("D8");

                await dispatcher.ExecuteQuery(new UpsertLarsLearningDeliveries()
                {
                    Records = new[]
                    {
                    new UpsertLarsLearningDeliveriesRecord()
                    {
                        LearnAimRef = learnAimRef,

                        // Unused columns that have `not null` constraints :-/
                        SourceURLLinkType = string.Empty,
                        RgltnStartDate = string.Empty,
                        OperationalEndDate = string.Empty,
                        NotionalNVQLevelv2 = string.Empty,
                        LearnDirectClassSystemCode1 = string.Empty,
                        KeySkillsIndicator = string.Empty,
                        Created_On = string.Empty,
                        Created_By = string.Empty,
                        A2LevelIndicator = string.Empty,
                        TotalQualificationTime = string.Empty,
                        SourceURLRef = string.Empty,
                        SourceSystemRef = string.Empty,
                        SectorSubjectAreaTier1 = string.Empty,
                        SectorSubjectAreaTier2 = string.Empty,
                        RegulatedCreditValue = string.Empty,
                        QCFCertificateIndicator = string.Empty,
                        Modified_By = string.Empty,
                        LearningDeliveryGenre = string.Empty,
                        LearnDirectClassSystemCode2 = string.Empty,
                        FrameworkCommonComponent = string.Empty,
                        AccessHEIndicator = string.Empty,
                        ALevelIndicator = string.Empty,
                        ASLevelIndicator = string.Empty,
                        AwardOrgAimRef = string.Empty,
                        AwardOrgCode = string.Empty,
                        CreditBasedFwkType = string.Empty,
                        DanceAndDramaIndicator = string.Empty,
                        EFACOFType = string.Empty,
                        EffectiveFrom = string.Empty,
                        EnglandFEHEStatus = string.Empty,
                        EnglPrscID = string.Empty,
                        EntrySubLevel = string.Empty,
                        FunctionalSkillsIndicator = string.Empty,
                        GCEIndicator = string.Empty,
                        GCSEIndicator = string.Empty,
                        GuidedLearningHours = string.Empty,
                        LearnAimRefTitle = string.Empty,
                        LearnAimRefType = string.Empty,
                        LearnDirectClassSystemCode3 = string.Empty,
                        MI_NotionalNVQLevel = string.Empty,
                        MI_NotionalNVQLevelv2 = string.Empty,
                        Note  = string.Empty,
                        NotionalNVQLevel = string.Empty,
                        OccupationalIndicator = string.Empty,
                        OfQualGlhMax = string.Empty,
                        OfQualGlhMin = string.Empty,
                        OfQualOfferedEngland = string.Empty,
                        OperationalStartDate = string.Empty,
                        QCFDiplomaIndicator = string.Empty,
                        QCFIndicator = string.Empty,
                        QltyAssAgencyType = string.Empty,
                        SFAFundedIndicator = string.Empty,
                        SourceQualType = string.Empty,
                        SuccessRateMapCode = string.Empty,
                        UnitType = string.Empty,
                        EffectiveTo = expiredLars
                    }
                }
                });

                return learnAimRef;
            });
    }
}
