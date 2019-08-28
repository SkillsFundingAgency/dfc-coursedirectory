﻿using System.Collections.Generic;
using System.IO;

namespace Dfc.CourseDirectory.Services.Interfaces.BulkUploadService
{
    public interface IApprenticeshipBulkUploadService
    {
        int CountCsvLines(Stream stream);
        List<string> ValidateCSVFormat(Stream stream);
    }
}
