using System;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace EasyMode;

[BepInPlugin("com.realk.ultrakill.easymode", "ultrakill_but_im_a_cs2_premier_cheater", "1.6.0")]
public sealed class Plugin : BaseUnityPlugin
{
    internal static Plugin Instance;
    internal readonly string[] labels = { "Helper enabled", "Auto-parry all attacks", "Invulnerability", "Infinite jumps", "Infinite stamina", "Rapid fire", "Enemy ESP", "No weapon cooldowns / infinite ammo", "Noclip", "Fullbright" };
    internal readonly ConfigEntry<bool>[] options = new ConfigEntry<bool>[10];
    Harmony harmony;
    HelperMenuUi menu;
    internal HelperSettings Settings;
    internal PRankHelper.RankTracker RankHelper;
    readonly AutoParry autoParry = new AutoParry();
    readonly CustomCrosshair crosshair = new CustomCrosshair();
    readonly EnemyEsp esp = new EnemyEsp();
    readonly NoclipController noclip = new NoclipController();
    readonly FullbrightController fullbright = new FullbrightController();

    internal static bool On(int index) => Instance && Instance.options[0].Value && Instance.options[index].Value;
    internal static bool Playing(NewMovement player) => player && player.activated && !player.dead && !player.levelOver && Time.timeScale > 0 && !GameStateManager.Instance.PlayerInputLocked;

    void Awake()
    {
        Instance = this;
        for (int i = 0; i < options.Length; i++)
            options[i] = Config.Bind("Assists", i == 1 ? "Auto-parry projectiles" : labels[i], false, labels[i]); // Preserve existing settings.
        Settings = new HelperSettings(Config);
        RankHelper = PRankHelper.RankRuntime.Get(Logger);
        menu = new HelperMenuUi(this);
        harmony = new Harmony("com.realk.ultrakill.easymode");
        try { harmony.PatchAll(typeof(Plugin).Assembly); }
        catch (Exception ex) { harmony.UnpatchSelf(); enabled = false; Logger.LogError(ex); return; }
        Logger.LogInfo($"ultrakill_but_im_a_cs2_premier_cheater loaded. {Settings.MenuKey.Value} menu, arrows + Enter to toggle, {Settings.MasterKey.Value} master switch.");
    }

    internal static bool Rapid(int weapon) => On(5) && weapon >= 0 && Instance.Settings.Rapid[weapon].Value;
    internal static bool Unlimited(int weapon) => On(7) && weapon >= 0 && Instance.Settings.Unlimited[weapon].Value;

    void Update() { menu?.Update(); RankHelper?.Tick(); }
    void LateUpdate() { noclip.Tick(); fullbright.Tick(); autoParry.Tick(); crosshair.Tick(); }
    void FixedUpdate() => autoParry.Tick();
    void OnGUI()
    {
        crosshair.Draw();
        esp.Draw();
        menu?.Draw();
        RankHelper?.Draw(menu != null && menu.IsOpen);
    }
    void OnDestroy()
    {
        crosshair.Restore();
        fullbright.Restore();
        noclip.Restore();
        menu?.Close();
        harmony?.UnpatchSelf();
        if (Instance == this) { Instance = null; PRankHelper.RankRuntime.Integrated = false; }
    }
    void OnEnable() { PRankHelper.RankRuntime.Integrated = true; }
    void OnDisable() { PRankHelper.RankRuntime.Integrated = false; crosshair.Restore(); fullbright.Restore(); noclip.Restore(); menu?.Close(); }
}

[HarmonyPatch(typeof(NewMovement), "GetHurt")]
static class DamagePatch
{
    static bool Prefix() => !Plugin.On(2);
}

[HarmonyPatch(typeof(NewMovement), "HandleInputs")]
static class MovementPatch
{
    static void Prefix(NewMovement __instance, bool ___jumpCooldown)
    {
        if (!Plugin.Playing(__instance)) return;
        if (Plugin.On(4)) __instance.boostCharge = 300f;
        if (Plugin.On(3) && __instance.falling && !___jumpCooldown && MonoSingleton<InputManager>.Instance.InputSource.Jump.WasPerformedThisFrame)
            __instance.Jump();
    }
    static void Postfix(NewMovement __instance)
    {
        if (Plugin.On(4) && Plugin.Playing(__instance)) __instance.boostCharge = 300f;
    }
}

[HarmonyPatch(typeof(Revolver), "Shoot")]
static class RevolverPatch
{
    static void Postfix(Revolver __instance, ref float ___shootCharge) { if (Plugin.Rapid(HelperSettings.Weapon(__instance))) ___shootCharge = 80f; }
}

[HarmonyPatch(typeof(Shotgun), "Shoot")]
static class ShotgunPatch
{
    static void Postfix(Shotgun __instance) { if (Plugin.Rapid(2)) __instance.Invoke("ReadyGun", 0.12f); }
}

[HarmonyPatch(typeof(Nailgun), "Shoot")]
static class NailgunPatch
{
    static void Postfix(Nailgun __instance, ref float ___fireCooldown) { if (Plugin.Rapid(HelperSettings.Weapon(__instance))) ___fireCooldown *= 0.2f; }
}

[HarmonyPatch(typeof(RocketLauncher), "Shoot")]
static class RocketPatch
{
    static void Postfix(ref float ___cooldown) { if (Plugin.Rapid(7)) ___cooldown *= 0.2f; }
}

[HarmonyPatch(typeof(Railcannon), "Update")]
static class RailPatch
{
    static void Prefix()
    {
        if (Plugin.Rapid(6) && Plugin.Playing(MonoSingleton<NewMovement>.Instance))
            MonoSingleton<WeaponCharges>.Instance.raicharge = 5f;
    }
}



[HarmonyPatch(typeof(ShotgunHammer), "Update")]
static class RapidHammer
{
    static void Prefix(ref float ___hammerCooldown)
    {
        if (Plugin.Rapid(3) && Plugin.Playing(MonoSingleton<NewMovement>.Instance))
            ___hammerCooldown = UnityEngine.Mathf.Max(0, ___hammerCooldown - UnityEngine.Time.deltaTime * 4f);
    }
}

