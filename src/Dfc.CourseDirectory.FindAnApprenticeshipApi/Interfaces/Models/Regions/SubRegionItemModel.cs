
using System.Linq;


namespace Dfc.Providerportal.FindAnApprenticeship.Interfaces.Models.Regions
{
    public class SubRegionItemModel
    {
        public string Id { get; set; }
        public int? ApiLocationId { get; set; }
        public string SubRegionName { get; set; }
        public bool? Checked { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Postcode { get; set; }
    }
}
