using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using Dfc.CosmosBulkUtils.Config;

namespace Dfc.CosmosBulkUtils.Features.Touch
{
    [Verb("touch", HelpText = "Touch the list of id's from the cosmosdb collection to force a sync to SQL")]
    public class TouchOptions :CmdOptions
    {
    }
}
