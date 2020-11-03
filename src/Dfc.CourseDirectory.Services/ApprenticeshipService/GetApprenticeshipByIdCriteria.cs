using System;
using Dfc.CourseDirectory.Services.Interfaces.ApprenticeshipService;

namespace Dfc.CourseDirectory.Services.ApprenticeshipService
{
    public class GetApprenticeshipByIdCriteria : IGetApprenticeshipByIdCriteria
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
