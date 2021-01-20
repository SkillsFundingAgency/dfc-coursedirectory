using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Dfc.CourseDirectory.FindAnApprenticeshipApi.Models.Enums
{
    public enum ApprenticeshipLocationType
    {
        [Description("Undefined")]
        Undefined = 0,
        [Description("Classroom based")] // Venue
        ClassroomBased = 1,
        [Description("Employer based")] // Region
        EmployerBased = 2,
        [Description("Classroom based and employer based")] // Venue with added 
        ClassroomBasedAndEmployerBased = 3
    }
}
