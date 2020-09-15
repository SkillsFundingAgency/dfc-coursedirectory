using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Core.Search
{
    public interface ISearchClient<TQuery, TResult>
    {
        Task<SearchResult<TResult>> Search(TQuery query);
    }
}