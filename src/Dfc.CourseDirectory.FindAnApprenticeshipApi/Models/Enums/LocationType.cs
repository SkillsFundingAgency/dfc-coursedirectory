using System.ComponentModel;

namespace Dfc.CourseDirectory.FindAnApprenticeshipApi.Models.Enums
{
    public enum LocationType
    {
        [Description("Undefined")]
        Undefined = 0,
        [Description("Venue")]
        Venue = 1,
        [Description("Region")]
        Region = 2,
        [Description("SubRegion")]
        SubRegion = 3
    }
}
