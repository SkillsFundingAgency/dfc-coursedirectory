using Microsoft.Azure.Cosmos;

namespace Dfc.Cosmos.JobProfileContainersToCsv.Data
{
    public class ExportRequest
    {
        public string DatabaseId { get; set; }
        public string ContainerId { get; set; }
        public QueryDefinition Query { get; set; }        
        public string FileName { get; set; }
    }
}
