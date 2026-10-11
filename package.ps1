$ErrorActionPreference = "Stop"

$ProjectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$PackageName = "ValeriusNoobShield"
$Version = "1.0.2"

$Dist = Join-Path $ProjectRoot "dist"
$Package = Join-Path $Dist "$PackageName-$Version"
$Plugins = Join-Path $Package "BepInEx\plugins"

# Clean previous package
if (Test-Path $Dist) {
    Remove-Item $Dist -Recurse -Force
}

# Create package structure
New-Item -ItemType Directory -Path $Plugins -Force | Out-Null

# Build Release
dotnet build (Join-Path $ProjectRoot "ValeriusNoobShield.csproj") -c Release

# Copy Thunderstore metadata
Copy-Item (Join-Path $ProjectRoot "manifest.json") $Package
Copy-Item (Join-Path $ProjectRoot "README.md") $Package
Copy-Item (Join-Path $ProjectRoot "CHANGELOG.md") $Package
Copy-Item (Join-Path $ProjectRoot "icon.png") $Package

# Copy compiled plugin into BepInEx/plugins
$Dll = Join-Path $ProjectRoot "bin\Release\net48\ValeriusNoobShield.dll"
Copy-Item $Dll $Plugins

# Create ZIP
$Zip = Join-Path $Dist "$PackageName-$Version.zip"

Compress-Archive `
    -Path "$Package\*" `
    -DestinationPath $Zip `
    -Force

Write-Host ""
Write-Host "Package created:"
Write-Host $Zip