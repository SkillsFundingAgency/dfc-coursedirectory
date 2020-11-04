using System;

namespace Dfc.CourseDirectory.Services.ProviderService
{
    public class ProviderAdd
    {
        public Guid id { get; set; }
        public int Status { get; set;  }
        public string UpdatedBy { get; set; }

        public ProviderAdd(Guid _id, int status, string updatedBy)
        {
            if (_id == null)
            {
                throw new ArgumentNullException(nameof(_id));
            }

            if (string.IsNullOrWhiteSpace(updatedBy))
            {
                throw new ArgumentNullException($"{nameof(updatedBy)} cannot be null or empty or whitespace.", nameof(updatedBy));
            }

            id = _id;
            Status = status;
            UpdatedBy = updatedBy;
        }
    }
}
