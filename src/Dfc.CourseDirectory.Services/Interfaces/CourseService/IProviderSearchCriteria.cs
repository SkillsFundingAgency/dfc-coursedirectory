
// TODO - Provider search is in the course service for now, needs moving!
namespace Dfc.CourseDirectory.Services.Interfaces.CourseService
{
    public interface IProviderSearchCriteria
    {
        //public string APIKeyField { get; }
        string Keyword { get; }
        string[] Town { get; }
        string[] Region { get; }

        int? TopResults { get; }
    }
}
