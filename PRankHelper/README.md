# P-Rank Helper

An independent BepInEx mod with its own plugin, settings menu, progress HUD, live enemy counting, and Extreme Assist ESP. HelperMenu also includes these features using the same shared code. Neither mod requires the other.

Standalone: open **F5**. With HelperMenu installed: **F5** opens its P-Rank tab (press again to close), or use **F1 > P-Rank**; the standalone UI yields to HelperMenu, preventing duplicate overlays and enemy counters. Both use `BepInEx/config/com.realk.ultrakill.prankhelper.settings.cfg`. The standalone menu key is stored in `com.realk.ultrakill.prankhelper.cfg`. Existing P-rank options previously saved inside the HelperMenu config must be set again once. The progress HUD defaults on and remains visible in a level with the menu closed, while paused/dead, and with the master assist switch off. It displays the installed level's actual S-rank time, kill and style thresholds, current progress, zero-restart requirement, and built-in cheat/major-assist blockers. Rankless levels, unsupported threshold arrays, casual modes and Cyber Grind show unavailable instead of invented targets. The HUD stays at its saved position and can be dragged while the settings menu is open.

The status is a live projection, not an award: finish the level for the game to award the rank. Taking damage alone does not disqualify a P rank.

## Enemies alive

The HUD and settings panel show the current count of active, living enemies tracked by the game, including enemies behind walls and off screen. It updates as enemies spawn, die, or become inactive. Future spawns and inactive encounters are excluded: zero does not guarantee that every kill in the level is complete.

No checkpoint recording, route selection, successful practice run, or manual target entry is needed. Old route files are left untouched but are no longer read or written.

## Extreme Assist

Available in the standalone mod; requires the progress HUD. Shows enemy hitbox outlines and names through walls. HelperMenu uses its full ESP controls under Visuals instead of Extreme Assist. It does not automate combat or modify rank statistics.

## Build and checks

Build the standalone mod from the game directory:

```powershell
.tools/dotnet/dotnet.exe build HelperMenu/PRankHelper/Standalone/PRankHelperStandalone.csproj -c Release
```

This installs **PRankHelperStandalone.dll + PRankHelper.dll** in `BepInEx/plugins/PRankHelper/`. Distribute that folder as the standalone mod; it requires BepInEx and ULTRAKILL, but no HelperMenu files.

Building `HelperMenu/EasyMode.csproj -c Release` installs **EasyMode.dll + PRankHelper.dll** in `BepInEx/plugins/HelperMenu/`. Either folder works alone. Installing both is supported; keep both copies of the shared PRankHelper.dll on the same version. The parent `PRankHelper.csproj` is the shared implementation, and `Standalone/PRankHelperStandalone.csproj` is the independent mod entry point.

```powershell
.tools/dotnet/dotnet.exe run --project HelperMenu/PRankHelper/tests/RankChecks.csproj -c Release
```

The executable checks cover exact S thresholds, inclusive time boundaries, missing/invalid rank tables, kills/style deficits, restarts, major assists, cheats, and time formatting. In-game validation still needs: live HUD layout, enemy count after spawns, deaths, restarts and level changes, unsupported modes, and outline/name-only Extreme Assist.

## HUD customization

Open the menu to adjust background opacity, RGB accent color, and HUD size (60-200%). Drag the HUD while the menu is open; release to save its position. Toggle title, level name, restarts, major assists, rank status, and enemies alive independently. Hidden rows collapse. Both mods share these preferences.
