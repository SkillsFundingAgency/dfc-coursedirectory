using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using Dfc.CosmosBulkUtils.Features.Base;

namespace Dfc.CosmosBulkUtils.Features.Delete
{
    [Verb("delete", HelpText = "Remove the list of id's from the cosmosdb collection")]
    public class DeleteOptions : BaseOptions
    {
    }
}
