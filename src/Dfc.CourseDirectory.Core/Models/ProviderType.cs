using System;
using System.Linq;

namespace Dfc.CourseDirectory.Core.Models
{
    [Flags]
    public enum ProviderType
    {
        None = 0,
        FE = 1,
        Apprenticeships = 99,
        TLevels = 4
    }

    public static class ProviderTypeExtensions
    {
        public static string ToDescription(this ProviderType providerType)
        {
            if (providerType == ProviderType.None)
            {
                return "None";
            }

            var parts = providerType.SplitFlags()
                .Select(part => part switch
                {
                    ProviderType.Apprenticeships => null,
                    ProviderType.FE => "FE Courses",
                    ProviderType.TLevels => "T Levels",
                    _ => throw new NotImplementedException($"Unknown value: '{providerType}'.")
                })
                .ToArray();

            return parts.ToCommaSeparatedString(finalValuesConjunction: "");
        }
    }
}
