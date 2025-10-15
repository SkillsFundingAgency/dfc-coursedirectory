using System;
using System.Text.RegularExpressions;

namespace Dfc.CourseDirectory.Core.Models
{
    public sealed class Postcode : IEquatable<Postcode>
    {
        private static readonly Regex _ukPostcodePattern = new Regex(
            @"^((([A-Za-z][0-9]{1,2})|(([A-Za-z][A-Ha-hJ-Yj-y][0-9]{1,2})|(([A-Za-z][0-9][A-Za-z])|([A-Za-z][A-Ha-hJ-Yj-y][0-9][A-Za-z]?))))\s?[0-9][A-Za-z]{2})$",
            RegexOptions.Compiled);

        private readonly string _value;

        public Postcode(string value)
            : this(value, validated: false)
        {
        }

        private Postcode(string value, bool validated)
        {
            if (!validated && !ValidateAndNormalize(value, out value))
            {
                throw new FormatException("Input is not a valid UK postcode.");
            }

            _value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public static bool TryParse(string s, out Postcode postcode)
        {
            if (!ValidateAndNormalize(s, out var normalized))
            {
                postcode = default;
                return false;
            }

            postcode = new Postcode(normalized, validated: true);
            return true;
        }

        private static bool ValidateAndNormalize(string input, out string normalized)
        {
            if (input == null)
            {
                normalized = default;
                return false;
            }

            if (!_ukPostcodePattern.IsMatch(input))
            {
                normalized = default;
                return false;
            }

            // See https://www.bph-postcodes.co.uk/guidetopc.cgi

            var trimmed = input.Replace(" ", "").Trim().ToUpper();
            normalized = $"{trimmed[0..(trimmed.Length - 3)]} {trimmed[^3..]}";
            return true;
        }

        public static bool operator ==(Postcode a, Postcode b) => (a is null && b is null) || a.Equals(b);

        public static bool operator !=(Postcode a, Postcode b) => !(a == b);

        public static implicit operator string(Postcode postcode) => postcode._value;

        public override bool Equals(object obj) => obj is Postcode other && Equals(other);

        public bool Equals(Postcode other) => other?._value == _value;

        public override int GetHashCode() => _value.GetHashCode();

        public override string ToString() => _value;
    }
}
