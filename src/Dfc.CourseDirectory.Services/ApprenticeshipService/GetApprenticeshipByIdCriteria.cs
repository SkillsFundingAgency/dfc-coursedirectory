using System;

namespace Dfc.CourseDirectory.Services.ApprenticeshipService
{
    public class GetApprenticeshipByIdCriteria
    {
        public Guid Id { get; }

        public GetApprenticeshipByIdCriteria(
            Guid id)
        {
            Throw.IfNullGuid(id, nameof(id));

            Id = id;
        }
    }
}
