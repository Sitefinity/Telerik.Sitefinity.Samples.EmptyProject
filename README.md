Telerik.Sitefinity.Samples.EmptyProject
=======================================

[![Build Status](http://sdk-jenkins-ci.cloudapp.net/buildStatus/icon?job=Telerik.Sitefinity.Samples.EmptyProject.CI)](http://sdk-jenkins-ci.cloudapp.net/job/Telerik.Sitefinity.Samples.EmptyProject.CI/)

The Empty Project sample project is module-free and is suitable for people who want to build a project from ground-up. 

Using the Empty Project sample, you can build the project from scratch and make it ready for further modification and adding of custom modules.

### Requirements

* Sitefinity license
* .NET Framework 4
* Visual Studio 2012
* Microsoft SQL Server 2008R2 or later versions

### Prerequisites

Clear the NuGet cache files. To do this:

1. In Windows Explorer, open the **%localappdata%\NuGet\Cache** folder.
2. Select all files and delete them.

### Nuget package restoration
The solution in this repository relies on NuGet packages with automatic package restore while the build procedure takes place.   
For a full list of the referenced packages and their versions see the [packages.config](https://github.com/Sitefinity-SDK/Telerik.Sitefinity.Samples.EmptyProject/blob/master/SitefinityWebApp/packages.config) file.    
For a history and additional information related to package versions on different releases of this repository, see the [Releases page](https://github.com/Sitefinity-SDK/Telerik.Sitefinity.Samples.EmptyProject/releases).    


### Installation instructions: SDK Samples from GitHub

1. In Solution Explorer, navigate to _SitefinityWebApp_ » *App_Data* » _Sitefinity_ » _Configuration_ and select the **StartupConfig.config** file. 
2. Modify the **dbType**, **sqlInstance** and **dbName** values to match your server settings.
3. Build the solution.

For version-specific details about the required Sitefinity NuGet packages for this sample application, click on [Releases](https://github.com/Sitefinity-SDK/Telerik.Sitefinity.Samples.EmptyProject/releases).

### Login

To login to Sitefinity backend, use the following credentials:  
**Username:** admin   
**Password:** password
