using System.Collections.Generic;

namespace Dfc.CourseDirectory.Services.UnregulatedProvision
{
    public static class Categories
    {

        
        public static List<CategoryModel> AllCategories
        {
            get
            {
                List<CategoryModel> _categoriesModel = new List<CategoryModel>();
                _categoriesModel = new List<CategoryModel>();
                _categoriesModel.Add(new CategoryModel() { Id = "APP H CAT A", Category = "A: Non-regulated (ESFA funded)" });
                _categoriesModel.Add(new CategoryModel() { Id = "APP H CAT B", Category = "B: English, maths, and ESOL (ESFA funded)" });
                _categoriesModel.Add(new CategoryModel() { Id = "APP H CAT C", Category = "C: NQF adult basic skills (ESFA funded)" });
                _categoriesModel.Add(new CategoryModel() { Id = "APP H CAT D", Category = "D: Innovation code" });
                _categoriesModel.Add(new CategoryModel() { Id = "APP H CAT E", Category = "E: Not Community Learning and not ESFA funded" });
                _categoriesModel.Add(new CategoryModel() { Id = "APP H CAT F", Category = "F: Community Learning" });
                _categoriesModel.Add(new CategoryModel() { Id = "APP H CAT G", Category = "G: English, maths, and ESOL (not ESFA funded)" });
                _categoriesModel.Add(new CategoryModel() { Id = "APP H CAT H", Category = "H: NQF qualification units" });
                _categoriesModel.Add(new CategoryModel() { Id = "APP H CAT I", Category = "I: Work experience and work placement" });
                _categoriesModel.Add(new CategoryModel() { Id = "APP H CAT J", Category = "J: Supported internship" });
                _categoriesModel.Add(new CategoryModel() { Id = "APP H CAT K", Category = "K: Programme aim" });
                _categoriesModel.Add(new CategoryModel() { Id = "APP H CAT L", Category = "L: ESFA co-financed" });
                _categoriesModel.Add(new CategoryModel() { Id = "APP H CAT M", Category = "M: Conversion codes between HNQs" });
                _categoriesModel.Add(new CategoryModel() { Id = "APP H CAT O", Category = "O: Education assessment" });
                return _categoriesModel;
            }
        }


    }
}

