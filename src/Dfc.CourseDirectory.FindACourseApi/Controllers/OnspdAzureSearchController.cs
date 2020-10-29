
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Newtonsoft.Json;


//namespace Dfc.ProviderPortal.AzureSearch.Onspd
//{
//    /// <summary>
//    /// Controller for swashbuckle to generate swagger doc for Onspd search
//    /// </summary>
//    [Route("api/[controller]")]
//    [ApiController]
//    public class OnspdAzureSearchController : ControllerBase
//    {
//        /// <summary>
//        /// Stub endpoint for swashbuckle
//        /// POST api/wherever
//        /// </summary>
//        /// <param name="criteria">Criteria</param>
//        [Route("~/onspdsearch")]
//        [HttpPost]
//        [ProducesResponseType(typeof(PostCodeSearchCriteria), StatusCodes.Status200OK)]
//        [ProducesResponseType(StatusCodes.Status403Forbidden)]
//        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
//        public PostCodeSearchResult PostcodeSearch([FromBody] PostCodeSearchCriteria criteria)
//        {
//            return new PostCodeSearchResult()
//            {
//                //ODataContext = "https://dfc-dev-prov-sch.search.windows.net/indexes('onspd')/$metadata#docs(*)",
//                //ODataCount = 567,
//                //SearchFacets = new OnspdSearchFacets() { },
//                //Value = new PostCodeSearchResultItem[] { new PostCodeSearchResultItem() { Id = "123", Text = "abc" } }
//            };
//        }




//        ////////////////////////////////////////////////////////////////////////////////
//        // The following classes are types required for post method                   //
//        // They are copied from elsewhere as we don't have a central repo for models! //
//        ////////////////////////////////////////////////////////////////////////////////


//        /// <summary>
//        /// search criteria
//        /// </summary>
//        public class PostCodeSearchCriteria //: ValueObject<PostCodeSearchCriteria>, IPostCodeSearchCriteria
//        {
//            /// <summary/>
//            public string Search { get; }
//        }

//        /// <summary>
//        /// search result
//        /// </summary>
//        public class PostCodeSearchResult //: ValueObject<PostCodeSearchResult>, IPostCodeSearchResult
//        {
//            /// <summary/>
//            public IEnumerable<PostCodeSearchResultItem> Value { get; set; }
//        }

//        public class PostCodeSearchResultItem //: ValueObject<PostCodeSearchResultItem>, IPostCodeSearchResultItem
//        {
//            /// <summary/>
//            public string Id { get; }
//            /// <summary/>
//            public string Text { get; }
//        }
//    }
//}
