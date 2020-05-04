

CREATE VIEW [dbo].[LARS]
--WITH SCHEMABINDING 
AS
SELECT        ld.LearnAimRef, ld.LearnAimRefTitle, lt.LearnAimRefTypeDesc, ld.NotionalNVQLevelv2, ld.AwardOrgCode, ld.AwardOrgAimRef, ld.LearnDirectClassSystemCode1, ld.LearnDirectClassSystemCode2, ld.SectorSubjectAreaTier1, 
                         SSA1.SectorSubjectAreaTier1Desc, ld.SectorSubjectAreaTier2, SSA2.SectorSubjectAreaTier2Desc, ld.GuidedLearningHours, ld.TotalQualificationTime, ld.UnitType, ac.AwardOrgName, ld.Modified_On, ld.EffectiveTo, 
                         ld.CertificationEndDate
FROM            LARS.LearningDelivery AS ld INNER JOIN
                         LARS.AwardOrgCode AS ac ON ac.AwardOrgCode = ld.AwardOrgCode INNER JOIN
                         LARS.LearnAimRefType AS lt ON lt.LearnAimRefType = ld.LearnAimRefType INNER JOIN
                         LARS.SectorSubjectAreaTier1 AS SSA1 ON SSA1.SectorSubjectAreaTier1 = ld.SectorSubjectAreaTier1 INNER JOIN
                         LARS.SectorSubjectAreaTier2 AS SSA2 ON SSA2.SectorSubjectAreaTier2 = ld.SectorSubjectAreaTier2