﻿using Dfc.CourseDirectory.Web.ViewComponents.Shared;

namespace Dfc.CourseDirectory.Web.RequestModels
{
    public class EditVenueRequestModel
    {
        public string VenueName { get; set; }
        public string PostcodeId { get; set; }
        public AddressModel Address { get; set; }
    }
}
