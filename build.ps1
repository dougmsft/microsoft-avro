<#
.SYNOPSIS
Builds Avro with the specified parameters
.DESCRIPTION
Builds Avro with the specified parameters
#>
param
    (
        # Build configuration
        [Parameter(Mandatory=$False, HelpMessage=".NET framework to build (netcoreapp1.0, net46)", Position=1)]
        [string]$Configuration="netcoreapp1.0",
        # Restore option
        [Parameter(Mandatory=$False, HelpMessage="Upate nuget packages and dependencies", Position=2)]
        [switch]$Restore=$False,
        # Clean option
        [Parameter(Mandatory=$False, HelpMessage="Remove all build previous build artifacts", Position=3)]
        [switch]$Clean=$False,
        # Clean only option
        [Parameter(Mandatory=$False, HelpMessage="Only remove all build previous build artifacts", Position=4)]
        [switch]$CleanOnly=$False
    )

Write-Host "Microsoft Avro Build" -ForegroundColor Green

$parent = Get-Location
$children = $core = "Microsoft.Avro.Core", "Microsoft.Avro.Tools", "Microsoft.Avro.Tests", "AvroTestApp"

$tool = Join-Path $parent "Microsoft.Avro.Tools"
$tests = Join-Path $parent "Microsoft.Avro.Tests"
$app = Join-Path $parent "AvroTestApp"

ForEach ($child in $children) {
    Write-Host "Building in $child" -ForegroundColor Green
    $path = Join-Path $parent $child
    Set-Location -LiteralPath $path
    if (($Clean -or CleanOnly) -and (Test-Path bin))
    {
        Write-Host "Deleting bin directory" -ForegroundColor Red
        Remove-Item -Path bin -Recurse
    }
    if (($Clean -or $CleanOnly) -and (Test-Path obj))
    {
        Write-Host "Deleting obj directory" -ForegroundColor Red
        Remove-Item -Path obj -Recurse
    }
    if (-not $CleanOnly)
    {
        if ($Restore)
        {
            dotnet restore
        }
        dotnet build -f $Configuration
    }
    }

Set-Location $parent
