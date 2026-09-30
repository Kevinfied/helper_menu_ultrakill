# ultrakill_but_im_a_cs2_premier_cheater

Installed in `BepInEx/plugins/HelperMenu/EasyMode.dll` for this copy of ULTRAKILL.

- F1: open/close the tabbed menu (closed on startup). Opening it unlocks the cursor and locks player/camera input; the world continues running. Close with F1 or X. Drag the header to move the window.
- Left/Right: switch tabs. Up/Down and Enter: select and toggle an option. Mouse buttons and the scroll wheel work in the menu.
- F2: master enable/disable. Enable individual assists as well as the master switch.
- Settings persist in `BepInEx/config/com.realk.ultrakill.easymode.cfg`.

## Menu and sub-options

Combat, Movement, Weapons, Visuals, and Interface tabs organize the settings. Existing main toggle values are preserved.

- Weapons combines Rapid fire and Unlimited, each with its own multi-select weapon dropdown: revolver, slab revolver, shotgun, jackhammer, nailgun, sawblade launcher, railcannon, and rocket launcher. Each family includes its color variants. Open the dropdown, use Clear all, then select Sawblade launcher for saw-only rapid fire. The dropdown uses a two-column card grid with checkmarks, hover highlights, and a selected-count header. Click Done to collapse it. Space opens the keyboard-selected dropdown; Up/Down moves between rows, Left/Right between columns, Enter toggles a weapon, and Esc closes it. Existing per-weapon choices are preserved. Unlimited also removes firing delays: disable that feature for other weapons if you want their normal cadence.
- Auto-parry has separate enemy-attack and projectile/object switches.
- Visuals > Fullbright brightens dark areas with flat ambient illumination and a shadow-free directional light, and disables fog. Original lighting is restored when switched off, the master switch is disabled, or the plugin unloads. New scenes capture their own lighting baseline. No built-in cheat flags are enabled.
- Visuals > ESP offers independent outlines, names, HP, conditions, and overlap suppression. Condition filters include Sanded, Enraged, Blessed, Radiant (health/speed/damage buffs), and Puppet. Only currently active conditions are displayed. Compact labels remain off for HP and conditions until you enable them.
- Interface > Active tools display enables a small top-right readout, at most three short rows. It stays visible with the menu closed, moves left if it would overlap the actual style panel, and hides if there is no free space. F2 disables both the assists and this readout. This setting defaults off.

The new UI scales down on small screens. Weapon sub-options default on to preserve existing behavior; the corresponding main feature and master switch must also be enabled.

Features:

- Auto-parry all attacks: checks the game's native enemy parry windows (including melee, boss-specific partial hitboxes, Malicious Faces, and crashing drones) and parry-object handlers (hostile projectiles, cannonballs, enabled ParryReceivers, thrown swords, spears, mines, chainsaws, and ground waves). Requires the active blue Feedbacker arm, a target within six units in front of the camera, and clear line of sight. Unparryable attacks stay unparryable. Friendly projectile boosting and Gutterman shield breaking are not automatic. The old config key `Auto-parry projectiles` is retained to preserve your toggle.
- Invulnerability: blocks the player's GetHurt damage handler, including damage that normally ignores invincibility. Scripted deaths or out-of-bounds resets that bypass that handler can still occur.
- Infinite jumps: press your bound jump key again in the air; preserves the game's 0.2-second jump debounce and explicit level restrictions.
- Infinite stamina: refills dash stamina while playing.
- Noclip (Movement): fly through geometry using your movement bindings. Move follows camera direction, jump rises, slide descends, and dodge boosts speed (24 units/second normally, 60 boosted). Disables normal movement, rigidbody collision, and boundary blockers while active, then restores their previous states when disabled, on player replacement, or when the plugin unloads. Pauses movement while menus/input locks are active. Turn it off in open space. Does not activate the built-in cheat system.
- Rapid fire: shortens primary-fire cooldowns for revolvers, ordinary shotguns, nail/saw guns, and rocket launchers. Railcannon is charged for each click (still click to fire).
- Enemy ESP: thin red boxes fitted to the projected damage-collider bounds, visible through walls. Box colliders preserve their orientation; other collider shapes use conservative world bounds. Compact 10-point name-only labels replace the large health/distance panels, and overlapping names are suppressed. Shows living, active enemies in the camera's view; works with the menu hidden. Does not change enemy materials or lighting.
- No weapon cooldowns / infinite ammo: replenishes coins, revolver alt charges, nail/saw ammo, heatsinks, magnets, jumpstart recharge, rail charge, shotgun cores/chainsaws, jackhammer recovery, rocket freeze time, cannonballs, and napalm fuel. Removes primary-fire cooldowns and shotgun/revolver alt-fire recovery. Normal charging/hold-and-release inputs and projectile lifetimes remain; this does not make every alt-fire automatic. With this enabled, primary fire can be faster than the separate Rapid fire setting. Both new toggles default off and require Helper enabled.

Does not enable or modify the game's built-in cheat/major-assist flags. This is still a gameplay-altering mod; rank/leaderboard behavior has not been verified. The existing bot is independent. In this local install its toggle/pause bindings have been moved to F6/F7 to avoid conflicts; leave it disabled when playing manually. Helper keys can be changed under [Controls] in its BepInEx config.

Build from the game directory:

```powershell
$env:DOTNET_CLI_HOME=(Resolve-Path '.tools').Path
.tools/dotnet/dotnet.exe build HelperMenu/EasyMode.csproj -c Release
.tools/dotnet/dotnet.exe run --project HelperMenu/tests/ParryChecks.csproj -c Release
./HelperMenu/CheckPatches.ps1
```

Restart the game after rebuilding. To uninstall, close the game and remove only `BepInEx/plugins/HelperMenu/EasyMode.dll`.

Validation: v1.5.0 Release build passes without warnings; 23 Harmony targets and their injected private-field types match this installed game assembly. Eight parry-window checks also pass. The earlier v1.0.0 was verified loading in-game; v1.5.0 UI rendering, weapon-selection behavior, and ESP condition labels require in-game verification. Restart the running game to load this update. Gameplay checks below remain manual.

1. Enable master and invulnerability; take a hit, then disable invulnerability and confirm damage returns.
2. Enable jumps and stamina; jump repeatedly in midair and dash repeatedly, then disable them and confirm normal limits return.
3. With blue arm active, face a melee windup, boss partial hitbox, Malicious Face charge, crashing drone, projectile, cannonball, sword, spear, mine, and enabled parry receiver. Confirm the native parry reaction/reward happens once per attack. Check that idle enemies, incorrect boss hitboxes, walls, disabled receiver windows, and unparryable attacks do not trigger. Test paused, disabled, and non-blue-arm states as well.
4. Compare primary-fire cadence with rapid fire on/off for each supported weapon; click the railcannon repeatedly.
5. Toggle F2 off, pause, die/restart, and change levels; confirm no assist actions run while disabled or input is locked.
6. Enable ESP, hide the menu, and move behind a wall: living enemies in view should retain their labels. Kill an enemy and toggle F2 off to check marker removal.
7. Enable unlimited weapons. Fire more than ten saws without waiting, toss more than four coins, repeatedly eject shotgun cores and chainsaws, and test heatsinks, magnets, jumpstart, railcannon clicks, jackhammer, cannonballs, and napalm. Disable the toggle and confirm the next shots consume resources normally.
8. Clear all Rapid fire weapons, select only Sawblade launcher, and disable Unlimited: saws should fire faster while nailgun/revolver/shotgun cadence stays normal. Repeat with Unlimited's independent selections.
9. Toggle ESP name, HP, and condition fields independently, including labels-only and boxes-only. Check Sanded and Enraged enemies against their actual state. Toggle each condition filter.
10. Enable Interface > Active tools display, close the menu, and build style rank. Confirm the small display does not cover the style board. Test dragging, scrolling, tab switching, mouse toggles, keyboard toggles, and restoring player input after closing.

Run `./HelperMenu/Check.ps1` after building and launching to check the installed artifact and startup log.



11. Enable Movement > Noclip, fly through a wall, test rise/descend/boost and menu lock, then turn it off in open space. Confirm gravity, collision, and normal movement return. Check F2, death/restart, and level changes as well.
12. Open each Weapons dropdown, choose different weapon sets, close/reopen the menu and restart the game. Confirm the selections persist independently and only selected weapons receive their feature.


13. Toggle Visuals > Fullbright in a dark room and confirm it brightens, then disable it or F2 and confirm lighting/fog return. Change levels while enabled and repeat. Shader-specific results still require in-game verification.


