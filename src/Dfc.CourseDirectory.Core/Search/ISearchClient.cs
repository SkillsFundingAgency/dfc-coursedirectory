using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Core.Search
{
    public interface ISearchClient<TResult>
    {
        Task<SearchResult<TResult>> Search<TQuery>(TQuery query)
            where TQuery : ISearchQuery<TResult>;
    }
}