namespace Dfc.CourseDirectory.Services.Interfaces
{
    public interface IPostCodeSearchSettings
    {
        string FindAddressesBaseUrl { get; set; }
        string RetrieveAddressBaseUrl { get; set; }
        string Key { get; }
    }
}