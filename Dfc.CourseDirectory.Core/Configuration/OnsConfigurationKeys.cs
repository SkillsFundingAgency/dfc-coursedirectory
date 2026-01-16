namespace Dfc.CourseDirectory.Core.Configuration
{
    public class OnsConfigurationKeys
    {
        //ONSPD Query link to retrieve dataset metadata
        public const string OnsQueryLink = "Ons:QueryLink";

        //ONS Download link to be used to download retrieved dataset
        public const string OnsDownloadLink = "Ons:DownloadLink";

        //Stores the name of the download info file stored on the PTTCD Blob storage.
        public const string OnsDownloadInfoBlobFile = "Ons:DownloadInfoBlobFile";
    }
}
