using Dfc.CourseDirectory.Models.Models.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.Helpers
{
    public class CSVHelper : ICSVHelper
    {
        public IEnumerable<string> ToCsv<T>(IEnumerable<T> objectlist, string separator = ",", bool header = true)
        {
            PropertyInfo[] properties = typeof(T).GetProperties();
            if (header)
            {
                var headers = from prop in properties
                              from attr in prop.CustomAttributes
                              from custAttr in attr.NamedArguments
                              select custAttr.TypedValue.Value;

                yield return String.Join(separator, headers);
            }
            foreach (var o in objectlist)
            {

                yield return string.Join(separator, properties.Select(p => (p.GetValue(o, null) ?? "").ToString()));
            }
        }
        public string SemiColonSplit(IEnumerable<string> list)
        {
            return string.Join(";", list.Select(x => SanitiseTextForCSVOutput(x.ToString())).ToArray());
        }

        public string SanitiseTextForCSVOutput(string text)
        {
            if (text.Contains(","))
            {
                text = "\"" + text + "\"";
            }
            text = Regex.Replace(text, @"\t|\n|\r", "");
            return text;
        }
        public IEnumerable<string> SanitiseRegionTextForCSVOutput(IEnumerable<string> regions)
        {
            SelectRegionModel selectRegionModel = new SelectRegionModel();

            foreach (var selectRegionRegionItem in selectRegionModel.RegionItems.OrderBy(x => x.RegionName))
            {
                //If Region is returned, check for existence of any subregions
                if (regions.Contains(selectRegionRegionItem.Id))
                {
                    var subregionsInList = from subRegion in selectRegionRegionItem.SubRegion
                                           where regions.Contains(subRegion.Id)
                                           select subRegion;

                    //If true, then ignore subregions
                    if (subregionsInList.Count() > 0)
                    {
                        foreach (var subRegion in subregionsInList)
                        {
                            regions = regions.Where(x => (x != subRegion.Id)).ToList();

                        }
                    }
                }
            }
            return regions;
        }

    }
}
