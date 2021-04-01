
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;

namespace Dfc.CourseDirectory.Web.ViewComponents.Apprenticeships
{


    public class DeliveryOption
    {
        public string LocationId { get; set; }

        public string LocationName { get; set; }

        public string PostCode { get; set; }

        public string Delivery { get; set; }
        public string Radius { get; set; }

        public bool? National { get; set; }
        public string[] Regions { get; set; }
        public Venue Venue { get; set; }

    }
}
