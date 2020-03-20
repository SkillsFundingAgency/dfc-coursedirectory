﻿using OneOf;

namespace Dfc.CourseDirectory.WebV2.Models
{
    public class StandardOrFramework : OneOfBase<Standard, Framework>
    {
        public StandardOrFramework(Standard standard)
            : base(0, standard, null)
        {
        }

        public StandardOrFramework(Framework framework)
            : base(1, null, framework)
        {
        }

        public Framework Framework => AsT1;

        public bool IsFramework => IsT1;

        public bool IsStandard => IsT0;

        public Standard Standard => AsT0;

        public string StandardOrFrameworkTitle => Match(
            s => s.StandardName,
            f => f.NasTitle);
    }
}
