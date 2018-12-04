using Dfc.CourseDirectory.Models.Interfaces.Qualifications;
using Dfc.CourseDirectory.Models.Models.Qualifications;
using Xunit;

namespace Dfc.CourseDirectory.Models.Test
{
    
    public class QualificationTests
    {
        public void Creating_And_Assigning_Qualifications()
        {
            Qualification qual = new Qualification("","","","","");
            Assert.NotNull(qual);
        }
    }
}
