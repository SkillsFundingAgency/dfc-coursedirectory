using System;
using Dfc.CourseDirectory.WebV2.Models;

namespace Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries
{
    public class CreateApprenticeshipQAUnableToCompleteInfo : ISqlQuery<int>
    {
        public Guid ProviderId { get; set; }
        public ApprenticeshipQAUnableToCompleteReasons UnableToCompleteReasons { get; set; }
        public string Comments { get; set; }
        public string StandardName { get; set; }
        public DateTime AddedOn { get; set; }
        public string AddedByUserId { get; set; }
    }
}
