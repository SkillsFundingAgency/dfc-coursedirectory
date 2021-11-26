using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing;
using Dfc.CourseDirectory.WebV2.Features.Apprenticeships.ClassroomLocation;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.Apprenticeships
{
    public class ChooseQualification : MvcTestBase
    {
        public ChooseQualification(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }
    }
}
