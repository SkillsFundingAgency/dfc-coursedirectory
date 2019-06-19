using System.ComponentModel.DataAnnotations;

namespace Dfc.CourseDirectory.Web.ViewComponents.ManualAddress
{
    public class ManualAddressModel
    {
        public string  Id { get; set; }


        public string AddressLine1 { get; set; }
        public string AddressLine1LabelText { get; set; }
        public string AddressLine1HintText { get; set; }
        public string AddressLine1AriaDescribedBy { get; set; }


        public string AddressLine2 { get; set; }
        public string AddressLine2LabelText { get; set; }
        public string AddressLine2HintText { get; set; }
        public string AddressLine2AriaDescribedBy { get; set; }


        public string TownOrCity { get; set; }
        public string TownOrCityLabelText { get; set; }
        public string TownOrCityHintText { get; set; }
        public string TownOrCityAriaDescribedBy { get; set; }


        public string County { get; set; }
        public string CountyLabelText { get; set; }
        public string CountyHintText { get; set; }
        public string CountyAriaDescribedBy { get; set; }

              public string Postcode { get; set; }
        public string PostcodeLabelText { get; set; }
        public string PostcodeHintText { get; set; }
        public string PostcodeAriaDescribedBy { get; set; }
    }
}