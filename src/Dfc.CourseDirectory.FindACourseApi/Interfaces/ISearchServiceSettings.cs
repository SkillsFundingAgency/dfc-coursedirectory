
using System;


namespace Dfc.CourseDirectory.FindACourseApi.Interfaces
{
    public interface ISearchServiceSettings
    {
        string SearchService { get; }
        string ApiUrl { get; }
        string ProviderApiUrl { get; }
        string LARSApiUrl { get; }
        string ONSPDApiUrl { get; }
        string ApiVersion { get; }
        string QueryKey { get; }
        string AdminKey { get; }
        string Index { get; }
        string onspdIndex { get; }
        int DefaultTop { get; }
        int MaxTop { get; }
        string RegionBoostScoringProfile { get; }
        int ThresholdVenueCount { get; }
    }
}
