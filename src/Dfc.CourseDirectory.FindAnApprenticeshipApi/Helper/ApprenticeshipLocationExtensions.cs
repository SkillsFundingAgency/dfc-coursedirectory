using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;

namespace Dfc.CourseDirectory.FindAnApprenticeshipApi.Helper
{
    public static class ApprenticeshipLocationExtensions
    {
        public static int ToAddressHash(this ApprenticeshipLocation location)
        {
            var propertiesToHash = new List<string> {location.Venue?.VenueName};

            if (location.National.HasValue)
            {
                propertiesToHash.Add($"{location.National}");
            }

            if (location.SubRegionIds != null && location.SubRegionIds.Count > 0)
            {
                propertiesToHash.Add(string.Join(",", location.SubRegionIds));
            }

            if (location.Venue != null)
            {
                propertiesToHash.Add($"{location.Venue.Latitude}");
                propertiesToHash.Add($"{location.Venue.Longitude}");
                propertiesToHash.Add($"{location.Venue.Postcode}");
            }

            return GenerateHash(string.Join(", ", propertiesToHash));
        }

        private static int GenerateHash(string value)
        {
            using (var crypto = new SHA256CryptoServiceProvider())
            {
                var hash = 0;
                if (string.IsNullOrEmpty(value)) return (hash);

                var byteContents = Encoding.Unicode.GetBytes(value);

                var hashText = crypto.ComputeHash(byteContents);

                var α = BitConverter.ToInt64(hashText, 0);
                var β = BitConverter.ToInt64(hashText, 8);
                var γ = BitConverter.ToInt64(hashText, 24);

                hash = (int) (α ^ β ^ γ);
                return (hash);
            }
        }
    }

}
