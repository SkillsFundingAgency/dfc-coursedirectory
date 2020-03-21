using System;

namespace Dfc.CourseDirectory.WebV2.MultiPageTransaction
{
    public class MptxInstanceFeature
    {
        public MptxInstanceFeature(MptxInstance instance)
        {
            Instance = instance ?? throw new ArgumentNullException(nameof(instance));
        }

        public MptxInstance Instance { get; }
    }
}
