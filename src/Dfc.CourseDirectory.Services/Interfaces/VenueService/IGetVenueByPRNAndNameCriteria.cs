
using System;
using System.Collections.Generic;
using System.Text;


namespace Dfc.CourseDirectory.Services.Interfaces.VenueService
{
    public interface IGetVenuesByPRNAndNameCriteria
    {
        string PRN { get; }
        string Name { get; }
    }
}
