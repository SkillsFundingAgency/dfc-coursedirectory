
public class Rootobject
{
    public string odatacontext { get; set; }
    public int odatacount { get; set; }
    public SearchFacets searchfacets { get; set; }
    public Value[] value { get; set; }
}

public class SearchFacets
{
    public string NotionalNVQLevelv2odatatype { get; set; }
    public Notionalnvqlevelv2[] NotionalNVQLevelv2 { get; set; }
    public string AwardOrgCodeodatatype { get; set; }
    public Awardorgcode[] AwardOrgCode { get; set; }
}

public class Notionalnvqlevelv2
{
    public int count { get; set; }
    public string value { get; set; }
}

public class Awardorgcode
{
    public int count { get; set; }
    public string value { get; set; }
}

public class Value
{
    public float searchscore { get; set; }
    public string LearnAimRef { get; set; }
    public string LearnAimRefTitle { get; set; }
    public string NotionalNVQLevelv2 { get; set; }
    public string AwardOrgCode { get; set; }
    public string LearnDirectClassSystemCode1 { get; set; }
    public string LearnDirectClassSystemCode2 { get; set; }
    public string SectorSubjectAreaTier1 { get; set; }
    public string SectorSubjectAreaTier2 { get; set; }
    public object GuidedLearningHours { get; set; }
    public object TotalQualificationTime { get; set; }
    public string UnitType { get; set; }
    public string AwardOrgName { get; set; }
}
