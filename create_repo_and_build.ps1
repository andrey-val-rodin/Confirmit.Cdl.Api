$ErrorActionPreference = 'stop'

$RepoName = "Confirmit.Cdl.Api"

<#
.SYNOPSIS
    Generic helper cmdlet to invoke Rest methods agains a target BitBucket server.
.DESCRIPTION
    This cmdlet extends the original Invoke-WebRequest cmdlet with BitBucket REST
    API specific parameters, so it does user authorization and provides easier resource access.
.PARAMETER Resource
    Mandatory - BitBucket REST API Resource that needs to be accessed
.PARAMETER Method
    Optional - REST method to be used for the call. (Default is GET)
.PARAMETER ApiVersion
    Optional - REST API version that needs to be targeted for the call. Default is the latest.
.PARAMETER AuthenticationToken
    Optional - Authentication Token to access BitBucket Server
.PARAMETER Body
    Optional - HTTP Body json
.EXAMPLE
    Invoke-BitBucketWebRequest -Resource "projects"
.EXAMPLE
    Invoke-BitBucketWebRequest -Resource "porjects" -Method Get
#>
function Invoke-BitBucketWebRequest {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$Resource,

        [ValidateSet('Get','Put','Post','Delete')]
        [string]$Method = 'Get',

        [string]$ApiVersion='latest',
        [string]$Server = $script:BitBucketServer,
        [string]$APIUrl="$Server/rest/api",
        [string]$BranchApiVersion='2.0',
        [string]$BranchPermissionApiUri="$Server/rest/branch-permissions/$BranchApiVersion/$Resource",        
        [string]$AuthenticationToken = $script:AuthenticationToken,
        [string]$ContentType='application/json',
        [psobject]$Headers=@{},
        [psobject]$Body
    )
    [string]$ResourceUrl="$APIUrl/$ApiVersion/$Resource"
    #write-host "[Info:] URI: $ResourceUrl, Method: $Method, AuthToken: $AuthenticationToken"
    $Headers.Authorization = "Basic $AuthenticationToken"
    $WebRequestResponse = $null

    try {
        if ($Method -eq "Get")
        {
            $WebRequestResponse = Invoke-WebRequest -Uri "$ResourceUrl" -Method $Method -Headers $Headers -UseBasicParsing  #-ContentType $ContentType
        }
        else {
            if (-not ([string]::IsNullOrEmpty($Body)))
            {
                $WebRequestResponse = Invoke-WebRequest -Uri "$ResourceUrl" -Method $Method -Headers $Headers -UseBasicParsing -ContentType $ContentType -Body $Body
            }
            else {
                Write-Host "[info] Body param is mandatory for Put/Post request"
            }
        }
    } catch {
        if ($_.ErrorDetails) {
            Write-Error $_.ErrorDetails.Message
        } else {
            Write-Error $_
        }
    }
    #Write-Verbose "Response: $WebRequestResponse"
    return $WebRequestResponse
}

<#
.SYNOPSIS
    Sets the credential for the target BitBucket Server.
.DESCRIPTION
    All further cmdlets from Ps.BitBucket will be executed with the Authentication details passed by this command.
.PARAMETER Credential
    PSCredential to be used to login to the target BitBucket server
.EXAMPLE
    Set-BitBucketAuthentication -Credential username
#>

function Set-BitBucketCredential {
    [CmdletBinding(DefaultParameterSetName="ByCredential")]
    [Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSUsePSCredentialType", "Credential")]
    param(
        [Parameter(Mandatory=$true,ParameterSetName='ByCredential')]
        [System.Management.Automation.PSCredential]
        [System.Management.Automation.CredentialAttribute()]
        $Credential
    )
    # Get UserName and Password from Credential
    $UserName=$Credential.UserName
    $Password = $null
    if ($Credential.GetNetworkCredential()) {
        $Password=$Credential.GetNetworkCredential().password
    } else {
        $Password=[Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($Credential.password))
    }
    $script:UserName=$UserName
    $script:AuthenticationToken=[Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f $UserName,$Password)))
}

<#
.SYNOPSIS
    Sets the target BitBucket Server
.DESCRIPTION
    All further cmdlets from Ps.BitBucket will be executed against the server-enpoint specified by this cmdlet.
.PARAMETER Url
    Mandatory - Fully qualified HTTP endpoint for the target BitBucket Server.
.EXAMPLE
    Set-BitBucketServer -Url "http://localhost:7990"
#>
function Set-BitBucketServer {
    param(
        [Parameter(Mandatory)]
        [ValidatePattern('^(https?:\/\/)([\w\.-]+)(:\d+)*\/*')]
        [string]$Url
    )
    $script:BitBucketServer = $Url
}

<#
.SYNOPSIS
    Gets the list of repos under given project.
.DESCRIPTION
.PARAMETER project
    Mandatory - project id
.EXAMPLE
    Get-BitBucketRepoByProject -Project "TES"
#>
function Get-BitBucketRepoByProject {
    [CmdletBinding()]param(
        [Parameter(Mandatory=$true,ParameterSetName='Project')]
        [ValidateNotNullOrEmpty()]
        [string]$Project
    )

    $Manifest = Invoke-BitBucketWebRequest -Resource "projects/$Project/repos?limit=1000" | ConvertFrom-Json
    #Write-Output "[List:] Repos under project - $Project"
    return $Manifest.values.name
}

<#
.SYNOPSIS
    Create new repository under given project
.DESCRIPTION
.PARAMETER Project
    Mandatory - Bitbucket Project ID
.PARAMETER Repository
    Mandatory - New Repository name to be created
.EXAMPLE
    New-BitBucketRepo -Project "TES" -Repository "ABC"
#>
function New-BitBucketRepo {
    [CmdletBinding()]param(
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string]$Project,
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string]$Repository
    )
    try
    {
        $JsonBody = @{
            name        = $Repository 
            scmId       = 'git'
            forkable    = 'false'
        } | ConvertTo-Json

        $Manifest = Invoke-BitBucketWebRequest -Resource "projects/$Project/repos" -Method Post -Body $JsonBody #| ConvertFrom-Json
        $Manifest1 = $Manifest | ConvertFrom-Json
        #$Status = $Manifest1.State
        if ($Manifest1.State -eq "AVAILABLE")
        {
            Write-Host "[Creation][Successful] URL: $script:BitBucketServer/projects/${Project}/repos/${Repository}/browse" -ForegroundColor Cyan
        }
        else {
            Write-Host "[Creation][Failed]"
        }
    }
    catch [System.Exception] 
    {
        Write-Host "[Return Message:] $Manifest"
        Throw $_.Exception.Message;
    }
}

function Add-Repository($credential)
{
    $BitBucketProjectName = "APP"
    $BitBuckerUrl = "https://stashosl.firmglobal.com"
    Set-BitBucketServer -Url $BitBuckerUrl
    Set-BitBucketCredential -Credential $credential
    $repos = Get-BitBucketRepoByProject -Project $BitBucketProjectName
    if($repos -contains $RepoName) {
        Write-Host "BitBucket repository '$RepoName' already exists" -ForegroundColor Cyan
    }
    else {
        $repo = New-BitBucketRepo -Project $BitBucketProjectName -Repository $RepoName
        Write-Host "BitBucket repository '$RepoName' created. URL: '$BitBuckerUrl/projects/$BitBucketProjectName/repos/$RepoName'" -ForegroundColor Green
    }
}

function Add-Build($credential)
{
    $RestApiBaseUrl = "http://teamcity/httpAuth/app/rest"
    $ParentProjectId = "ConfirmitHorizonsApplicationsV190"
    $ProjectId = $ParentProjectId + "_" + ($RepoName.Replace(".", ""))
    $DockerHelmProjectName = "Build Docker image and Helm chart"
    $CakeBuildTemplateId = "ConfirmitHorizonsApplicationsV190_ConfirmitCakeDockerBuildTemplate"
    $CakeBuildName = $RepoName + "-Cake"
    $CakeBuildWhitesourceTemplateId = "ConfirmitHorizonsApplicationsV190_ConfirmitCakeDockerBuildWhitesourceTemplate"
    $CakeBuildLegacyWhitesourceName = $RepoName + "-Nuget.Whitesource"
    $CakeBuildWhitesourceName = $RepoName + "-Whitesource"
    $headers = @{}
    $headers.Add("Accept","application/json")
    $headers.Add("Origin", "http://teamcity")
    try {
        $project = Invoke-RestMethod -Uri "$RestApiBaseUrl/projects/id:$ProjectId" -Credential $credential -Method Get -UseBasicParsing -Headers $headers
    } catch {
        if($_.Exception.Response.StatusCode -ne 404) {
            throw
        }
        $project = $null
    }
    if($project) {
        Write-Host "TeamCity build project '$RepoName' already exists" -ForegroundColor Cyan
    }
    else {
        $project = Invoke-RestMethod -Uri "$RestApiBaseUrl/projects" -Credential $credential -Method Post -UseBasicParsing -Headers $headers -ContentType "application/xml" -Body "<newProjectDescription name='$RepoName' id='$ProjectId'><parentProject locator='id:$ParentProjectId'/></newProjectDescription>"
        Write-Host "TeamCity project '$RepoName' created. URL: 'http://teamcity/project.html?projectId=$ProjectId'" -ForegroundColor Green
    }

    $buildTypes = Invoke-RestMethod -Uri "$RestApiBaseUrl/projects/id:$ProjectId/buildTypes" -Credential $credential -Method Get -UseBasicParsing -Headers $headers
    $legacyCakeBuild = ($buildTypes.buildType | Where-Object {$_.name -eq $RepoName})
    if($legacyCakeBuild) {
        Invoke-RestMethod -Verbose -Uri "$RestApiBaseUrl/buildTypes/id:$($legacyCakeBuild.id)" -Credential $credential -Method Delete -UseBasicParsing -Headers $headers
        Write-Host "TeamCity build configuration '$RepoName' deleted" -ForegroundColor Yellow
    }

    $legacyCakeBuild = ($buildTypes.buildType | Where-Object {$_.name -eq $DockerHelmProjectName})
    if($legacyCakeBuild) {
        Invoke-RestMethod -Uri "$RestApiBaseUrl/buildTypes/id:$($legacyCakeBuild.id)" -Credential $credential -Method Delete -UseBasicParsing -Headers $headers
        Write-Host "TeamCity build configuration '$DockerHelmProjectName' deleted" -ForegroundColor Yellow
    }

    $legacyWhitesourceBuild = ($buildTypes.buildType | Where-Object {$_.name -eq $CakeBuildLegacyWhitesourceName})
    if($legacyWhitesourceBuild) {
        Invoke-RestMethod -Uri "$RestApiBaseUrl/buildTypes/id:$($legacyWhitesourceBuild.id)" -Credential $credential -Method Delete -UseBasicParsing -Headers $headers
        Write-Host "TeamCity build configuration '$CakeBuildLegacyWhitesourceName' deleted" -ForegroundColor Yellow
    }
   
    $buildTypes = Invoke-RestMethod -Uri "$RestApiBaseUrl/projects/id:$ProjectId/buildTypes" -Credential $credential -Method Get -UseBasicParsing -Headers $headers
    if($buildTypes.buildType.name -eq $CakeBuildName) {
        Write-Host "TeamCity Cake build configuration already exists" -ForegroundColor Cyan
    }
    else {
        $buildType = Invoke-RestMethod -Uri "$RestApiBaseUrl/projects/id:$ProjectId/buildTypes" -Credential $credential -Method Post -UseBasicParsing -Headers $headers -ContentType "text/plain" -Body $CakeBuildName
        Write-Host "TeamCity build configuration '$CakeBuildName' created" -ForegroundColor Green
        $template = Invoke-RestMethod -Uri "$RestApiBaseUrl/buildTypes/id:$($BuildType.id)/template" -Credential $credential -Method Put -UseBasicParsing -Headers $headers -ContentType "text/plain" -Body $CakeBuildTemplateId
        Write-Host "TeamCity build configuration '$CakeBuildName' attached to template '$($template.name)'" -ForegroundColor Green
    }

    if($buildTypes.buildType.name -eq $CakeBuildWhitesourceName) {
        Write-Host "TeamCity Cake build configuration already exists" -ForegroundColor Cyan
    }
    else {
        $buildType = Invoke-RestMethod -Uri "$RestApiBaseUrl/projects/id:$ProjectId/buildTypes" -Credential $credential -Method Post -UseBasicParsing -Headers $headers -ContentType "text/plain" -Body $CakeBuildWhitesourceName
        Write-Host "TeamCity build configuration '$CakeBuildWhitesourceName' created" -ForegroundColor Green
        $template = Invoke-RestMethod -Uri "$RestApiBaseUrl/buildTypes/id:$($BuildType.id)/template" -Credential $credential -Method Put -UseBasicParsing -Headers $headers -ContentType "text/plain" -Body $CakeBuildWhitesourceTemplateId
        Write-Host "TeamCity build configuration '$CakeBuildWhitesourceName' attached to template '$($template.name)'" -ForegroundColor Green
    }

    $buildTypes = Invoke-RestMethod -Uri "$RestApiBaseUrl/projects/id:$ProjectId/buildTypes" -Credential $credential -Method Get -UseBasicParsing -Headers $headers
    $cakeBuildId = ($buildTypes.buildType | Where-Object {$_.name -eq $CakeBuildName}).id
    $ConfirmitRepoNameJson = '{"name":"Confirmit.RepoName","value":"' + $RepoName+ '"}'
    Invoke-RestMethod -Uri "$RestApiBaseUrl/buildTypes/$cakeBuildId/parameters/Confirmit.RepoName" -Credential $credential -Method Put -UseBasicParsing -Headers $headers -ContentType "application/json" -Body $ConfirmitRepoNameJson
    $ConfirmitLockOctopusEnvironmentJson = '{"name":"Confirmit.LockOctopusEnvironment","value":"true"}'
    Invoke-RestMethod -Uri "$RestApiBaseUrl/buildTypes/$cakeBuildId/parameters/Confirmit.LockOctopusEnvironment" -Credential $credential -Method Put -UseBasicParsing -Headers $headers -ContentType "application/json" -Body $ConfirmitLockOctopusEnvironmentJson

    $cakeBuildWhitesourceId = ($buildTypes.buildType | Where-Object {$_.name -eq $CakeBuildWhitesourceName}).id

    Invoke-RestMethod -Uri "$RestApiBaseUrl/buildTypes/$cakeBuildWhitesourceId/parameters/Confirmit.RepoName" -Credential $credential -Method Put -UseBasicParsing -Headers $headers -ContentType "application/json" -Body $ConfirmitRepoNameJson

    $snapshotDependencies = Invoke-RestMethod -Uri "$RestApiBaseUrl/buildTypes/$cakeBuildWhitesourceId/snapshot-dependencies" -Credential $credential -Method Get -UseBasicParsing -Headers $headers
    if($snapshotDependencies.count -eq 0) {
        Write-Host "Creating snapshot dependency for whitesource configuration..." -ForegroundColor Green
        $snapshotDependencyJson = '{"type":"snapshot_dependency","properties":{"count":6,"property":[{"name":"run-build-if-dependency-failed","value":"MAKE_FAILED_TO_START"},{"name":"run-build-if-dependency-failed-to-start","value":"MAKE_FAILED_TO_START"},{"name":"run-build-on-the-same-agent","value":"false"},{"name":"sync-revisions","value":"true"},{"name":"take-started-build-with-same-revisions","value":"true"},{"name":"take-successful-builds-only","value":"true"}]},"source-buildType":{"id":"'+$cakeBuildId+'"}}'
        Invoke-RestMethod -Uri "$RestApiBaseUrl/buildTypes/$cakeBuildWhitesourceId/snapshot-dependencies" -Credential $credential -Method Post -UseBasicParsing -Headers $headers -ContentType "application/json" -Body $snapshotDependencyJson
    }

    $triggers = Invoke-RestMethod -Uri "$RestApiBaseUrl/buildTypes/$cakeBuildWhitesourceId/triggers" -Credential $credential -Method Get -UseBasicParsing -Headers $headers
    if ($triggers.count -eq 0) {
$triggerBody = @"
<trigger type="buildDependencyTrigger">
    <properties>
        <property name="dependsOn" value="$cakeBuildId"/>
        <property name="afterSuccessfulBuildOnly" value="true"/>
        <property name="branchFilter" value="+:&lt;default&gt;"/>
    </properties>
</trigger>
"@;
        Invoke-RestMethod -Uri "$RestApiBaseUrl/buildTypes/$cakeBuildWhitesourceId/triggers" -Credential $credential -Method Post -UseBasicParsing -Headers $headers -ContentType "application/xml" -Body $triggerBody
        Write-Host "Whitesource build configuration trigger configured" -ForegroundColor Green
    }    


}

$credential = Get-Credential $env:username

Add-Repository $credential
Add-Build $credential
