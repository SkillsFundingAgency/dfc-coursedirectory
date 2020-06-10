namespace Dfc.CourseDirectory.WebV2.Security
{
    public class DfeSignInSettings
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string MetadataAddress { get; set; }
        public string CallbackPath { get; set; }
        public string SignedOutCallbackPath { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string ApiSecret { get; set; }
        public string ApiUri { get; set; }
        public string ApiBaseUri { get; set; }
        public string ServiceId { get; set; }
    }
}
