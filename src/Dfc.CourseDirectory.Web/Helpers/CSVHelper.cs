using Dfc.CourseDirectory.Services.Models.Regions;
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
        private const string Quote = "\"";
        private const string EscapedQuote = "\"\"";
        private static char[] CharactersToQuote = { ',', '"'};
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
                text = Escape(text);
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
        internal static string Escape(string s)
        {
            if (s.Contains(Quote))
                s = s.Replace(Quote, EscapedQuote);
            if (s.IndexOfAny(CharactersToQuote) > -1)
                s = Quote + s + Quote;
            return s;
        }
    }
}
