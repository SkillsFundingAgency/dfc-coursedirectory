using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Dfc.CourseDirectory.Models.Enums
{

    public enum ApprenticeShipPreviousPage
    {
        [Display(Name = "Undefined")]
        [Description("Undefined")]
        Undefined = 0,
        [Display(Name = "Details")]
        [Description("Details")]
        Details = 1,
        [Display(Name = "DeliveryLocation")]
        [Description("DeliveryLocation")]
        DeliveryLocation = 2,
        [Display(Name = "DeliveryOptions")]
        [Description("DeliveryOptions")]
        DeliveryOptions = 3,
        [Display(Name = "LocationChoice")]
        [Description("LocationChoice")]
        LocationChoice = 4,
        [Display(Name = "LocationRegions")]
        [Description("LocationRegions")]
        LocationRegions = 5,
        [Display(Name = "Summary")]
        [Description("Summary")]
        Summary = 6,
        [Display(Name = "Complete")]
        [Description("Complete")]
        Complete = 7,
        [Display(Name = "YourApprenticeships")]
        [Description("YourApprenticeships")]
        YourApprenticeships = 8

    }

    //public enum ApprenticeShipMode
    //{
    //    [Display(Name = "Undefined")]
    //    [Description("Undefined")]
    //    Undefined = 0,
    //    [Display(Name = "Add new")]
    //    [Description("Add")]
    //    Add = 1,
    //    [Display(Name = "Edit")]
    //    [Description("Edit")]
    //    Edit = 2

    //}

    public enum ApprenticeShipDeliveryLocation
    {
        [Display(Name = "Undefined")]
        [Description("Undefined")]
        Undefined = 0,
        [Display(Name = "Day release")]
        [Description("Day Release")]
        DayRelease = 1,
        [Display(Name = "Block release")]
        [Description("Block Release")]
        BlockRelease = 2,
        [Display(Name = "Employer address")]
        [Description("Employer address")]
        EmployerAddress = 3

    }
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
        BulkUploadPending = 16,
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

    public enum WhatDoYouWantToDoNext
    {
        [Description("Undefined")]
        Undefined = 0,
        [Description("Fix the errors on screen using this service")]
        OnScreen = 1,
        [Description("Download an error file to fix them at source and start again with a new bulk upload")]
        DownLoad = 2,
        [Description("Delete the file you uploaded and make no changes to the Course directory ")]
        Delete = 3
    }

    public enum MigrationErrors
    {
        Undefined = 0,
        [Description("Fix errors on screen using this service.")]
        [Hint("This will automatically publish the courses to the Course Directory")]
        FixErrors = 1,
        [Description("Delete the courses with errors.")]
        [Hint("You will need to call the help desk to do this.")]
        DeleteCourses = 2,
        [Description("Start again by publishing all of your courses with a file upload.")]
        [Hint("You can download an error file to help you")]
        StartAgain = 3
    }

    public enum MigrationOptions
    {
        Undefined = 0,
        [Description("Check the courses you have published on the Course Directory.")]
        [Hint("You can edit and delete them or add a new one.")]
        CheckCourses = 1,
        [Description("Start again by publishing all of your courses with a file upload.")]
        [Hint("This will overwrite and migrated data")]
        StartAgain = 2
    }

    public enum MigrationDeleteOptions
    {
        Undefined = 0,
        [Description("Yes")]
        DeleteMigrations = 1,
        [Description("No")]
        Cancel = 2
    }

    public enum CoursesLandingOptions
    {
        [Description("Undefined")]
        Undefined = 0,
        [Description("Add a new course")]
        Add = 1,
        [Description("Add multiple courses via a bulk upload")]
        Upload = 2,
        [Description("View or make changes to existing course(s)")]
        View = 3
    }

    public enum LocationsLandingOptions
    {
        [Description("Undefined")]
        Undefined = 0,
        [Description("Add a new location")]
        Add = 1,
        [Description("Manage your existing locations")]
        Manage = 2,
    
    }

    public enum BulkUploadLandingOptions
    {
        [Description("Undefined")]
        Undefined = 0,
        [Description("Further education courses")]
        FE = 1,
        [Description("Apprenticeship training")]
        Apprenticeship = 2,

    }

    public enum ApprenticeshipWhatWouldYouLikeToDo
    {
        [Description("Undefined")]
        Undefined = 0,
        [Description("Add an apprenticeship")]
        Add = 1,
        [Description("Add multiple apprenticeships via a file upload")]
        Upload = 2,
        [Description("View or make changes to an existing apprenticeship")]
        View = 3
    }


    public enum ApprenticeshipDelete
    {
        [Description("Undefined")]
        Undefined = 0,
        [Description("Yes, Delete")]
        Delete = 1,
        [Description("No, go back")]
        Back = 2,

    }

    public enum ApprenticeshipDelivery
    {
        [Description("Undefined")]
        Undefined = 0,
        [Description("At one of your locations")]
        YourLocation = 1,
        [Description("At an employer's address")]
        EmployersAddress = 2,
        [Description("Both")]
        Both = 3
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

    public enum NationalApprenticeship
    {
        [Description("Undefined")]
        Undefined = 0,
        [Description("Yes")]
        Yes = 1,
        [Description("No")]
        No = 2
    }

    public class Enums
    {
    }
}
