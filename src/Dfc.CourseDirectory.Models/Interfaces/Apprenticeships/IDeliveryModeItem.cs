using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Interfaces.Apprenticeships
{
    public interface IDeliveryModeItem
    {
        int DeliveryModeId { get; set; }
        string DeliveryModeName { get; set; }
        string DeliveryModeDescription { get; set; }
        //public string BulkUploadRef { get; set; } // Not Used
        string DASRef { get; set; }
        //RecordStatusId - Not used 
        bool MustHaveFullLocation { get; set; }
    }
}
