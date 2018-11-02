using Dfc.CourseDirectory.Services.Enums;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Xunit;

namespace Dfc.CourseDirectory.Services.Tests
{
    public class LarsSearchServiceTests
    {
        [Fact]
        public async void SearchAsync_Just_to_help_me_dev()
        {
            // arrange
            var criteria = new LarsSearchCriteria(
                "business Management", 
                null, 
                new LarsSearchFacet[] 
                {
                    LarsSearchFacet.NotionalNVQLevelv2,
                    LarsSearchFacet.AwardOrgCode
                }, 
                true);
            var settings = new LarsSearchSettings()
            {
                ApiUrl = "https://dfc-larsearch.search.windows.net/indexes/index-lars-awardorg/docs/search",
                ApiKey = "B60E6C1EB35B630F22F471375AF2FD23",
                ApiVersion = "2017-11-11",
                Indexes = "index-lars-awardorg"
            };
            var service = new LarsSearchService(new HttpClient(), Options.Create(settings));

            // act
            var actual = await service.SearchAsync(criteria);

            // assert
            Assert.Equal(1, 0);
        }
    }
}
