using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dfc.CosmosBulkUtils.Config
{
    public class CosmosDbSettings
    {
        public static string SectionName => "CosmosDb";
        public string EndpointUrl { get; set; }
        public string AccessKey { get; set; }

        public string ContainerId { get; set; }
        public string DatabaseId { get; set; }
    }
}
