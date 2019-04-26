using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Models.Apprenticeships
{
    public class DeliveryModeItem
    {
        public int DeliveryModeId { get; set; }
        public string DeliveryModeName { get; set; }
        public string DeliveryModeDescription { get; set; }
        //public string BulkUploadRef { get; set; } // Not Used
        public string DASRef { get; set; }
        //RecordStatusId - Not used 
        public bool MustHaveFullLocation { get; set; }
    }
}
