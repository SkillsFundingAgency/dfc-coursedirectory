using Dfc.CourseDirectory.Web.ViewComponents.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewComponents.EditVenueName
{

    public class EditVenueNameModel
    {
        public string VenueName { get; set; }
        public string PostcodeId { get; set; }
        public AddressModel Address { get; set; }

        public string Id { get; set; }

    }

}
