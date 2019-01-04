using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Models.Models.Venues;


namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.Line2
{
    public class Line2Model
    {
        public string CourseNameLabel { get; set; }
        public string CourseName { get; set; }

        public string CourseNameAriaDescribedBy { get; set; }

        public string IdLabel { get; set; }
        public string Id { get; set; }

        public string IdAriaDescribedBy { get; set; }

        public string DeliveryLabel { get; set; }
        public string Delivery { get; set; }

        public string DeliveryAriaDescribedBy { get; set; }

        public string StartDateLabel { get; set; }
        public string StartDate { get; set; }

        public string StartDateAriaDescribedBy { get; set; }

        public string VenueLabel { get; set; }
        public string Venue { get; set; }

        public string VenueAriaDescribedBy { get; set; }

        public string DeliveryType { get; set; }


        public string UrlLabel { get; set; }
        public string Url { get; set; }

        public string UrlAriaDescribedBy { get; set; }

        public string CostLabel { get; set; }
        public string Cost { get; set; }

        public string CostAriaDescribedBy { get; set; }

        public string CostDetailLabel { get; set; }
        public string CostDetail { get; set; }

        public string CostDetailAriaDescribedBy { get; set; }



        public DeliveryType DeliveryTypes { get; set; }
        public List<Venue> Venues { get; set; }


    }
}
