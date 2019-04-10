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
                _categoriesModel.Add(new CategoryModel() { Id = "APP H CAT A", Category = "A: SFA funded NR provision" });
                _categoriesModel.Add(new CategoryModel() { Id = "APP H CAT B", Category = "B: SFA funded English, maths, ESOL" });
                _categoriesModel.Add(new CategoryModel() { Id = "APP H CAT C", Category = "C: SFA funded NQF Adult Basic Skills" });
                _categoriesModel.Add(new CategoryModel() { Id = "APP H CAT D", Category = "D: Innovation code" });
                _categoriesModel.Add(new CategoryModel() { Id = "APP H CAT E", Category = "E: Not Community Learning or SFA funded" });
                _categoriesModel.Add(new CategoryModel() { Id = "APP H CAT F", Category = "F: Community learning" });
                _categoriesModel.Add(new CategoryModel() { Id = "APP H CAT G", Category = "G: Non-SFA funded English, maths, ESOL" });
                _categoriesModel.Add(new CategoryModel() { Id = "APP H CAT H", Category = "H: Units of NQF qualifications" });
                _categoriesModel.Add(new CategoryModel() { Id = "APP H CAT I", Category = "I: Work experience / placement" });
                _categoriesModel.Add(new CategoryModel() { Id = "APP H CAT J", Category = "J: Supported internship" });
                _categoriesModel.Add(new CategoryModel() { Id = "APP H CAT K", Category = "K: Programme aim" });
                _categoriesModel.Add(new CategoryModel() { Id = "APP H CAT L", Category = "L: ESF co financed" });
                _categoriesModel.Add(new CategoryModel() { Id = "APP H CAT M", Category = "M: Conversion codes between HNQs" });
                _categoriesModel.Add(new CategoryModel() { Id = "APP H CAT O", Category = "O: Education assessments" });
                return _categoriesModel;
            }
        }


    }
}

