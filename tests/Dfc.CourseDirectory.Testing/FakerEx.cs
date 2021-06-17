namespace Dfc.CourseDirectory.Testing
{
    public static class FakerEx
    {
        public static string StreetAddressSafe()
        {
            // Some Address.StreetAddress() results don't validate against our rules;
            // keep grabbing one until we get a valid result

            string result;

            do
            {
                result = Faker.Address.StreetAddress();
            }
            while (!Core.Validation.VenueValidation.RuleBuilderExtensions.AddressLinePattern.IsMatch(result));

            return result;
        }
    }
}
