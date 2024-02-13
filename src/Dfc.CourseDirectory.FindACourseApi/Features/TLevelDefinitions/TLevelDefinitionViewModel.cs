using System;

namespace Dfc.CourseDirectory.FindACourseApi.Features.TLevelDefinitions
{
    public class TLevelDefinitionViewModel
    {
        public Guid TLevelDefinitionId { get; set; }
        public int FrameworkCode { get; set; }
        public int ProgType { get; set; }
        public string QualificationLevel { get; set; }
        public string Name { get; set; }
    }
}
