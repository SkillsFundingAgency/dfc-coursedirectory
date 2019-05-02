using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Interfaces.Apprenticeships;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Models.Apprenticeships
{
    public class ProgType : IProgType
    {
        public Guid id { get; set; } // Cosmos DB id

        public int ProgTypeId { get; set; }
        public string ProgTypeDesc { get; set; }
        public string ProgTypeDesc2 { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }

        // Standard auditing properties 
        public RecordStatus RecordStatus { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }
    }
}
