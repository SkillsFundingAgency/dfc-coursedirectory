namespace Dfc.Cosmos.JobProfileContainersToCsv.Config
{
    public class AppSettings
    {
        public string OutputFilename { get; set; }
        public string Key { get; set; }
        public CosmosDbSettings CosmosDb { get; set; }
        public SqlDbSettings SqlDb { get; set; }
    }
}
