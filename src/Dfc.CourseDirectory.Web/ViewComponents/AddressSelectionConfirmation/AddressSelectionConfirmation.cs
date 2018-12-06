using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.ViewComponents.PostCodeSearchResult
{
    public class AddressSelectionConfirmation : ViewComponent
    {
        public IViewComponentResult Invoke(PostCodeSearchResultModel model)
        {
            var actualModel = model ?? new PostCodeSearchResultModel();

            return View("~/ViewComponents/PostCodeSearchResult/Default.cshtml", actualModel);
        }
    }
}