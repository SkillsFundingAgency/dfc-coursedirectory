using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.Providerportal.FindAnApprenticeship.Interfaces.Models
{
    public interface IBulkUploadError
    {
        int LineNumber { get; }
        string Header { get; }
        string Error { get; }
    }
}
