using System;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Web.ViewComponents.DashboardCard
{
    public class DashboardCardModel
    {
        public class Link
        {
            public string Href { get; set; }
            public string Description { get; set; }

            public Link(string href, string description)
            {
                Href = href;
                Description = description;
            }
        }

        public string Title { get; set; }
        public int Value { get; set; }
        public string Description { get; set; }
        public string ValueHref { get; set; }
        public List<Link> Links { get; set; }

        public DashboardCardModel()
        {
            Links = new List<Link>();
        }
    }
}
