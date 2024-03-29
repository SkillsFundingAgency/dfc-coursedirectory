﻿using System;
using Dfc.CourseDirectory.Services.Models;

namespace Dfc.CourseDirectory.Web.ViewModels.EditCourse
{
    public class EditCourseSaveViewModel
    {
        public string CourseFor { get; set; }
        public string EntryRequirements { get; set; }
        public string WhatWillLearn { get; set; }
        public string HowYouWillLearn { get; set; }
        public string WhatYouNeed { get; set; }
        public string HowAssessed { get; set; }
        public string WhereNext { get; set; }

        public bool AdultEducationBudget { get; set; }

        public bool AdvancedLearnerLoan { get; set; }

        public Guid? CourseId { get; set; }
        public Guid? CourseRunId { get; set; }
        public PublishMode Mode { get; set; }
        public string CourseName { get; set; }

    }

   
}
