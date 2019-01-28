using Dfc.CourseDirectory.Services.Interfaces.OnspdService;

namespace Dfc.CourseDirectory.Services.OnspdService
{
    public class OnspdSearchSettings : IOnspdSearchSettings
    {
        public string SearchServiceName { get; set; }
        public string SearchServiceAdminApiKey { get; set; }
        public string SearchServiceQueryApiKey { get; set; }
        public string IndexName { get; set; }
    }
}
