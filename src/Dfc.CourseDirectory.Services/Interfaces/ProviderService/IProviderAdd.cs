using System;

namespace Dfc.CourseDirectory.Services.Interfaces.ProviderService
{
    public interface IProviderAdd
    {
        Guid id { get; set; }
        int Status { get; set; }
        string UpdatedBy { get; set; }
    }
}
