$config = @{
    ApplicationPools = @(
        @{
            Name = "Confirmit.Cdl.Api";
            FrameworkVersion = "";
			Enable32BitAppOnWin64 = $false
			AppPoolIdentityType = "ApplicationPoolIdentity";
        });
    Site = @{
        Name = "$ConfirmitServerIISWebSite";
        SiteRoot = (Join-Path $ConfirmitServerLocalPathConfirmitProgram "wwwroot");
        AppPoolName = "DefaultAppPool";
        Port = 80
        Application = @{
            Name = "api/cdl";
            AppPoolName = "Confirmit.Cdl.Api";
            ApplicationRoot = $OctopusOriginalPackageDirectoryPath
        }
    };
}
