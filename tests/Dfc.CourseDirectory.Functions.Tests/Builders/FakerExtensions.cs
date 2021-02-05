namespace Dfc.CourseDirectory.Functions.Tests.Builders
{
    /// <summary>
    /// Things not available in Faker.net
    /// </summary>
    public static class DfcFaker
    {
        public static decimal UkLatitude()
        {
            return Faker.RandomNumber.Next(5000,6020)/100m;
        }
        public static decimal UkLongitude()
        {
            return Faker.RandomNumber.Next(-770,176)/100m;
        }
    }
}
