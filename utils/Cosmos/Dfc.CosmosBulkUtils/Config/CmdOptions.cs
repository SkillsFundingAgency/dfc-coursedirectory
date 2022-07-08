using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace Dfc.CosmosBulkUtils.Config
{
    public class CmdOptions
    {
        [Option('f', "file", Required = true, HelpText = "text file containing document ids")]
        public string Filename { get; set; }
        [Option('u', "url", Required = true, HelpText = "Azure Cosmosdb URL")]
        public string EndpointUrl { get; set; }
        [Option('k', "key", Required = true, HelpText = "Azure Cosmosdb Access Key")]
        public string AccessKey { get; set; }
        [Option('c', "container", Required = true, HelpText = "Azure Container Id")]
        public string ContainerId { get; set; }
        [Option('d', "database", Required = true, HelpText = "Azure Database Id")]
        public string DatabaseId { get; set; }

        [Option('m', "mode", Required = true, HelpText = "Mode of operation, [delete|touch]")]
        public ModeEnum Mode { get; set; }
    }
}
