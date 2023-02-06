using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dfc.CosmosBulkUtils.Features.Patch
{
    public interface IPatchService
    {
        Task<int> Execute(PatchOptions options);
    }
}
