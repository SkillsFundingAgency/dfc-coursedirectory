using System.ComponentModel.DataAnnotations;

namespace Dfc.CourseDirectory.Services.Models.Apprenticeships
{
    public class CsvApprenticeship
    {
        [Display(Name = "STANDARD_CODE")]
        public string StandardCode { get; set; }

        [Display(Name = "STANDARD_VERSION")]
        public string Version { get; set; }

        [Display(Name = "FRAMEWORK_CODE")]
        public string FrameworkCode { get; set; }

        [Display(Name = "FRAMEWORK_PROG_TYPE")]
        public string ProgType { get; set; }

        [Display(Name = "FRAMEWORK_PATHWAY_CODE")]
        public string PathwayCode { get; set; }

        [Display(Name = "APPRENTICESHIP_INFORMATION")]
        public string ApprenticeshipInformation { get; set; }

        [Display(Name = "APPRENTICESHIP_WEBPAGE")]
        public string ApprenticeshipWebpage { get; set; }

        [Display(Name = "CONTACT_EMAIL")]
        public string ContactEmail { get; set; }

        [Display(Name = "CONTACT_PHONE")]
        public string ContactPhone { get; set; }

        [Display(Name = "CONTACT_URL")]
        public string ContactURL { get; set; }

        [Display(Name = "DELIVERY_METHOD")]
        public string DeliveryMethod { get; set; }

        [Display(Name = "VENUE")]
        public string Venue { get; set; }

        [Display(Name = "RADIUS")]
        public string Radius { get; set; }

        [Display(Name = "DELIVERY_MODE")]
        public string DeliveryMode { get; set; }

        [Display(Name = "ACROSS_ENGLAND")]
        public string AcrossEngland { get; set; }

        [Display(Name = " NATIONAL_DELIVERY")]
        public string NationalDelivery { get; set; }

        [Display(Name = "REGION")]
        public string Region { get; set; }

        [Display(Name = "SUB_REGION")]
        public string Subregion { get; set; }
    }
}
