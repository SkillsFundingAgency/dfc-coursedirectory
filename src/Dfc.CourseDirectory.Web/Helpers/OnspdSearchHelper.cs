using Dfc.CourseDirectory.Models.Models.Onspd;
using Dfc.CourseDirectory.Services.Interfaces.OnspdService;
using Dfc.CourseDirectory.Services.OnspdService;

namespace Dfc.CourseDirectory.Web.Helpers
{
    public class OnspdSearchHelper : IOnspdSearchHelper
    {
        private readonly IOnspdService _onspdService;

        public OnspdSearchHelper(IOnspdService onspdService)
        {
            _onspdService = onspdService;
        }

        public Onspd GetOnsPostcodeData(string postcode)
        {
            var onspd = new Onspd();
            if (!string.IsNullOrWhiteSpace(postcode))
            {
                var onspdSearchCriteria = new OnspdSearchCriteria(postcode);
                var onspdResult = _onspdService.GetOnspdData(onspdSearchCriteria);
                if (onspdResult.IsSuccess && onspdResult.HasValue)
                {
                    onspd = onspdResult.Value.Value;
                }
            }

            return onspd;
        }
    }
}
