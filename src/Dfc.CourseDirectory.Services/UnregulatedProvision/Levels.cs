using System.Collections.Generic;

namespace Dfc.CourseDirectory.Services.UnregulatedProvision
{
    public class Levels
    {
        private List<LevelsModel> _levelsModel = new List<LevelsModel>();
        public List<LevelsModel> AllLevels
        {
            get
            {
                _levelsModel = new List<LevelsModel>
                    {
                        new LevelsModel() {Id = "E", Level = "Entry level"},
                        new LevelsModel() {Id = "1", Level = "Level 1"},
                        new LevelsModel() {Id = "2", Level = "Level 2"},
                        new LevelsModel() {Id = "3", Level = "Level 3"},
                        new LevelsModel() {Id = "4", Level = "Level 4"},
                        new LevelsModel() {Id = "5", Level = "Level 5"},
                        new LevelsModel() {Id = "6", Level = "Level 6"},
                        new LevelsModel() {Id = "7", Level = "Level 7"},
                        new LevelsModel() {Id = "8", Level = "Level 8"},
                        new LevelsModel() {Id = "M", Level = "Mixed"},
                        new LevelsModel() {Id = "H", Level = "Higher"},
                        new LevelsModel() {Id = "X", Level = "X - Not applicable/unknown"},
                        new LevelsModel() {Id = "Level 1/2", Level = "Level 1/2"}
                    };
                return _levelsModel;
            }


        }
    }
}

