namespace Dfc.CourseDirectory.Services.Interfaces
{
    public interface ILarsSearchResultItem
    {
        decimal SearchScore { get; }
        string LearnAimRef { get; }
        string LearnAimRefTitle { get; }
        string NotionalNVQLevelv2 { get; }
        string AwardOrgCode { get; }
        string LearnDirectClassSystemCode1 { get; }
        string LearnDirectClassSystemCode2 { get; }
        //string SectorSubjectAreaTier1 { get; }
        //string SectorSubjectAreaTier2 { get; }
        string GuidedLearningHours { get; }
        string TotalQualificationTime { get; }
        string UnitType { get; }
        string AwardOrgName { get; }
        string SectorSubjectAreaTier1Desc { get; }
        string SectorSubjectAreaTier2Desc { get; }
        string AwardOrgAimRef { get; }
    }
}

/*

    {
            "@search.score": 0.46396017,

            "LearnAimRef": "Z0008964",

            "LearnAimRefTitle": "Non regulated SFA formula funded provision, Level 4, Business Management, 3 to 4 hrs, PW A",

            "NotionalNVQLevelv2": "4",

            "AwardOrgCode": "NONE",

            "LearnDirectClassSystemCode1": "NUL",

            "LearnDirectClassSystemCode2": "NUL",

            "SectorSubjectAreaTier1": "15",

            "SectorSubjectAreaTier2": "15.3",

            "GuidedLearningHours": null,

            "TotalQualificationTime": null,

            "UnitType": "CLASS CODE",

            "AwardOrgName": "Generic award - no awarding body"
        }

*/