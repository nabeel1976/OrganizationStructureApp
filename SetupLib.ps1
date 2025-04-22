# SetupLib.ps1 - copy official SQLite DLLs into lib folder
$solutionDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$libPath = Join-Path $solutionDir 'lib'
$sourceDllFolder = 'C:\Program Files\System.Data.SQLite\2022\bin\x86'
$filesToCopy = @('System.Data.SQLite.dll','SQLite.Interop.dll')

if (-not (Test-Path $libPath)) {
    New-Item -ItemType Directory -Path $libPath | Out-Null
}

foreach ($file in $filesToCopy) {
    $src = Join-Path $sourceDllFolder $file
    $dest = Join-Path $libPath $file
    if (Test-Path $src) {
        Copy-Item -Path $src -Destination $dest -Force
        Write-Host "Copied $file to lib folder."
    } else {
        Write-Host "Source file not found: $src" -ForegroundColor Yellow
    }
}

Write-Host 'SetupLib.ps1 execution completed.' -ForegroundColor Green
