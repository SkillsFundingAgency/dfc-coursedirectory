
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;


namespace Dfc.CourseDirectory.FindACourseApi.Models
{
    public enum RecordStatus
    {
        [Description("Undefined")]
        Undefined = 0,
        [Description("Pending")]
        Pending = 1,
        [Description("Live")]
        Live = 2,
        [Description("Archived")]
        Archived = 3,
        [Description("Deleted")]
        Deleted = 4,
        [Description("Ready To Go Live")]
        ReadyToGoLive = 5
    }

    public enum TransferMethod
    {
        [Description("Undefined")]
        Undefined = 0,
        [Description("BulkUpload")]
        BulkUpload = 1,
        [Description("API")]
        API = 2,
        [Description("CourseMigrationTool")]
        CourseMigrationTool = 3,
        [Description("CourseMigrationToolCsvFile")]
        CourseMigrationToolCsvFile = 4,
        [Description("CourseMigrationToolSingleUkprn")]
        CourseMigrationToolSingleUkprn = 5
    }

    public enum MigrationSuccess
    {
        [Description("Undefined")]
        Undefined = 0,
        [Description("Success")]
        Success = 1,
        [Description("Failure")]
        Failure = 2
    }

    public enum DeploymentEnvironment
    {
        [Description("Undefined")]
        Undefined = 0,
        [Description("Local")]
        Local = 1,
        [Description("Dev")]
        Dev = 2,
        [Description("Sit")]
        Sit = 3,
        [Description("PreProd")]
        PreProd = 4,
        [Description("Prod")]
        Prod = 5
    }
}
