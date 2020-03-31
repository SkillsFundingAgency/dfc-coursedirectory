using System;

namespace Dfc.CourseDirectory.WebV2.MultiPageTransaction
{
    public class MptxInstanceContextFeature
    {
        public MptxInstanceContextFeature(MptxInstanceContext instanceContext)
        {
            InstanceContext = instanceContext ?? throw new ArgumentNullException(nameof(instanceContext));
        }

        public MptxInstanceContext InstanceContext { get; }
    }
}
