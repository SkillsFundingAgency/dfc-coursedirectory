﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Services
{
    public interface IVenueAddResultItem
    {
        string Id { get; }
        //int UKPRN { get; }
        //int ProviderID { get; }
        //int VenueID { get; }
        //string VenueName { get; }
        //string ProvVenueID { get; }
        string Address1 { get; }
        string Address2 { get; }
        string Town { get; }
        string County { get; }
        string PostCode { get; }
    }
}
