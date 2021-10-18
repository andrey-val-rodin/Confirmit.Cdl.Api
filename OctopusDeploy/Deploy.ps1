try
{
	. ".\DeployScripts\IISConfiguration.ps1"
	. ".\DeployScripts\IISConfigurationMethods.ps1"
	SetUpEncryptedConfiguration $null "cdl" "Confirmit.Cdl.Api"

}
catch [Exception]
{ 
    "Failed to run package script:"
    $_.Exception.Message
    $_.Exception.StackTrace
    Exit -1
}