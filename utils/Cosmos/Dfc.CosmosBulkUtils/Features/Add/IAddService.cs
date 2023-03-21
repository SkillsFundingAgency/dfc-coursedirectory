using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dfc.CosmosBulkUtils.Features.Add
{
    public interface IAddService
    {
        Task<int> Execute(string path, string partitionKey);
    }
}
