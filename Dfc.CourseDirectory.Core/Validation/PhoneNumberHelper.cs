using PhoneNumbers;

namespace Dfc.CourseDirectory.Core.Validation
{
    public static class PhoneNumberHelper
    {
        private const string Region = "GB";

        public static string FormatPhoneNumber(string phoneNumber)
        {
            var phoneNumberUtil = PhoneNumberUtil.GetInstance();
            var pn = phoneNumberUtil.Parse(phoneNumber, Region);
            return phoneNumberUtil.Format(pn, PhoneNumberFormat.NATIONAL);
        }

        public static bool IsValid(string phoneNumber)
        {
            var phoneNumberUtil = PhoneNumberUtil.GetInstance();

            try
            {
                var pn = phoneNumberUtil.Parse(phoneNumber, Region);
                return phoneNumberUtil.IsValidNumberForRegion(pn, Region);
            }
            catch (NumberParseException)
            {
                return false;
            }
        }
    }
}
