
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Dfc.CourseDirectory.FindACourseApi.Interfaces;


namespace Dfc.CourseDirectory.FindACourseApi.Models
{
    public enum VenueStatus
    {
        Undefined = 0,
        Live = 1,
        Deleted = 2,
        Pending = 3,
        Uknown = 99
    }

    public class Venue
    {
        public Guid id { get; set; }
        public int UKPRN { get; set; }
        public int PROVIDER_ID { get; set; }
        public int VENUE_ID { get; set; }
        public string VENUE_NAME { get; set; }
        public string PROV_VENUE_ID { get; set; }
        public string PHONE { get; set; }
        public string ADDRESS_1 { get; set; }
        public string ADDRESS_2 { get; set; }
        public string TOWN { get; set; }
        public string COUNTY { get; set; }
        public string POSTCODE { get; set; }
        public string EMAIL { get; set; }
        public string WEBSITE { get; set; }
        //public string FAX { get; set; }
        //public string FACILITIES { get; set; }
        //public string DATE_CREATED { get; set; }
        //public string DATE_UPDATE { get; set; }
        //public string STATUS { get; set; }
        //public string UPDATED_BY { get; set; }
        //public string CREATED_BY { get; set; }
        //public string X_COORD { get; set; }
        //public string Y_COORD { get; set; }
        //public string SEARCH_REGION { get; set; }
        //public string SYS_DATA_SOURCE { get; set; }
        //public string DATE_UPDATED_COPY_OVER { get; set; }
        //public string DATE_CREATED_COPY_OVER { get; set; }

        public VenueStatus Status { get; set; }
        public DateTime DateUpdated { get; set; }
        public string UpdatedBy { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        // Apprenticeship related
        // Except for LocationId, these fields are duplicates of above PHONE, EMAIL and WEBSITE
        // fields from the up_VenueListForCsvExport procedure in Tribal DB
        // Do not use fields below, they will soon be replaced with the above
        public int? LocationId { get; set; }
        public int? TribalLocationId { get; set; }
        public string Telephone { get { return PHONE; } set { PHONE = value; } }
        public string Email { get { return EMAIL; } set { EMAIL = value; } }
        public string Website { get { return WEBSITE; } set { WEBSITE = value; } }
    }
}