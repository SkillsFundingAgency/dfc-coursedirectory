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

#### Secrets

* Get user secrets from another developer on the team.

#### Create the database

* Manually create empty database `CourseDirectory`:

```
create database CourseDirectory;
ALTER DATABASE CourseDirectory SET ALLOW_SNAPSHOT_ISOLATION ON;
```

#### Deploy database from Visual Studio

* Right-click "Publish" on the database project then deploy to database `CourseDirectory`.

#### Deploy database from CLI

Temporarily configure user secret `ConnectionStrings:DefaultConnection` for project `CourseDirectoryTesting` to point to database `CourseDirectory`.

Run the tests in `Dfc.CourseDirectory.WebV2.Tests`, this will cause the dacpac to deploy.

Undo the temporary user secret configuration change.

#### Run the web project

Run project `Dfc.CourseDirectory.Web`

Navigate to https://localhost:44345/ in a browser.

#### Login

Ask the team to make you a valid DfE Sign-in account with provider(s) associated with it.

## Project architecture


### Azure Function App Dependencies

This project depends on multiple deployed Azure function app resources:

* [dfc-providerportal-courses](https://github.com/SkillsFundingAgency/dfc-providerportal-courses)
* [dfc-providerportal-fileprocessor](https://github.com/SkillsFundingAgency/dfc-providerportal-fileprocessor)
* [dfc-providerportal-tribalexporter](https://github.com/SkillsFundingAgency/dfc-providerportal-tribalexporter)
* [dfc-providerportal-ukrlp](https://github.com/SkillsFundingAgency/dfc-providerportal-ukrlp)

### Project Structure

#### Legacy

The projects in the `Legacy` solution folder were inherited without good test coverage and are not in an easily testable state.

#### WebV2

New code is being written in the WebV2 project that is test-driven and uses modern patterns of development to achieve cleaner testable code.

See the [WebV2 readme](src/Dfc.CourseDirectory.WebV2/README.md) for details of the patterns in use.

The WebV2 controllers are made available under the legacy `Dfc.CourseDirectory.Web` project so that we can move routes across one at time. As such to run the WebV2 code you actually need to run the original "Web" project.
