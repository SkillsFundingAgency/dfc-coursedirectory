namespace Dfc.CourseDirectory.WebV2.Models
{
    public class Standard
    {
        public string CosmosId { get; set; }
        public int StandardCode { get; set; }
        public int Version { get; set; }
        public string StandardName { get; set; }
        public string NotionalNVQLevelv2 { get; set; }
        public bool OtherBodyApprovalRequired { get; set; }
    }
}
