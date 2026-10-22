# ArchLight

A lightweight Windows night-light utility with adjustable screen color temperature, brightness controls, automatic scheduling, and a customizable interface.

## Download ArchLight 0.5.3

Download the package for your Windows architecture from [Releases](https://github.com/archfirein/ArchLight/releases/latest).

| Windows architecture | Installer | Portable ZIP |
| --- | --- | --- |
| x64 — most Intel/AMD 64-bit PCs | `ArchLight-Installer.win-x64.exe` | `ArchLight.win-x64.zip` |
| x86 — 32-bit Windows | `ArchLight-Installer.win-x86.exe` | `ArchLight.win-x86.zip` |
| ARM64 — Windows on ARM | `ArchLight-Installer.win-arm64.exe` | `ArchLight.win-arm64.zip` |

Check **Settings > System > About > System type** if you are unsure which package to choose. x64 and x86 builds require .NET Framework 4 or later. The ARM64 build requires an ARM64-capable .NET Framework 4.8.1 installation on a compatible Windows version.

## Features

- Adjust color temperature from **1800 K to 6500 K**.
- Adjust screen brightness from **25% to 100%** using software controls.
- Toggle the filter with the night-mode button or **Ctrl+Alt+N**.
- Choose manual operation, custom hours, or sunrise/sunset scheduling based on your selected location.
- Choose from five saved interface themes: **Yellow, White, Red, Blue, and Black**.
- Use the interface in **English, Turkish, Spanish, French, Chinese, Japanese, or Korean**. Theme names follow the selected language.
- Use a compact custom header, rounded controls, and a system-tray icon.
- Retain preferences and location across restarts, with atomic settings writes and backup recovery.

Interface themes change the application's appearance; color temperature and brightness control the screen filter separately.

## Getting started

1. Download the installer or portable ZIP matching your Windows architecture.
2. Run the installer, or extract the entire portable ZIP and open `ArchLight.exe`.
3. Select your language and theme from the header.
4. Set your preferred color temperature and brightness.
5. Choose manual operation or configure your location/custom hours for scheduling.
6. Use **Ctrl+Alt+N** to quickly toggle the filter.

Use the application's **Exit** command to fully stop it before replacing its files. A portable package does not require installation, but preferences are still stored in your Windows user profile.

## Settings and existing installations

The application is named **ArchLight**. For compatibility with earlier ArcLight installations, settings remain at `%APPDATA%\ArcLight\ayarlar.cfg`, and the installer retains the legacy installation directory. Existing settings are preserved during an upgrade. Back up the configuration file before moving to another PC.

## About this publication

Version **0.5.2** was the first publication in this recreated repository; **0.5.3** fixes startup and uninstall behavior. The existing version number is retained so installed copies and release packages remain consistent. It includes the refreshed five-theme interface, localized selectors, a single integrated title/header, and the fix for the disposed dropdown-menu error that previously prevented theme or language changes.

## Verification and limitations

Release assets include `SHA256SUMS.txt`. Compare downloaded files with the listed SHA-256 checksums using PowerShell:

```powershell
Get-FileHash .\ArchLight-Installer.win-x64.exe -Algorithm SHA256
```

Package validation checks architecture headers and verifies that each installer contains the same application as its matching portable ZIP. Dropdown tests cover repeated selection and dismissal; integration checks cover all 35 language/theme combinations with isolated settings.

**x86 and ARM64 have not been tested on physical target hardware.** Exclusive fullscreen games, HDR, display-mode changes, and other color-management tools can override Windows gamma settings. Brief changes in screen color may occur when switching applications or display modes. The filter is not a medical treatment.

## Build from source

Run `build-release.ps1` in Windows PowerShell. The script uses Roslyn 4.8.0, downloading it from NuGet if no compiler path is supplied, and builds all three architectures, both source archives, and SHA-256 checksums.

```powershell
.\build-release.ps1 -OutputDirectory .\release
```

To use an existing compiler, add `-Compiler C:\path\to\csc.exe`.

- `Program.cs`: application behavior, scheduling, and display filter logic.
- `ModernUI.cs`: themed controls and interface presentation.
- `Settings.cs`: configuration persistence and recovery.
- `Installer.cs`: Windows installer.
- `LogoArtwork.cs` and `GenerateIcon.cs`: application artwork and icon generation.

## Reporting problems

Open an [issue](https://github.com/archfirein/ArchLight/issues) with your Windows version, architecture, ArchLight version, steps to reproduce, and any error text. For display issues, mention HDR, fullscreen/windowed mode, and other active screen-color tools. Remove personal information from screenshots and logs before posting.
