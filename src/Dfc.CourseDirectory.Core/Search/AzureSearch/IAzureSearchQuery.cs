using Azure.Search.Documents;

namespace Dfc.CourseDirectory.Core.Search.AzureSearch
{
    public interface IAzureSearchQuery
    {
        (string SearchText, SearchOptions Options) GenerateSearchQuery();
    }
}