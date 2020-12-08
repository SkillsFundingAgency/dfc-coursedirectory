using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Testing;
using Dfc.CourseDirectory.WebV2.Validation;
using GovUk.Frontend.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.ValidationTests
{
    public class ValidatorBaseTests : MvcTestBase
    {
        public ValidatorBaseTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Theory]
        [InlineData(null, null, null, "Enter your date of birth")]
        [InlineData("", "", "", "Enter your date of birth")]
        [InlineData("", "2", "1987", "Date of birth must include a day")]
        [InlineData("", "", "1987", "Date of birth must include a day and month")]
        [InlineData("6", "2", "", "Date of birth must include a year")]
        [InlineData("6", "", "", "Date of birth must include a month and year")]
        [InlineData("6", "", "1987", "Date of birth must include a month")]
        [InlineData("0", "2", "1987", "Date of birth must be a real date")]
        [InlineData("29", "2", "1987", "Date of birth must be a real date")]
        [InlineData("6", "2", "0", "Date of birth must be a real date")]
        [InlineData("6", "2", "10000", "Date of birth must be a real date")]
        public async Task Date(string day, string month, string year, string expectedErrorMessage)
        {
            // Arrange
            var content = new FormUrlEncodedContentBuilder()
                .Add("DateOfBirth.Day", day)
                .Add("DateOfBirth.Month", month)
                .Add("DateOfBirth.Year", year)
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, "ValidatorBaseTests")
            {
                Content = content
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();

            var json = JObject.Parse(await response.Content.ReadAsStringAsync());
            var errors = (JArray)json["errors"];
            var error = Assert.Single(errors);
            Assert.Equal("DateOfBirth", error.SelectToken("propertyName").ToString());
            Assert.Equal(expectedErrorMessage, error.SelectToken("errorMessage").ToString());
        }
    }

    public class ValidatorBaseTestsController : Controller
    {
        [HttpPost("ValidatorBaseTests")]
        public IActionResult Post(
            ValidatorBaseTestsModel model,
            [FromServices] ValidatorBaseTestsValidator validator)
        {
            var result = validator.Validate(model);
            return Json(new { isValid = result.IsValid, errors = result.Errors });
        }
    }

    public class ValidatorBaseTestsModel
    {
        public Date? DateOfBirth { get; set; }
    }

    public class ValidatorBaseTestsValidator : ValidatorBase<ValidatorBaseTestsModel>
    {
        public ValidatorBaseTestsValidator(IActionContextAccessor actionContextAccessor)
            : base(actionContextAccessor)
        {
            RuleFor(m => m.DateOfBirth)
                .Date(
                    displayName: "Date of birth",
                    missingErrorMessage: "Enter your date of birth",
                    isRequired: true);
        }
    }
}
