using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services.Enums;
using Dfc.CourseDirectory.Services.Interfaces;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Services
{
    public class AddressSelectionCriteria : ValueObject<PostCodeSearchCriteria>, IAddressSelectionCriteria
    {
        public string Id { get; }

        public AddressSelectionCriteria(
            string id)
        {
            Throw.IfNullOrWhiteSpace(id, nameof(id));


            Id = id;

        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Id;

        }
    }
}