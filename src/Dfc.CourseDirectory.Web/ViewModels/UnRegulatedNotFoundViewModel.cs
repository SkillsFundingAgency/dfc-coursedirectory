using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Dfc.CourseDirectory.Services.Enums;
using Dfc.CourseDirectory.Services.UnregulatedProvision;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Dfc.CourseDirectory.Web.ViewModels
{
    public class UnRegulatedNotFoundViewModel
    {
        public IEnumerable<SSAOptions> ssaLevel1 { get; set; }

        public List<SelectListItem> Level1 { get; set; }

        public List<SelectListItem> Level2 { get; set; }

        public string Level1Id { get; set; }

        public string Level2Id { get; set; }

        public List<SelectListItem> Levels { get; set; }

        public string LevelId { get; set; }

        public List<SelectListItem> Categories { get; set; }

        public string CategoryId { get; set; }
    }
}
