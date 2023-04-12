using System;
using System.Linq;

namespace Dfc.CourseDirectory.Core.Models
{
    [Flags]
    public enum ProviderType
    {
        None = 0,
        FE = 1,
        TLevels = 4,
        Apprenticeships = 99
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

        public static string ToDescriptionWithoutApprenticeships(this ProviderType providerType)
        {
            var parts = providerType.ProviderTypeSplitFlags()
                .Select(part => part switch
                {
                    ProviderType.FE => "FE Courses",
                    ProviderType.TLevels => "T Levels",
                    _ => throw new NotImplementedException($"Unknown value: '{providerType}'.")
                })
                .ToArray();

            return parts.ToCommaSeparatedString(finalValuesConjunction: "");
        }
    }
}
