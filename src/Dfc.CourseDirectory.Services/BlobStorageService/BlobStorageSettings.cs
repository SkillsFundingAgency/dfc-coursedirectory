﻿
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
        public string ApprenticeshipsTemplatePath { get; set; }
        public int InlineProcessingThreshold { get; set; } // below this number of rows we process the file immediately, above this number we let the Azure trigger pick up the file to process it
    }
}
