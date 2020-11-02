using Dfc.CourseDirectory.Services.Models.Onspd;

namespace Dfc.CourseDirectory.Web.Helpers
{
    public interface IOnspdSearchHelper
    {
        Onspd GetOnsPostcodeData(string postcode);
    }
}
