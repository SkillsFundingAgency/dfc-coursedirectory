
using System;
using System.Text;
using System.Collections.Generic;
using Dfc.CourseDirectory.Services.Interfaces.BlobStorageService;


namespace Dfc.CourseDirectory.Services.BlobStorageService
{
    public class BlobStorageSettings : IBlobStorageSettings
    {
        public string AccountName { get; set; }
        public string AccountKey { get; set; }
        public string Container { get; set; }
        //public string BulkUploadPathFormat { get; set; }
        public string TemplatePath { get; set; }
    }
}
