using Dfc.CourseDirectory.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Interfaces.Apprenticeships
{
    public interface IFEChoice
    {
        Guid id { get; set; } // Cosmos DB id

        int UPIN { get; set; }
        double? LearnerDestination { get; set; }
        double? LearnerSatisfaction { get; set; }
        double? EmployerSatisfaction { get; set; }

        // Standard auditing properties 
        RecordStatus RecordStatus { get; set; }
        DateTime CreatedDate { get; set; }
        string CreatedBy { get; set; }
        DateTime? UpdatedDate { get; set; }
        string UpdatedBy { get; set; }
    }
}
