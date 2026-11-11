# ArchLight 0.5.5 — Stability and Display Recovery

This release improves scheduling, display-filter recovery and shutdown behavior while preserving existing preferences and the seven-language interface.

## Fixes

- Re-evaluate the active schedule after waking from sleep instead of reapplying a stale day/night state.
- Restore the captured display calibration when turning the filter off, and clear the recovery target so the old filter cannot be reapplied.
- Coalesce bursts of display notifications into one owned timer and cancel pending work on shutdown.
- Back off after display-driver exceptions as well as rejected writes. Check stable gamma settings every two seconds instead of every 500 milliseconds.
- Stop the scheduling timer in manual mode and the recovery timer while the filter is disabled.
- Exit through normal form disposal, release timers and system-event subscriptions, remove tray resources and flush preferences once. Restore display settings if startup fails and keep crash logging in the writable user-profile directory.

## Downloads

| Windows architecture | Installer | Portable archive | Runtime requirement |
| --- | --- | --- | --- |
| x64 (Intel/AMD 64-bit) | `ArchLight-Installer.win-x64.exe` | `ArchLight.win-x64.zip` | .NET Framework 4 or later |
| x86 (32-bit) | `ArchLight-Installer.win-x86.exe` | `ArchLight.win-x86.zip` | .NET Framework 4 or later |
| ARM64 (Windows on ARM) | `ArchLight-Installer.win-arm64.exe` | `ArchLight.win-arm64.zip` | Native .NET Framework 4.8.1 on a compatible Windows on ARM system |

ARM32 is not supported. These are managed .NET Framework applications, not self-contained .NET distributions. See [Microsoft's .NET Framework documentation](https://learn.microsoft.com/en-us/dotnet/framework/whats-new/) for native ARM64 support and platform requirements.

Use **Exit** inside ArchLight before replacing an existing installation. Existing preferences remain under `%APPDATA%\ArcLight\ayarlar.cfg`. Portable packages use the same preference location. The installer retains the legacy `%LOCALAPPDATA%\ArcLight` installation directory for compatibility.

## Validation and limits

- **55 regression checks passed** across display recovery, mocked application lifecycle, settings persistence and artwork/font reuse; lifecycle coverage includes all 35 language/theme combinations.
- **x64:** built and package-validated; the application was launched on the local Windows x64 computer and existing settings were preserved.
- **x86:** cross-compiled and package-validated; not executed on physical x86 hardware.
- **ARM64:** cross-compiled and package-validated; not executed on ARM64 hardware.
- The application, installer and uninstaller PE architecture headers are checked for every target. Embedded installer payloads and portable application payloads are compared by SHA-256. Both source archives match the release commit.
- In a simulated 60-second stable-display workload, driver reads fell from **120 to 30**, with no writes in either version. This is a **75% reduction in these reads**, not a measured overall CPU, memory or startup-speed improvement. Detecting an external gamma override can now take up to approximately two seconds.
- In a simulated 10-second driver-exception workload, attempts fell from 20 to 4 and exceptions no longer escaped the recovery routine. Tests mock display writes and use isolated settings files.
- Actual sleep/resume, HDR, exclusive-fullscreen games, multiple-monitor behavior and different GPU drivers have not been validated on target hardware. Driver rejection can still prevent gamma changes or restoration.

`SHA256SUMS.txt`, `TestResults.txt`, `Validation.txt`, `Source.code.zip` and `Source.code.tar.gz` accompany the architecture packages. No personal settings or rollback copies are included in release assets.
