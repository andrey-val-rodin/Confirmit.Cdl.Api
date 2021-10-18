# How to use this template

The purpose of this template is to provide some guiding rules and best practices when it comes to developing RESTful apis.

This document will only discuss on the principals and usage of the ASP.Net core Web API project located in this template.

If you are advanced user you can jump to the post template installation task list [here -->>](#Post-template-installation-task-list)

## Structure of the ASP.Net core Web API project

This project is based on the default asp.net web api template that comes with the framework extending it with Confirmit related packages and usages.

After installing the template you will see the following structure

``` bash
.
+-- Controllers
|   +-- HealthzController.cs
+-- TemplateSample
|   +-- [folder structure omitted]
+-- DatabaseConfiguration.cs
+-- Program.cs
+-- readme.md [this file]
+-- Startup.cs
+-- StartupExtensions.cs
+-- web.config
```

The team responsible for the maintaining of this template (*microservices* clan) would like to frequently update the content and add new features and improvements; therefore is important to understand how this template should be used and what is content is.

Files marked with the **=== DO NOT MODIFY ===** header should not be modified. Most likely this files will be changed in a future release of the template and you may lose your data. This files are

- HealthzController.cs
- DatabaseConfiguration.cs
- Program.cs
- Startup.cs
- StartupExtensions.cs
- web.config

However if a change in this files is necessary then most likely this template needs to be updated, so please contact the *microservices* clan for instructions.

### Content of the TemplateSample folder

The content of the `TemplateSample` folder contains files that are provided as example and they should not be used in production. Feel free to modify or delete this folder.

## How to configure my ASP.Net Core service

First thing that you need to do once you install this template is to copy the `StartupExtensionsImpl.cs` file from the `TemplateSample` folder to the root of the project.

The `StartupExtensionsImpl.cs` is a `partial` class that should contain method implementations from the `StartupExtensions.cs` class. We have provided several `extension` methods in the `StartupExtensions.cs` that can be used to configure various services from the `MVC` framework or to just register your project service. The `Startup.cs` uses this `extension` methods from the `StartupExtensions.cs` and configures the services.

## How to configure my database

In case the new api has a corresponding database, the database needs to be created or upgraded on deployment of the api.
The upgrade is done in `Program.cs`, calling the `DatabaseConfiguration.UpgradeDatabase()` function to perform the upgrade steps via a partial class method. If you need a database, copy the `DatabaseConfigurationImpl.cs` class from the `TemplateSample` folder and put it in your root folder.
We recommend using [FluentMigrator](https://www.nuget.org/packages/FluentMigrator/) to do the upgrade. If the database is used both from a rest api and an agent or a task, the upgrade functionality should be put in a nuget that can be used from all applications.
The database nuget should be called `Confirmit.Domain.Database`. See [Confirmit.Hub.Vault.Database](https://stashosl.firmglobal.com/projects/NUGET/repos/confirmit.hub.vault.database/browse) for an example using FluentMigrator.

## Authorization

ASP.NET core introduced new `policy-based` authorization that you should use in your project. The official documentation has good information and you should familiarize your self with this concepts.

- Declarative policy-based authorization [Policy-based authorization in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/policies)
- Imperative policy-based authorization [Resource-based authorization in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/resourcebased)

The `Confirmit.NetCore.Authorization` nuget package contains such commonly used `Confirmit` policies that may be reused in your project.

### How to configure authorization

If you just want to protect your api and require that the user is authorized (has valid token) then add this code in your `AddLocalServicesImpl` extension method in the `StartupExtensionsImpl.cs` class

```cs
services.AddConfirmitDefaultAuthorization(configuration, "DefaultAccessPolicy");
```

and then add the `Authorize` attribute on your controller with the name of the policy

```cs
[Authorize("DefaultAccessPolicy")]
public class ValuesController : Controller
{
}
```

Configuring the authorization on this way will only require that the token issued from identity has the introspection `scope` for your api.

Take a look at the `Authorization` folder in the `TemplateSample` if you want to see an example of more realistic implementation of policy-based authorization. The example provided there authorizes users with system administrator or company administrate roles. It also requires that you implement `IAuthorizationContextCompanyProvider` in your project and register such authorization services.

Feel free to contact the *microservices* clan if you need help or instructions with authorization.

## Post template installation task list

- [ ] Copy `StartupExtensionsImpl.cs` file from the `TemplateSample` folder to the root of the project.
- [ ] Copy `DatabaseConfigurationImpl.cs` file from the `TemplateSample` folder to the root of the project.
- [ ] Happy coding and have fun :=)