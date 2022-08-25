using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using Dfc.CosmosBulkUtils.Config;

namespace Dfc.CosmosBulkUtils.Features.Patch
{
    [Verb("patch", HelpText = "Patch list of items based on query")]
    public class PatchOptions : CmdOptions
    {
        [Option('q', "query", Required = true, HelpText = "query used to select items to patch")]
        public string Query { get; set; }
    }
}
