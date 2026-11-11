param([string]$Compiler, [string]$OutputDirectory)
$ErrorActionPreference = 'Stop'
if (-not $Compiler) { $Compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe' }
if (-not $OutputDirectory) { $OutputDirectory = Join-Path $PSScriptRoot 'work\tests' }
$OutputDirectory = [IO.Path]::GetFullPath($OutputDirectory)
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
Push-Location $PSScriptRoot
try {
    foreach ($test in @('RecoveryTests','RecoveryWorkloadTests','StabilityTests','BrandTests','SettingsTests')) {
        $executable = Join-Path $OutputDirectory "$test.exe"
        if ($test -eq 'SettingsTests') {
            & $Compiler /nologo /codepage:65001 /target:exe /main:ArchLight.Tests.SettingsTests "/out:$executable" Settings.cs SettingsTests.cs
        } else {
            & $Compiler /nologo /codepage:65001 /target:exe "/main:$test" "/out:$executable" /r:System.Windows.Forms.dll /r:System.Drawing.dll /resource:logo.png,ArchLight.Logo.png Program.cs Settings.cs StartupRegistration.cs LogoArtwork.cs ModernUI.cs "$test.cs"
        }
        if ($LASTEXITCODE -ne 0) { throw "Compilation failed: $test" }
        if ($test -eq 'BrandTests') { & $executable (Join-Path $OutputDirectory 'brand') }
        else { & $executable }
        if ($LASTEXITCODE -ne 0) { throw "Tests failed: $test" }
    }
} finally { Pop-Location }
