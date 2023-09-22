namespace Dfc.Cosmos.JobProfileContainersToCsv.Config
{
    internal class CosmosDbSettings
    {
        public static string SectionName => "CosmosDb";
        public string EndpointUrl { get; set; }
        public string AccessKey { get; set; }
        public List<ContainerSettings> Containers { get; set; }
        public string DatabaseId { get; set; }        
        public string Filename { get; set; }
        public string Key { get; set; }
    }
}
