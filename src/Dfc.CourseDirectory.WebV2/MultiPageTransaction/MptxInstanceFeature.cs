using System;

namespace Dfc.CourseDirectory.WebV2.MultiPageTransaction
{
    public class MptxInstanceFeature
    {
        public MptxInstanceFeature(MptxInstance instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            Instance = instance;
        }

        public MptxInstance Instance { get; }
    }
}
