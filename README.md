# dfc-coursedirectory

[![Build Status](https://dev.azure.com/sfa-gov-uk/Digital%20First%20Careers/_apis/build/status/Find%20an%20Opportunity/dfc-coursedirectory?branchName=main)](https://dev.azure.com/sfa-gov-uk/Digital%20First%20Careers/_build/latest?definitionId=1700&branchName=main)

User Interface for course directory, implemented using the provider portal APIs.
Contains provider portal Azure resources in the Resources directory.

## Developer setup

### Running the tests

Manually create an empty database named `CourseDirectoryTesting`:

```
create database CourseDirectoryTesting;
ALTER DATABASE CourseDirectoryTesting SET ALLOW_SNAPSHOT_ISOLATION ON;
```

Run the tests.

The dacpac will automatically be deployed and create all the database schema and tables.

### Running the web app locally

Get user secrets from another developer on the team.

Manually create empty database `CourseDirectory`:
```
create database CourseDirectory;
ALTER DATABASE CourseDirectory SET ALLOW_SNAPSHOT_ISOLATION ON;
```
Temporarily configure user secret `ConnectionStrings:DefaultConnection` for project `CourseDirectoryTesting` to point to database `CourseDirectory`.

Run the tests in `Dfc.CourseDirectory.WebV2.Tests`, this will cause the dacpac to deploy.

Undo the temporary user secret configuration change.

Run project `Dfc.CourseDirectory.Web`

Navigate to https://localhost:44345/ in a browser.

Ask the team for a valid DfE Sign-in account with provider(s) associated with it.
