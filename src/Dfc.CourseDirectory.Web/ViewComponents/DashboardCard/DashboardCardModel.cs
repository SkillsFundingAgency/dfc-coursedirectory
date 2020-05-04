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
            public bool Disabled { get; set; }

            public Link(string href, string description, bool disabled = false)
            {
                Href = href;
                Description = description;
                Disabled = disabled;
            }
        }

        public string Title { get; set; }

        public string Title2 { get; set; }
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
