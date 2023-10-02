namespace Dfc.Cosmos.JobProfileContainersToCsv.Config
{
    public class CosmosDbSettings
    {
        public static string SectionName => "CosmosDb";
        public string EndpointUrl { get; set; }
        public string AccessKey { get; set; }
        public List<ContainerSettings> Containers { get; set; }
        public string DatabaseId { get; set; }       
    }
}
