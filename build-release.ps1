$ErrorActionPreference = "Stop"

# Проект приложения Avalonia
$Project = "Philadelphus.Presentation.Avalonia\Philadelphus.Presentation.Avalonia.csproj"

# Проект, из которого берем версию
$VersionProject = "Philadelphus.Presentation\Philadelphus.Presentation.csproj"

$runtimes = @(
    "win-x64",
    "linux-x64",
    "osx-x64",
    "osx-arm64"
)

Write-Host "Reading version..."

[xml]$versionProj = Get-Content $VersionProject

$assemblyVersionNode = $versionProj.SelectSingleNode("//AssemblyVersion")

if ($null -eq $assemblyVersionNode)
{
    throw "AssemblyVersion not found in $VersionProject"
}

$Version = $assemblyVersionNode.InnerText.Trim()

# Убираем последний .0 если версия вида 1.1.9.0
# $Version = $Version -replace '\.0$',''

Write-Host "Version: $Version"

$PublishRoot = Join-Path $PSScriptRoot "publish"
$ReleaseRoot = Join-Path $PSScriptRoot "release"

Write-Host "Cleaning old artifacts..."

Remove-Item $PublishRoot -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item $ReleaseRoot -Recurse -Force -ErrorAction SilentlyContinue

New-Item -ItemType Directory -Path $PublishRoot | Out-Null
New-Item -ItemType Directory -Path $ReleaseRoot | Out-Null

foreach ($rid in $runtimes)
{
    Write-Host ""
    Write-Host "========================================"
    Write-Host "Building $rid"
    Write-Host "========================================"

    $outputDir = Join-Path $PublishRoot $rid

    dotnet publish `
        $Project `
        -c Release `
        -r $rid `
        --self-contained true `
        -p:PublishSingleFile=true `
        -o $outputDir

    if ($LASTEXITCODE -ne 0)
    {
        throw "Build failed for $rid"
    }

    $archiveName = "Philadelphus-$Version-$rid.zip"
    $archivePath = Join-Path $ReleaseRoot $archiveName

    if (Test-Path $archivePath)
    {
        Remove-Item $archivePath -Force
    }

    Compress-Archive `
        -Path "$outputDir\*" `
        -DestinationPath $archivePath `
        -Force

    Write-Host "Created archive:"
    Write-Host $archivePath
}

Write-Host ""
Write-Host "========================================"
Write-Host "Build completed successfully"
Write-Host "Artifacts:"
Write-Host $ReleaseRoot
Write-Host "========================================"