param([string]$Compiler, [string]$OutputDirectory, [string]$BuildDirectory)
$ErrorActionPreference = 'Stop'
if (-not $OutputDirectory) { $OutputDirectory = Join-Path $PSScriptRoot 'release' }
$OutputDirectory = [IO.Path]::GetFullPath($OutputDirectory)
if (-not $BuildDirectory) { $BuildDirectory = Join-Path $PSScriptRoot 'work\build' }
$buildDirectory = [IO.Path]::GetFullPath($BuildDirectory)
New-Item -ItemType Directory -Force -Path $buildDirectory,$OutputDirectory | Out-Null
if (-not $Compiler) {
    $Compiler = Join-Path $buildDirectory 'compiler/tasks/net472/csc.exe'
    if (-not (Test-Path -LiteralPath $Compiler)) {
        $package = Join-Path $buildDirectory 'compiler.zip'
        Invoke-WebRequest 'https://api.nuget.org/v3-flatcontainer/microsoft.net.compilers.toolset/4.8.0/microsoft.net.compilers.toolset.4.8.0.nupkg' -OutFile $package
        Expand-Archive -LiteralPath $package -DestinationPath (Join-Path $buildDirectory 'compiler') -Force
    }
}
$Compiler = [IO.Path]::GetFullPath($Compiler)
Push-Location $PSScriptRoot
try {
    $iconBuilder = Join-Path $buildDirectory 'GenerateIcon.exe'
    & $Compiler /nologo /codepage:65001 /target:exe "/out:$iconBuilder" /r:System.Drawing.dll /resource:logo.png,ArchLight.Logo.png GenerateIcon.cs LogoArtwork.cs
    if ($LASTEXITCODE -ne 0) { throw 'Icon builder compilation failed' }
    Push-Location $buildDirectory
    try {
        & $iconBuilder
        if ($LASTEXITCODE -ne 0) { throw 'Icon generation failed' }
    } finally { Pop-Location }
    $generatedIcon = Join-Path $buildDirectory 'app.ico'
    foreach ($architecture in @('x64','x86','arm64')) {
        $stage = Join-Path $buildDirectory $architecture
        New-Item -ItemType Directory -Force -Path $stage | Out-Null
        $application = Join-Path $stage 'ArchLight.exe'
        & $Compiler /nologo /codepage:65001 /deterministic+ /target:winexe "/platform:$architecture" /optimize+ "/win32icon:$generatedIcon" "/out:$application" /r:System.Windows.Forms.dll /r:System.Drawing.dll /resource:logo.png,ArchLight.Logo.png Program.cs Settings.cs StartupRegistration.cs LogoArtwork.cs ModernUI.cs
        if ($LASTEXITCODE -ne 0) { throw "Application build failed: $architecture" }
        $uninstaller = Join-Path $stage 'Uninstall.exe'
        & $Compiler /nologo /target:winexe "/platform:$architecture" /optimize+ "/out:$uninstaller" /r:System.Windows.Forms.dll Uninstaller.cs StartupRegistration.cs
        if ($LASTEXITCODE -ne 0) { throw "Uninstaller build failed: $architecture" }
        $installer = Join-Path $OutputDirectory "ArchLight-Installer.win-$architecture.exe"
        & $Compiler /nologo /codepage:65001 /deterministic+ /target:winexe "/platform:$architecture" /optimize+ "/win32icon:$generatedIcon" "/out:$installer" "/resource:$application,ArchLight.exe" "/resource:$uninstaller,Uninstall.exe" /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:Microsoft.CSharp.dll /resource:logo.png,ArchLight.Logo.png Installer.cs LogoArtwork.cs Settings.cs StartupRegistration.cs
        if ($LASTEXITCODE -ne 0) { throw "Installer build failed: $architecture" }
        Copy-Item -LiteralPath README.md -Destination (Join-Path $stage 'README.md')
        Compress-Archive -Path $application,(Join-Path $stage 'README.md') -DestinationPath (Join-Path $OutputDirectory "ArchLight.win-$architecture.zip") -Force
    }
    $dirty = & git status --porcelain --untracked-files=no
    if ($LASTEXITCODE -ne 0 -or $dirty) { throw 'Release source must be built from a clean committed checkout.' }
    & git archive --format=zip --prefix=ArchLight/ "--output=$(Join-Path $OutputDirectory 'Source.code.zip')" HEAD
    if ($LASTEXITCODE -ne 0) { throw 'Source ZIP archive failed' }
    & git archive --format=tar.gz --prefix=ArchLight/ "--output=$(Join-Path $OutputDirectory 'Source.code.tar.gz')" HEAD
    if ($LASTEXITCODE -ne 0) { throw 'Source tar archive failed' }
    $sums = Get-ChildItem -LiteralPath $OutputDirectory -File | Where-Object { $_.Name -match '^(ArchLight.*\.(exe|zip)|Source\.code\.(zip|tar\.gz))$' } | Sort-Object Name | ForEach-Object {
        (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant() + '  ' + $_.Name
    }
    [IO.File]::WriteAllLines((Join-Path $OutputDirectory 'SHA256SUMS.txt'), [string[]]$sums, (New-Object Text.UTF8Encoding($false)))
} finally { Pop-Location }





