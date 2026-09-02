<p align="center">
  <img src="assets/DeskBound-logo.png" width="104" alt="DeskBound logo">
</p>

<h1 align="center">DeskBound</h1>

<p align="center">
  Keep your desktop organized without changing the way you work.
</p>

<p align="center">
  <strong>English</strong> · <a href="README.zh-TW.md">繁體中文</a> ·
  <a href="https://github.com/bestdrduck/DeskBound/releases/latest">Download</a>
</p>

DeskBound is a lightweight desktop organizer for Windows 10 and Windows 11. It groups files, folders, and shortcuts into movable, tabbed desktop fences while keeping the underlying files real and accessible. Its transparent WPF windows integrate with the Windows desktop layer and work alongside animated wallpapers such as Wallpaper Engine.

## Download

Download the latest portable build from [GitHub Releases](https://github.com/bestdrduck/DeskBound/releases/latest), then run `DeskBound.exe`. First launch starts with an empty layout and never moves existing desktop items automatically.

## Highlights

- Movable, resizable, and collapsible desktop fences
- Multiple tabs in each fence with animated switching
- Drag desktop items into a fence and move them back out at any time
- Folder-backed fences and in-fence folder navigation
- Search, sorting, icon scaling, multi-selection, and keyboard controls
- Desktop Inbox for collecting newly created desktop items
- Smart organization rules, layout snapshots, scenes, and persistent undo history
- Four visual styles with adjustable opacity, accent colors, shadows, and title visibility
- Wallpaper Engine optimization without a full-screen overlay
- Background update checks and one-click, SHA-256-verified self-updates

## Designed for real desktop use

DeskBound keeps every item as a real Windows file, folder, or shortcut. Removing a fence or tab removes only its layout entry—it does not delete its contents. Name collisions never overwrite existing files, layout data keeps a backup, and failed application updates restore the previous executable.

Each fence is an independent desktop window instead of part of a full-screen transparent layer. This keeps desktop interaction responsive and reduces interference with animated wallpaper software.

## Version 0.13.0

- Checks GitHub Releases in the background, at most once every six hours
- Offers update installation from a notification, the system tray, or Control Center
- Verifies the downloaded executable using its GitHub SHA-256 digest
- Replaces and restarts the application automatically after confirmation
- Keeps the previous executable available for rollback if installation fails

Users of version 0.12.0 need to download version 0.13.0 once. Versions starting with 0.13.0 include the updater for future releases.

## Quick controls

- Drag the title bar to move a fence
- Drag the bottom-right corner to resize it
- Double-click the title bar to collapse or expand
- Press `Ctrl + F` to search the current fence
- Press `Ctrl + A` to select all visible items
- Press `Ctrl + Z` to undo the latest move
- Press `Ctrl + Alt + Space` to show or hide all fences
- Press `Ctrl + Alt + P` to toggle quick peek

## Build from source

Run the following command in Windows PowerShell:

```powershell
.\build.ps1
```

The local build is written to `outputs\桌伴.exe`. Existing settings remain under `%LOCALAPPDATA%\DeskBound` for backward compatibility.
