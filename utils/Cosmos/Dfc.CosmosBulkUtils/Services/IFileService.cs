using System;
using System.Collections.Generic;
using Dfc.CosmosBulkUtils.Config;

namespace Dfc.CosmosBulkUtils.Services
{
    public interface IFileService
    {
        IList<Guid> LoadRecordIds(string filename);
        PatchConfig LoadPatchConfig(string filename);
    }
}
