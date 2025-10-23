using System.Linq;
using Dfc.CourseDirectory.Services.Models.Regions;

namespace Dfc.CourseDirectory.Services.CourseService
{
    public class CourseService : ICourseService
    {
        public SelectRegionModel GetRegions()
        {
            var selectRegion = new SelectRegionModel
            {
                LabelText = "Where in England can you deliver this course?",
                HintText = "Select all regions and areas that apply.",
                AriaDescribedBy = "Select all that apply."
            };

            if (selectRegion.RegionItems != null && selectRegion.RegionItems.Any())
            {
                selectRegion.RegionItems = selectRegion.RegionItems.OrderBy(x => x.RegionName);
                foreach (var selectRegionRegionItem in selectRegion.RegionItems)
                {
                    selectRegionRegionItem.SubRegion = selectRegionRegionItem.SubRegion.OrderBy(x => x.SubRegionName).ToList();
                }
            }

            return selectRegion;
        }
    }
}
