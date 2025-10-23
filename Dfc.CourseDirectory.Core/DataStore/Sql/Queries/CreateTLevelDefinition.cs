using System;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class CreateTLevelDefinition : ISqlQuery<Success>
    {
        public Guid TLevelDefinitionId { get; set; }
        public int FrameworkCode { get; set; }
        public int ProgType { get; set; }
        public int QualificationLevel { get; set; }
        public string Name { get; set; }
        public string ExemplarWhoFor { get; set; }
        public string ExemplarEntryRequirements { get; set; }
        public string ExemplarWhatYoullLearn { get; set; }
        public string ExemplarHowYoullLearn { get; set; }
        public string ExemplarHowYoullBeAssessed { get; set; }
        public string ExemplarWhatYouCanDoNext { get; set; }
    }
}
