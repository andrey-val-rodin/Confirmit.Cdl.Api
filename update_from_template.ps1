dotnet new -i Confirmit.Template.MicroService
Push-Location $PSScriptRoot
$hasTemplateSamples = (Get-ChildItem TemplateSample -Directory -Recurse).Count -gt 0
dotnet new micro -n Confirmit.Cdl.Api -o . -S api/cdl --force --allow-scripts yes
if(!$hasTemplateSamples) {
    Get-ChildItem TemplateSample -Directory -Recurse | Remove-Item -Recurse
}
Pop-Location