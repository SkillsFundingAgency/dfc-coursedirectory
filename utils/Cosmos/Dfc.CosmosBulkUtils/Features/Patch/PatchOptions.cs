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
        [Option('f', "file", Required = true, HelpText = "text file containing JSON config for patch operation")]
        public string Filename { get; set; }
    }
}
