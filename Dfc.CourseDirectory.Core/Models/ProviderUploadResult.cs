using System.ComponentModel;

namespace Dfc.CourseDirectory.Core.Models
{
    public enum ProviderUploadResult
    {
        [Description("New Provider")]
        NewProvider = 1,

        [Description("Change to CD Provider Status ")]
        ProviderStatusUpdated = 2,

        [Description("Change to Provider Type")]
        ProviderTypeUpdated = 3,

        [Description("Change to CD Provider Status and Type")]
        ProviderStatusAndTypeUpdated = 4,

    }
}
