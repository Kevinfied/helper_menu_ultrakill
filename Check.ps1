$ErrorActionPreference = 'Stop'
$gameRoot = Split-Path $PSScriptRoot -Parent
$built = Join-Path $PSScriptRoot 'bin/Release/netstandard2.1/EasyMode.dll'
$installed = Join-Path $gameRoot 'BepInEx/plugins/HelperMenu/EasyMode.dll'
if ((Get-FileHash $built).Hash -ne (Get-FileHash $installed).Hash) { throw 'Installed DLL differs from build.' }
$rankBuilt = Join-Path $PSScriptRoot 'bin/Release/netstandard2.1/PRankHelper.dll'
$rankInstalled = Join-Path $gameRoot 'BepInEx/plugins/HelperMenu/PRankHelper.dll'
if ((Get-FileHash $rankBuilt).Hash -ne (Get-FileHash $rankInstalled).Hash) { throw 'Installed P-Rank Helper differs from build.' }
$log = Get-Content (Join-Path $gameRoot 'BepInEx/LogOutput.log') -Raw
if ($log -notmatch 'Loading \[ultrakill_but_im_a_cs2_premier_cheater 1\.6\.0\]') { throw 'Restart the game to load v1.6.0 before running this startup check.' }
if ($log -notmatch 'ultrakill_but_im_a_cs2_premier_cheater loaded\.') { throw 'Plugin did not report successful patch installation.' }
if ($log -match '\[Error\s*:\s*ultrakill_but_im_a_cs2_premier_cheater\]') { throw 'ultrakill_but_im_a_cs2_premier_cheater reported an error; inspect LogOutput.log.' }
Write-Output 'PASS: installed DLL matches build; game successfully loaded ultrakill_but_im_a_cs2_premier_cheater and applied its patches.'
