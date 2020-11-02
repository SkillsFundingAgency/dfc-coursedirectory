using System.Collections.Generic;

namespace Dfc.CourseDirectory.Models.Models.Apprenticeships
{
    public class SelectDeliveryModeItems
    {
        public IEnumerable<DeliveryModeItem> DeliveryModeItems { get; set; }
        public SelectDeliveryModeItems()
        {
            DeliveryModeItems = new[]
            {
                new DeliveryModeItem
                {
                    DeliveryModeId = 1,
                    DeliveryModeName = "100% Employer Based",
                    DeliveryModeDescription = "Entirely delivered at employer's premises",
                    DASRef = "100PercentEmployer",
                    MustHaveFullLocation = false
                },
                new DeliveryModeItem
                {
                    DeliveryModeId = 2,
                    DeliveryModeName = "Day Release",
                    DeliveryModeDescription = "Apprentices given days off to study",
                    DASRef = "DayRelease",
                    MustHaveFullLocation = false
                },
                new DeliveryModeItem
                {
                    DeliveryModeId = 3,
                    DeliveryModeName = "Block Release",
                    DeliveryModeDescription = "Apprentices given blocks of time off to study",
                    DASRef = "BlockRelease",
                    MustHaveFullLocation = false
                }
            };

        }
    }
}
