using System;
using System.Collections.Generic;

namespace Dfc.CosmosBulkUtils.Services
{
    public interface IFileService
    {
        IList<Guid> LoadRecordIds(string filename);
    }
}
