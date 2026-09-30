$ErrorActionPreference = 'Stop'
$gameRoot = Split-Path $PSScriptRoot -Parent
Add-Type -Path (Join-Path $gameRoot 'BepInEx/core/Mono.Cecil.dll')
$game = [Mono.Cecil.AssemblyDefinition]::ReadAssembly((Join-Path $gameRoot 'ULTRAKILL_Data/Managed/Assembly-CSharp.dll'))
$plugin = [Mono.Cecil.AssemblyDefinition]::ReadAssembly((Join-Path $PSScriptRoot 'bin/Release/netstandard2.1/EasyMode.dll'))
$checked = 0
try {
    foreach ($patch in $plugin.MainModule.Types) {
        $attrs = @($patch.CustomAttributes | Where-Object { $_.AttributeType.FullName -eq 'HarmonyLib.HarmonyPatch' })
        if (!$attrs.Count) { continue }
        $targets = @()
        if ($patch.Name -eq 'UnlimitedWeapons') {
            foreach ($name in @('WeaponCharges','Revolver','Shotgun','ShotgunHammer','Nailgun','Railcannon','RocketLauncher')) { $targets += ,@($name, 'Update') }
            $targets += ,@('Nailgun','FixedUpdate')
        } elseif ($patch.Name -eq 'UnlimitedNailgun') {
            $targets = @(@('Nailgun','Update'), @('Nailgun','FixedUpdate'))
        } else {
            foreach ($attr in $attrs) {
                if ($attr.ConstructorArguments.Count -ne 2) { throw "Unhandled patch declaration: $($patch.Name)" }
                $targets += ,@($attr.ConstructorArguments[0].Value.FullName, $attr.ConstructorArguments[1].Value)
            }
        }
        foreach ($target in $targets) {
            $type = $game.MainModule.Types | Where-Object FullName -eq $target[0]
            if (!$type) { throw "Missing game type $($target[0])" }
            $methods = @($type.Methods | Where-Object Name -eq $target[1])
            if ($methods.Count -ne 1) { throw "Missing or ambiguous target $($target -join '.')" }
            foreach ($method in $patch.Methods | Where-Object { $_.Name -in @('Prefix','Postfix') }) {
                foreach ($param in $method.Parameters | Where-Object { $_.Name.StartsWith('___') }) {
                    $fieldName = $param.Name.Substring(3)
                    $field = $type.Fields | Where-Object Name -eq $fieldName
                    if (!$field) { throw "Missing field $($target[0]).$fieldName" }
                    $expected = $param.ParameterType.FullName.TrimEnd('&')
                    if ($field.FieldType.FullName -ne $expected) { throw "Field type mismatch: $fieldName" }
                }
            }
            $checked++
        }
    }
    if ($checked -lt 20) { throw "Too few patch targets checked: $checked" }
    Write-Output "PASS: $checked Harmony targets and all injected private fields match the installed game assembly."
} finally {
    $game.Dispose()
    $plugin.Dispose()
}
