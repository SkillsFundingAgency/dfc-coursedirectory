using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.Helpers
{
    public interface ICSVHelper
    {
        string SemiColonSplit(IEnumerable<string> list);
        IEnumerable<string> ToCsv<T>(IEnumerable<T> objectlist, string separator = ",", bool header = true);
        string SanitiseTextForCSVOutput(string text);
        IEnumerable<string> SanitiseRegionTextForCSVOutput(IEnumerable<string> regions);
    }
}
