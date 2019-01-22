namespace Dfc.CourseDirectory.Services.Interfaces.OnspdService
{
    public interface IOnspdSearchSettings
    {
        string SearchServiceName { get; }
        string SearchServiceAdminApiKey { get; }
        string SearchServiceQueryApiKey { get; }
        string IndexName { get; }
    }
}
