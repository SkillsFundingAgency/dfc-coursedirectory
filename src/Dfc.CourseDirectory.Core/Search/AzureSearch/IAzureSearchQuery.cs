using Azure.Search.Documents;

namespace Dfc.CourseDirectory.Core.Search.AzureSearch
{
    public interface IAzureSearchQuery<TResult> : ISearchQuery<TResult>
    {
        (string SearchText, SearchOptions Options) GenerateSearchQuery();
    }
}