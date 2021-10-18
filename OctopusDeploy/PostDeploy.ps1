try
{
	. ".\DeployScripts\ConfirmitUtils.ps1"

	Remove-DeployScriptsDirectory

	# Any call to Cdl.Api will result in deployment of CmlStorage database. Use health readiness probe for this
	$response = Invoke-WebRequest http://localhost/api/cdl/healthz/ready -UseBasicParsing -TimeoutSec 30
	# This will only execute if the Invoke-WebRequest is successful
	$StatusCode = $Response.StatusCode
}
catch [Exception]
{ 
    "Failed to run package script:"
    $_.Exception.Message
    $_.Exception.StackTrace
    Exit -1
}