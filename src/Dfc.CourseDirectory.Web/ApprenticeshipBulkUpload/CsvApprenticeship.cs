using System.ComponentModel.DataAnnotations;
using CsvHelper.Configuration.Attributes;

namespace Dfc.CourseDirectory.Web.ApprenticeshipBulkUpload
{
    public class CsvApprenticeship
    {
        [Display(Name = "STANDARD_CODE")]
        [Name("STANDARD_CODE")]
        public string StandardCode { get; set; }

        [Display(Name = "STANDARD_VERSION")]
        [Name("STANDARD_VERSION")]
        public string Version { get; set; }

        [Display(Name = "APPRENTICESHIP_INFORMATION")]
        [Name("APPRENTICESHIP_INFORMATION")]
        public string ApprenticeshipInformation { get; set; }

        [Display(Name = "APPRENTICESHIP_WEBPAGE")]
        [Name("APPRENTICESHIP_WEBPAGE")]
        public string ApprenticeshipWebpage { get; set; }

        [Display(Name = "CONTACT_EMAIL")]
        [Name("CONTACT_EMAIL")]
        public string ContactEmail { get; set; }

        [Display(Name = "CONTACT_PHONE")]
        [Name("CONTACT_PHONE")]
        public string ContactPhone { get; set; }

        [Display(Name = "CONTACT_URL")]
        [Name("CONTACT_URL")]
        public string ContactURL { get; set; }

        [Display(Name = "DELIVERY_METHOD")]
        [Name("DELIVERY_METHOD")]
        public string DeliveryMethod { get; set; }

        [Display(Name = "VENUE")]
        [Name("VENUE")]
        public string Venue { get; set; }

        [Display(Name = "RADIUS")]
        [Name("RADIUS")]
        public string Radius { get; set; }

        [Display(Name = "DELIVERY_MODE")]
        [Name("DELIVERY_MODE")]
        public string DeliveryMode { get; set; }

        [Display(Name = "ACROSS_ENGLAND")]
        [Name("ACROSS_ENGLAND")]
        public string AcrossEngland { get; set; }

        [Display(Name = "NATIONAL_DELIVERY")]
        [Name("NATIONAL_DELIVERY")]
        public string NationalDelivery { get; set; }

        [Display(Name = "REGION")]
        [Name("REGION")]
        public string Region { get; set; }

        [Display(Name = "SUB_REGION")]
        [Name("SUB_REGION")]
        public string Subregion { get; set; }
    }
}
