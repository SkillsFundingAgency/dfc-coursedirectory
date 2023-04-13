using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Dfc.CosmosBulkUtils.Utils
{
    public static class Extensions
    {
        public static bool IsSuccessStatusCode(this HttpStatusCode statusCode)
            => (int)statusCode >= 200 && (int)statusCode <= 299;
    }
}
