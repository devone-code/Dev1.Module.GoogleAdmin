del "*.nupkg"
"..\..\oqtane.framework\oqtane.package\nuget.exe" pack Dev1.Module.GoogleAdmin.nuspec 
XCOPY "*.nupkg" "..\..\oqtane.framework\Oqtane.Server\Packages\" /Y

