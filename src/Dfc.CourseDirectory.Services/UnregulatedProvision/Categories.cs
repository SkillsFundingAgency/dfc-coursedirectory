using System.Collections.Generic;

namespace Dfc.CourseDirectory.Services.UnregulatedProvision
{
    public class Categories
    {

        private List<CategoryModel> _categoriesModel = new List<CategoryModel>();
        public List<CategoryModel> AllCategogies
        {
            get
            {
                _categoriesModel = new List<CategoryModel>();
                _categoriesModel.Add(new CategoryModel() { Id = "A: SFA funded NR provision", Category = "A: SFA funded NR provision" });
                _categoriesModel.Add(new CategoryModel() { Id = "B: SFA funded English, maths, ESOL", Category = "B: SFA funded English, maths, ESOL" });
                _categoriesModel.Add(new CategoryModel() { Id = "C: SFA funded NQF Adult Basic Skills", Category = "C: SFA funded NQF Adult Basic Skills" });
                _categoriesModel.Add(new CategoryModel() { Id = "D: Innovation code", Category = "D: Innovation code" });
                _categoriesModel.Add(new CategoryModel() { Id = "E: Not Community Learning or SFA funded", Category = "E: Not Community Learning or SFA funded" });
                _categoriesModel.Add(new CategoryModel() { Id = "F: Community learning", Category = "F: Community learning" });
                _categoriesModel.Add(new CategoryModel() { Id = "G: Non-SFA funded English, maths, ESOL", Category = "G: Non-SFA funded English, maths, ESOL" });
                _categoriesModel.Add(new CategoryModel() { Id = "H: Units of NQF qualifications", Category = "H: Units of NQF qualifications" });
                _categoriesModel.Add(new CategoryModel() { Id = "I: Work experience / placement", Category = "I: Work experience / placement" });
                _categoriesModel.Add(new CategoryModel() { Id = "J: Supported internship", Category = "J: Supported internship" });
                _categoriesModel.Add(new CategoryModel() { Id = "K: Programme aim", Category = "K: Programme aim" });
                _categoriesModel.Add(new CategoryModel() { Id = "L: ESF co financed", Category = "L: ESF co financed" });
                _categoriesModel.Add(new CategoryModel() { Id = "M: Conversion codes between HNQs", Category = "M: Conversion codes between HNQs" });
                _categoriesModel.Add(new CategoryModel() { Id = "O: Education assessments", Category = "O: Education assessments" });
                return _categoriesModel;
            }
        }


    }
}

