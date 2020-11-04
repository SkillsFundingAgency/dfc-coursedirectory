namespace Dfc.CourseDirectory.FindACourseApi.Interfaces
{
    public interface ISearchServiceSettings
    {
        string SearchService { get; }
        string ApiUrl { get; }
        string ApiVersion { get; }
        string QueryKey { get; }
        string Index { get; }
        string onspdIndex { get; }
        int DefaultTop { get; }
        int MaxTop { get; }
        string RegionBoostScoringProfile { get; }
    }
}
