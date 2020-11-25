using System;
using System.Collections.Generic;
using System.Text;
using Dfc.Providerportal.FindAnApprenticeship.Interfaces.Models;

namespace Dfc.Providerportal.FindAnApprenticeship.Models
{
    public class BulkUploadError : IBulkUploadError
    {
        public int LineNumber { get; set; }
        public string Header { get; set; }
        public string Error { get; set; }
    }
}
