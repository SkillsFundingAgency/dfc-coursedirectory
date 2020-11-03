using System;

namespace Dfc.CourseDirectory.Services.ProviderService
{
    public class ProviderAdd
    {
        public Guid id { get; set; }
        public int Status { get; set;  }
        public string UpdatedBy { get; set; }

        public ProviderAdd(
            Guid _id,
        int status,
        string updatedBy)
        {
            Throw.IfNull(_id, nameof(_id));
            Throw.IfNull(status, nameof(status));
            Throw.IfNullOrWhiteSpace(updatedBy, nameof(updatedBy));

            id = _id;
            Status = status;
            UpdatedBy = updatedBy;
        }
    }
}
