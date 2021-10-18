var repoName = "Confirmit.Cdl.Api";
var appNameShort = "cdl-api";
var healthzReadyPath = "healthz/ready";

#load "nuget:?package=Confirmit.Cake.DockerBuild&version=8.12.1"

templateVersion = "5.1.6";

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument<string>("target", "Default");

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

// Add dependent tasks that will run before the helm chart is installed
// This can be a good place to initialize the helmChartValuesYaml variable if it uses e.g. version information
Task("BeforeInstallHelmChart");

// Add dependent tasks that will run integration/end2end tests and before the Helm chart is deleted
// Use this if you need to do additional cleanup/uninstall tasks
Task("BeforeDeleteHelmChart");

//Not used for MicroService Api
Task("RunTestsUsingIngress");

// Add dependent tasks that will run after the application is deployed with Octopus
// When the RunTestsUsingOctopusPackage task and dependencies has completed the application version will be rolled back in Octopus
// Enable RunIntegrationTestContainerTargetOctopusPackage task to run all integration tests that is run using the Kubernetes deployed app also using the Octopus deployed app
Task("RunTestsUsingOctopusPackage");
    // .IsDependentOn("RunIntegrationTestContainerTargetOctopusPackage");

// Add dependent tasks that will run after all other tasks, but before the PromoteClientApp task
// This is the place to add additional tasks that needs to run to validate the build before it is promoted to SaaS
// The promote to SaaS is still only done if --promote=true and the version number is not a pre-release version number
Task("RunAdditionalTasksBeforePromote");

// Default task to run. You can optionally add tasks before and after the ClientApp task
Task("Default")
    .IsDependentOn("MicroServiceApi");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
