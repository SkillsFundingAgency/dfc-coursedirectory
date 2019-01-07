using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewComponents.ModalPopup
{
    public class ModalPopup : ViewComponent
    {
        public IViewComponentResult Invoke(ModalPopupModel model)
        {
            return View("~/ViewComponents/ModalPopup/Default.cshtml", model);
        }
    }
}
