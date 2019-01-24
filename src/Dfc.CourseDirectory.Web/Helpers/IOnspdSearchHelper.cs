using Dfc.CourseDirectory.Models.Models.Onspd;

namespace Dfc.CourseDirectory.Web.Helpers
{
    public interface IOnspdSearchHelper
    {
        Onspd GetOnsPostcodeData(string postcode);
    }
}
