using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Testing
{
    public partial class TestData
    {
        public Task<LearningDelivery> CreateLearningDelivery(
                string learnAimRef = null,
                string learnAimRefTitle = "National Certificate in Building Studies",
                DateTime? effectiveTo = null,
                string notionalNVQLevelv2 = "3",
                string awardOrgCode = "EDEXCEL",
                string learnAimRefTypeDesc = "National Certificate",
                DateTime? operationalEndDate = null) => 
            WithSqlQueryDispatcher(async dispatcher =>
            {
                learnAimRef ??= new Random().Next(100000, 109999).ToString("D8");
                var learnAimRefType = Guid.NewGuid().ToString();

                await dispatcher.ExecuteQuery(new UpsertLarsLearnAimRefTypes()
                {
                    Records = new[]
                    {
                        new UpsertLarsLearnAimRefTypesRecord()
                        {
                            LearnAimRefType = learnAimRefType,
                            LearnAimRefTypeDesc = learnAimRefTypeDesc,

                            // Unused columns that have `not null` constraints :-/
                            EffectiveFrom = string.Empty,
                            EffectiveTo = string.Empty,
                            LearnAimRefTypeDesc2 = string.Empty
                        }
                    }
                });

                await dispatcher.ExecuteQuery(new UpsertLarsLearningDeliveries()
                {
                    Records = new[]
                    {
                        new UpsertLarsLearningDeliveriesRecord()
                        {
                            LearnAimRef = learnAimRef,
                            NotionalNVQLevelv2 = notionalNVQLevelv2,
                            AwardOrgCode = awardOrgCode,
                            LearnAimRefType = learnAimRefType,
                            LearnAimRefTitle = learnAimRefTitle,

                            // Unused columns that have `not null` constraints :-/
                            SourceURLLinkType = string.Empty,
                            RgltnStartDate = string.Empty,
                            OperationalEndDate = operationalEndDate,
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
                            EffectiveTo = effectiveTo
                        }
                    }
                });

                return new LearningDelivery()
                {
                    LearnAimRef = learnAimRef,
                    LearnAimRefTitle = learnAimRefTitle,
                    EffectiveTo = effectiveTo,
                    NotionalNVQLevelv2 = notionalNVQLevelv2,
                    AwardOrgCode = awardOrgCode,
                    LearnAimRefTypeDesc = learnAimRefTypeDesc,
                    OperationalEndDate = operationalEndDate
                };
            });
    }
}
