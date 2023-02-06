namespace Dfc.CosmosToCsv.Config
{
    internal class CosmosDbSettings
    {
        public static string SectionName => "CosmosDb";
        public string EndpointUrl { get; set; }
        public string AccessKey { get; set; }

        public string ContainerId { get; set; }
        public string DatabaseId { get; set; }

        public string Query { get; set; }
        public string Filename { get; set; }

        public string Key { get; set; }
    }
}
