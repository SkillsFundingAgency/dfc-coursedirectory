using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using Dfc.CosmosBulkUtils.Config;

namespace Dfc.CosmosBulkUtils.Features.Add
{
    [Verb("add", HelpText = "Add documents to the collection")]
    public class AddOptions : CmdOptions
    {
        [Option('p', "path", Required = true, HelpText = "folder containing the documents to add")]
        public string Path { get; set; }

        [Option("pkey", Required = false, HelpText = "Optional partition key to be used when inserting document")]
        public string PartitionKey { get; set; }
    }
}
