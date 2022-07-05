using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace Dfc.CosmosBulkUtils.Features.Base
{
    public abstract class BaseOptions
    {
        [Option("file", Required = true, HelpText = "text file containing document ids")]
        public string Filename { get; set; }
    }
}
