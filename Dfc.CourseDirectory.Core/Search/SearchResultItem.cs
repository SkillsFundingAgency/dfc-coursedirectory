namespace Dfc.CourseDirectory.Core.Search
{
    public class SearchResultItem<TResult>
    {
        public TResult Record { get; set; }
        public double? Score { get; set; }
    }
}
