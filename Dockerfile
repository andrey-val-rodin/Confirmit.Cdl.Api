FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine as builder
RUN mkdir -p /artifacts/PublishOutput
ENV TEAMCITY_PROJECT_NAME=Confirmit.Cdl.Api
RUN mkdir -p /src/app/Confirmit.Cdl.Api
WORKDIR  /src/app/Confirmit.Cdl.Api

COPY NuGet.config code/Confirmit.Cdl.Api.sln ./code/
COPY code/src/Confirmit.Cdl.Api/Confirmit.Cdl.Api.csproj ./code/src/Confirmit.Cdl.Api/
COPY code/test/Confirmit.Cdl.Api.xUnitTests/Confirmit.Cdl.Api.xUnitTests.csproj ./code/test/Confirmit.Cdl.Api.xUnitTests/
COPY code/test/Confirmit.Cdl.Api.xIntegrationTests/Confirmit.Cdl.Api.xIntegrationTests.csproj ./code/test/Confirmit.Cdl.Api.xIntegrationTests/
RUN dotnet restore ./code/

COPY . .
RUN dotnet test --no-restore --verbosity normal code/test/Confirmit.Cdl.Api.xUnitTests
RUN dotnet publish --no-restore --configuration Release --output /artifacts/PublishOutput code/src/Confirmit.Cdl.Api/Confirmit.Cdl.Api.csproj

FROM dockerv2-confirmit-local.kube.firmglobal.com:30100/confirmit/whitesource-environment:1 as whitesource-scanning
ARG OSS_Params
ENV OSS_Params ${OSS_Params}
COPY --from=builder /src/app/Confirmit.Cdl.Api/code /usr/src/data

FROM builder as app-builder
RUN rm /artifacts/PublishOutput/web.config

FROM builder as integrationtest-runner
ENV Confirmit__ContainerEnvironment true
ARG Confirmit__ServiceVersion
ENV Confirmit__ServiceVersion ${Confirmit__ServiceVersion}
ENTRYPOINT ["dotnet", "test", "--no-restore", "--no-build", "--verbosity", "normal", "code/test/Confirmit.Cdl.Api.xIntegrationTests"]
RUN dotnet build --no-restore code/test/Confirmit.Cdl.Api.xIntegrationTests

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine as app-production
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV Confirmit__ContainerEnvironment true
ENV Confirmit__Logging__EnableLogToStdStreams true
ARG Confirmit__ServiceVersion
ENV Confirmit__ServiceVersion ${Confirmit__ServiceVersion}
ARG Confirmit__InstallationDate
ENV Confirmit__InstallationDate ${Confirmit__InstallationDate}
WORKDIR /app
EXPOSE 5000
ENTRYPOINT ["dotnet", "Confirmit.Cdl.Api.dll"]
COPY --from=app-builder /artifacts/PublishOutput/ .

FROM octopusdeploy/octo:5.2.2-alpine as nuget-production
WORKDIR /src
RUN mkdir -p /OctopusDeploy/Confirmit.Deployment.Deploy2IISWithOctopus \
 && wget -O /OctopusDeploy/Confirmit.Deployment.Deploy2IISWithOctopus.nupkg https://co-osl-nuget02.firmglobal.com/nuget/confirmit-nuget/package/Confirmit.Deployment.Deploy2IISWithOctopus/21.2.1 \
 && unzip /OctopusDeploy//Confirmit.Deployment.Deploy2IISWithOctopus.nupkg -d /OctopusDeploy/Confirmit.Deployment.Deploy2IISWithOctopus
RUN mkdir DeployScripts \
 && cp \
    /OctopusDeploy/Confirmit.Deployment.Deploy2IISWithOctopus/Content/DeployScripts/ConfigurationFileUtils.ps1 \
    /OctopusDeploy/Confirmit.Deployment.Deploy2IISWithOctopus/Content/DeployScripts/ConfirmitUtils.ps1 \
    /OctopusDeploy/Confirmit.Deployment.Deploy2IISWithOctopus/Content/DeployScripts/BaseMethods.ps1 \
    /OctopusDeploy/Confirmit.Deployment.Deploy2IISWithOctopus/Content/DeployScripts/IISConfigurationMethods.ps1 \
    /OctopusDeploy/Confirmit.Deployment.Deploy2IISWithOctopus/Content/DeployScripts/IISConfiguration.ps1 \
    DeployScripts/
ARG Confirmit__AppVersion
COPY --from=builder /artifacts/PublishOutput/ .
COPY OctopusDeploy/DeployScripts/Local.Config.ps1 \
     ./DeployScripts/
COPY OctopusDeploy/Deploy.ps1 \
     OctopusDeploy/PostDeploy.ps1 \
     ./
RUN octo pack \
    --id=Confirmit.Cdl.Api \
    --format=NuPkg \
    --version=${Confirmit__AppVersion} \
    --outFolder=/artifacts \
    --author=Confirmit \
    --title="Confirmit.Cdl.Api" \
    --description="Confirmit.Cdl.Api"