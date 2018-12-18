using Dfc.CourseDirectory.Services.Interfaces.ProviderService;


namespace Dfc.CourseDirectory.Services.ProviderService
{
    public class ProviderServiceSettings : IProviderServiceSettings
    {
        public string ApiUrl { get; set; }
        public string ApiKey { get; set; }
    }
}
