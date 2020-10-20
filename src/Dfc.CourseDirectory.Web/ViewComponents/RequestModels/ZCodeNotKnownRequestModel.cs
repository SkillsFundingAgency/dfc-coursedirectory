using System;

namespace Dfc.CourseDirectory.Web.RequestModels
{
    public class ZCodeNotKnownRequestModel
    {
        public string Level1Id { get; set; }

        public string Level2Id { get; set; }

        public string[] NotionalNVQLevelv2Filter { get; set; }
        
        public string[] AwardOrgAimRefFilter { get; set; }

        public int PageNo { get; set; }

        public ZCodeNotKnownRequestModel()
        {
            NotionalNVQLevelv2Filter = Array.Empty<string>();
            AwardOrgAimRefFilter = Array.Empty<string>();
            PageNo = 1;
        }
    }
}