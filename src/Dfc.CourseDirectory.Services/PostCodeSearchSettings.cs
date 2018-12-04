using Dfc.CourseDirectory.Services.Interfaces;

namespace Dfc.CourseDirectory.Services
{
    public class PostCodeSearchSettings : IPostCodeSearchSettings
    {
        public string FindAddressesBaseUrl { get; set; }
        public string RetrieveAddressBaseUrl { get; set; }
        public string Key { get; set; }
    }
}