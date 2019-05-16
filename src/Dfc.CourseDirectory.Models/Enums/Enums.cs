using System.ComponentModel;

namespace Dfc.CourseDirectory.Models.Enums
{

    public enum PublishMode
    {
        [Description("Default")]
        Undefined = 0,
        [Description("BulkUpload")]
        BulkUpload = 1,
        [Description("Migration")]
        Migration = 2,
        [Description("DataQualityIndicator")]
        DataQualityIndicator = 3,
        [Description("Summary")]
        Summary = 4
      
    }
    public enum FundingOptions
    {
        [Description("Undefined")]
        Undefined = 0,
        [Description("Adult education budget")]
        AdultEducationBudget = 1,
        [Description("Advanced learner loan")]
        AdvancedLearnerLoan = 2,
    }
    public enum RecordStatus
    {
        [Description("Undefined")]
        Undefined = 0,
        [Description("Live")]
        Live = 1,
        [Description("Pending")]
        Pending = 2,
        [Description("Archived")]
        Archived = 4,
        [Description("Deleted")]
        Deleted = 8,
        [Description("BulkUload Pending")]
        BulkUloadPending = 16,
        [Description("BulkUpload Ready To Go Live")]
        BulkUploadReadyToGoLive = 32,
        [Description("API Pending")]
        APIPending = 64,
        [Description("API Ready To Go Live")]
        APIReadyToGoLive = 128,
        [Description("Migration Pending")]
        MigrationPending = 256,
        [Description("Migration Ready To Go Live")]
        MigrationReadyToGoLive = 512,
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

    public enum RegulatedType
    {
        [Description("Regulated qualification")]
        Regulated = 0,
        [Description("Non-regulated provision")]
        NonRegulated = 1
    }

    public enum ApprenticeshipDelivery
    {
        [Description("At one of your locations")]
        YourLocation = 0,
        [Description("At an employer's address")]
        EmployersAddress = 1,
        [Description("Both")]
        Both = 2
    }

    public enum LocationType
    {
        [Description("Undefined")]
        Undefined = 0,
        [Description("Venue")]
        Venue = 1,
        [Description("Region")]
        Region = 2,
        [Description("SubRegion")]
        SubRegion = 3
    }


    public class Enums
    {
    }
}
