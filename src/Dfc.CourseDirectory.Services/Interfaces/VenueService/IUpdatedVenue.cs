using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Services.Interfaces.VenueService
{
    public interface IUpdatedVenue
    {
        String ID { get; }
        string VENUE_NAME { get; }
        string ADDRESS_1 { get; }
        string ADDRESS_2 { get; }
        string TOWN { get; }
        string COUNTY { get; }
        string POSTCODE { get; }
    }
}
