param(
    [string]$Configuration = "Release",
    [string]$SolutionPath = (Join-Path $PSScriptRoot "..\\src\\XIVLauncher.sln"),
    [switch]$SkipSubmodules,
    [switch]$SkipRestore,
    [switch]$SkipHashes
)

$ErrorActionPreference = "Stop"

function Get-MSBuildPath {
    if ($env:MSBUILD_EXE_PATH -and (Test-Path $env:MSBUILD_EXE_PATH)) {
        return $env:MSBUILD_EXE_PATH
    }

    $vswhere = "C:\\Program Files (x86)\\Microsoft Visual Studio\\Installer\\vswhere.exe"
    if (Test-Path $vswhere) {
        $found = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find "MSBuild\\Current\\Bin\\MSBuild.exe" 2>$null
        if ($found -and (Test-Path $found)) {
            return $found
        }
    }

    $fallbacks = @(
        "C:\\Program Files\\Microsoft Visual Studio\\2022\\Enterprise\\MSBuild\\Current\\Bin\\MSBuild.exe",
        "C:\\Program Files\\Microsoft Visual Studio\\2022\\Professional\\MSBuild\\Current\\Bin\\MSBuild.exe",
        "C:\\Program Files\\Microsoft Visual Studio\\2022\\Community\\MSBuild\\Current\\Bin\\MSBuild.exe",
        "C:\\Program Files\\Microsoft Visual Studio\\2022\\BuildTools\\MSBuild\\Current\\Bin\\MSBuild.exe"
    )

    foreach ($p in $fallbacks) {
        if (Test-Path $p) {
            return $p
        }
    }

    throw "MSBuild.exe not found. Install Visual Studio 2022 (with MSBuild) or set MSBUILD_EXE_PATH."
}

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$solutionFullPath = Resolve-Path $SolutionPath
$msbuild = Get-MSBuildPath

Write-Host "Repo: $repoRoot"
Write-Host "Solution: $solutionFullPath"
Write-Host "MSBuild: $msbuild"
Write-Host "Configuration: $Configuration"

if (-not $SkipSubmodules) {
    Write-Host "Updating submodules..."
    git submodule update --init --recursive
}

if (-not $SkipRestore) {
    Write-Host "Restoring NuGet packages..."
    Push-Location (Join-Path $repoRoot "src")
    dotnet restore
    Pop-Location
}

Write-Host "Building solution..."
& $msbuild $solutionFullPath /t:Build /p:Configuration=$Configuration /m

$buildOutDir = Join-Path $repoRoot "src\\bin\\win-x64"
if (Test-Path $buildOutDir) {
    Write-Host "Build output: $buildOutDir"
}
else {
    Write-Warning "Expected build output directory not found: $buildOutDir"
}

if (-not $SkipHashes) {
    Write-Host "Generating hashes.json..."
    & (Join-Path $repoRoot "scripts\\CreateHashList.ps1") $buildOutDir

    $releasesDir = Join-Path $repoRoot "src\\Releases"
    New-Item -ItemType Directory -Force -Path $releasesDir | Out-Null

    $hashesPath = Join-Path $buildOutDir "hashes.json"
    if (Test-Path $hashesPath) {
        Move-Item -Path $hashesPath -Destination $releasesDir -Force
        Write-Host "Hashes moved to: $releasesDir"
    }
    else {
        Write-Warning "hashes.json not found at: $hashesPath"
    }
}

Write-Host "Done."
