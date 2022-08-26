using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dfc.CosmosBulkUtils.Config
{
    public class PatchConfig
    {
        public string FilterPredicate { get; set; }
        public IEnumerable<PatchCommand> Operations { get; set; }

    }
}
