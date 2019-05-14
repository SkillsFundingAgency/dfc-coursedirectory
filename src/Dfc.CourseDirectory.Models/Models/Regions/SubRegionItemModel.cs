
using System.Linq;


namespace Dfc.CourseDirectory.Models.Models.Regions
{
    public enum SearchResultWeightings
    {
        Low,
        Medium,
        High
    }

    public class SubRegionItemModel
    {
        public string Id { get; set; }
        public int? ApiLocationId { get; set; }
        public string SubRegionName { get; set; }
        public bool? Checked { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public SearchResultWeightings Weighting { get { return CalcWeighting(); } }

        // Subregions also used to store regions for azure search index, so calculate search weighting based on whether it's a region or subregion
        private SearchResultWeightings CalcWeighting()
        {
            if (new SelectRegionModel().RegionItems.Any(r => r.Id == Id))
                return SearchResultWeightings.Medium;
            if (new SelectRegionModel().RegionItems.SelectMany(r => r.SubRegion).Any(r => r.Id == Id))
                return SearchResultWeightings.High;
            return SearchResultWeightings.Low;
        }
    }
}
