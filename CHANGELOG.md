# Changelog

## 0.5.3 — 2026-10-23

- Fixed installer startup selection: disabling removes both current and legacy startup entries, and the selected value is saved to application preferences.
- Added a dedicated uninstaller that requests a normal application exit, removes only owned files, preserves preferences, resolves the actual Desktop folder and removes startup entries.
- Corrected the installer version label and made uninstall-registration failures visible.

## 0.5.2 — 2026-10-22

- Fixed ObjectDisposedException during language and theme selection by retaining each dropdown menu until its owning control is disposed.
- Verified repeated selection/dismissal and all 35 language/theme combinations with isolated settings.



## 0.5.1 — 2026-10-22

- Removed the duplicate native title bar and added integrated minimize/close buttons and header dragging.
- Replaced native theme/language fields with rounded selectors without white borders.
- Localized theme names and selector labels in all seven supported languages.


## 0.5.0 — 2026-10-22

- Redesigned the interface with rounded cards, a two-column layout and custom temperature/brightness sliders.
- Added five persistent themes: Yellow, White, Red, Blue and Black. White uses a light palette; the other themes use dark surfaces.
- Moved theme and language selectors into the header.
- Preserved existing settings, scheduling, hotkey and gamma-recovery behavior.
- Updated release documentation in English.

## 0.4.1 — 2026-10-17

- Enlarged the icon artwork by approximately 26% while preserving its proportions and transparent corners.
- Shortened the Turkish Exit label to prevent clipping.

## 0.4.0 — 2026-10-17

- Renamed ArcLight to ArchLight and introduced rounded icon backgrounds.
- Retained legacy configuration and single-instance identifiers for upgrade compatibility.

## 0.3.4 — 2026-09-28

- Updated the logo and corrected icon letterbox colors.

## Earlier releases

- Added atomic settings persistence and backup recovery.
- Separated status and shortcut text to prevent clipping.
- Added measured gamma restoration after application/display transitions.
