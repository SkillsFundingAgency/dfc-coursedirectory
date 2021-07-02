using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Validation;
using Dfc.CourseDirectory.Testing;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.ValidationTests
{
    public class DateValidationTests : MvcTestBase
    {
        public DateValidationTests(CourseDirectoryApplicationFactory factory)
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

            var request = new HttpRequestMessage(HttpMethod.Post, "DateValidationTests")
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

    public class DateValidationTestsController : Controller
    {
        [HttpPost("DateValidationTests")]
        public IActionResult Post(
            DateValidationTestsModel model,
            [FromServices] DateValidationTestsValidator validator)
        {
            var result = validator.Validate(model);
            return Json(new { isValid = result.IsValid, errors = result.Errors });
        }
    }

    public class DateValidationTestsModel
    {
        public DateInput DateOfBirth { get; set; }
    }

    public class DateValidationTestsValidator : AbstractValidator<DateValidationTestsModel>
    {
        public DateValidationTestsValidator()
        {
            RuleFor(m => m.DateOfBirth)
                .NotEmpty()
                    .WithMessage("Enter your date of birth")
                .Apply(builder => Rules.Date(builder, displayName: "Date of birth"));
        }
    }
}
