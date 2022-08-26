using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace Dfc.CosmosBulkUtils.Utils
{
    public static class TaskHandlers
    {
        public static async Task<(ItemResponse<object>?, Exception?)> CosmosExecuteAndCaptureErrorsAsync(Task<ItemResponse<object>> task)
        {
            try
            {
                return (await task, null);
            }
            catch (Exception ex)
            {

                return (null, ex);
            }
        }
    }
}
