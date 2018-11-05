namespace Dfc.CourseDirectory.Services.Interfaces
{
    public interface ISearchFacet
    {
        int Count { get; }
        string Value { get; }
    }
}