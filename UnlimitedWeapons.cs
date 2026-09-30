using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace EasyMode;

[HarmonyPatch]
internal static class UnlimitedWeapons
{
    internal static bool Active => Plugin.On(7) && Plugin.Playing(MonoSingleton<NewMovement>.Instance);

    static IEnumerable<MethodBase> TargetMethods()
    {
        foreach (var type in new[] { typeof(WeaponCharges), typeof(Revolver), typeof(Shotgun),
            typeof(ShotgunHammer), typeof(Nailgun), typeof(Railcannon), typeof(RocketLauncher) })
            yield return AccessTools.Method(type, "Update");
        yield return AccessTools.Method(typeof(Nailgun), "FixedUpdate");
    }

    // Refill before each consumer reads its ammo, regardless of Unity's component update order.
    [HarmonyPriority(Priority.First)]
    static void Prefix(object __instance)
    {
        if (!Active) return;
        var wc = MonoSingleton<WeaponCharges>.Instance;
        if (!wc) return;
        int weapon = __instance is WeaponCharges ? HelperSettings.EquippedWeapon() : HelperSettings.Weapon(__instance as Component);
        if (!Plugin.Unlimited(weapon)) return;
        if (weapon == 0 || weapon == 1)
        {
            wc.rev0charge = 100f;
            wc.rev1charge = 400f;
            wc.rev2charge = 300f;
            Array.Clear(wc.revaltpickupcharges, 0, wc.revaltpickupcharges.Length);
        }
        if (weapon == 2 || weapon == 3)
        {
            wc.shoAltNadeCharge = wc.shoSawCharge = 1f;
            wc.shoAltYellows = 0;
            wc.shoAltYellowsTimer = wc.shoSawResetTimer = 0f;
            Array.Clear(wc.shoaltcooldowns, 0, wc.shoaltcooldowns.Length);
        }
        if (weapon == 4 || weapon == 5)
        {
            if (weapon == 4) { wc.naiHeatsinks = 2f; wc.naiAmmo = 100f; }
            else { wc.naiSawHeatsinks = 1f; wc.naiSaws = 10f; }
            wc.naiMagnetCharge = 3f;
            wc.naiZapperRecharge = 5f;
        }
        if (weapon == 6) wc.raicharge = 5f;
        if (weapon == 7)
        {
            wc.rocketFreezeTime = 5f;
            wc.rocketcharge = 0f;
            wc.rocketCannonballCharge = wc.rocketNapalmFuel = 1f;
        }
        // Keep magnet/projectile tracking intact; only replenish consumable resources.
    }
}

[HarmonyPatch(typeof(Revolver), "Update")]
static class UnlimitedRevolver
{
    static void Prefix(Revolver __instance, ref bool ___shootReady, ref bool ___gunReady,
        ref float ___shootCharge, ref float ___pierceCharge)
    {
        if (!UnlimitedWeapons.Active || !Plugin.Unlimited(HelperSettings.Weapon(__instance))) return;
        __instance.MaxCharge();
        ___shootReady = ___gunReady = true;
        ___shootCharge = ___pierceCharge = 100f;
        // Let native Update finish pierceReady and its UI transition with a full charge.
    }
}

[HarmonyPatch(typeof(Shotgun), "Update")]
static class UnlimitedShotgun
{
    static void Prefix(ref bool ___gunReady, ref bool ___resettingCores, ref TimeSince ___sinceLastCore)
    {
        if (!UnlimitedWeapons.Active || !Plugin.Unlimited(2)) return;
        ___gunReady = true;
        ___resettingCores = false;
        ___sinceLastCore = 1f;
    }
}

[HarmonyPatch(typeof(ShotgunHammer), "Update")]
static class UnlimitedHammer
{
    static void Prefix(ref bool ___gunReady, ref float ___hammerCooldown)
    {
        if (!UnlimitedWeapons.Active || !Plugin.Unlimited(3)) return;
        ___gunReady = true;
        ___hammerCooldown = 0f;
    }
}

[HarmonyPatch]
static class UnlimitedNailgun
{
    static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(Nailgun), "Update");
        yield return AccessTools.Method(typeof(Nailgun), "FixedUpdate");
    }
    static void Prefix(Nailgun __instance, ref float ___heatSinks, ref float ___heatUp,
        ref bool ___burnOut, ref float ___fireCooldown)
    {
        if (!UnlimitedWeapons.Active || !Plugin.Unlimited(HelperSettings.Weapon(__instance))) return;
        ___heatSinks = __instance.altVersion ? 1f : 2f;
        // Preserve a usable heat level for repeated overheat alt-fire.
        ___heatUp = Mathf.Max(___heatUp, 0.1f);
        if (MonoSingleton<InputManager>.Instance.InputSource.Fire2.WasPerformedThisFrame) ___burnOut = false;
        ___fireCooldown = 0f;
    }
}

[HarmonyPatch(typeof(Railcannon), "Update")]
static class UnlimitedRail
{
    static void Prefix(ref float ___altCharge)
    {
        if (UnlimitedWeapons.Active && Plugin.Unlimited(6)) ___altCharge = 5f;
    }
}

[HarmonyPatch(typeof(RocketLauncher), "Update")]
static class UnlimitedRocket
{
    static void Prefix(ref float ___cooldown)
    {
        if (UnlimitedWeapons.Active && Plugin.Unlimited(7)) ___cooldown = 0f;
    }
}

