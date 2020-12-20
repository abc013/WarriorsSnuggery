$configs = @("linux_x64", "osx_x64", "windows_x64", "windows_x86")

$host.ui.RawUI.WindowTitle = "Warrior's Snuggery publishing"

$in = Read-Host "Please enter the configuration for publishing [available: $($configs -join ", ") ]"
if ($in -in $configs)
{
    echo ""
    dotnet publish /p:PublishProfile=$in
    echo ""
    Write-Host "Done!"
}
else
{
    Write-Host "Invalid configuration: $($in)" -ForegroundColor Red
}
Read-Host -Prompt "Press any key to exit"