using BepInEx.Configuration;
using UnityEngine;

namespace EasyMode;

internal sealed class HelperSettings
{
    internal static readonly string[] Weapons = { "Revolver", "Slab revolver", "Shotgun", "Jackhammer", "Nailgun", "Sawblade launcher", "Railcannon", "Rocket launcher" };
    internal readonly ConfigEntry<bool>[] Rapid = new ConfigEntry<bool>[8], Unlimited = new ConfigEntry<bool>[8];
    internal readonly ConfigEntry<KeyCode> MenuKey, MasterKey;
    internal readonly ConfigEntry<bool> Names, Health, Conditions, Boxes, HideOverlap, Sanded, Enraged, Blessed, Radiant, Puppet, ActiveTools, ParryMelee, ParryObjects;
    internal readonly ConfigEntry<PRankHelper.OutlineStyle> Outline;
    internal readonly ConfigEntry<Color> EspColor;
    internal readonly ConfigEntry<float> OutlineThickness, InfoOpacity;
    internal readonly ConfigEntry<bool> CustomCrosshair, CrossDot, CrossLines, CrossCircle, CrossOutline, HideDefaultCrosshair;
    internal readonly ConfigEntry<float> CrossSize, CrossThickness, CrossGap, CrossOpacity;
    internal readonly ConfigEntry<Color> CrossColor;
    internal HelperSettings(ConfigFile config)
    {
        CustomCrosshair = config.Bind("Crosshair", "Enabled", false);
        CrossDot = config.Bind("Crosshair", "Dot", false);
        CrossLines = config.Bind("Crosshair", "Cross", true);
        CrossCircle = config.Bind("Crosshair", "Circle", false);
        CrossOutline = config.Bind("Crosshair", "Outline", true);
        HideDefaultCrosshair = config.Bind("Crosshair", "Hide default crosshair", true);
        CrossSize = config.Bind("Crosshair", "Size", 8f, new ConfigDescription("", new AcceptableValueRange<float>(2, 40)));
        CrossThickness = config.Bind("Crosshair", "Thickness", 2f, new ConfigDescription("", new AcceptableValueRange<float>(1, 8)));
        CrossGap = config.Bind("Crosshair", "Gap", 4f, new ConfigDescription("", new AcceptableValueRange<float>(0, 24)));
        CrossOpacity = config.Bind("Crosshair", "Opacity", 1f, new ConfigDescription("", new AcceptableValueRange<float>(0, 1)));
        CrossColor = config.Bind("Crosshair", "Color", new Color(0.2f, 1f, 0.4f, 1));
        MenuKey = config.Bind("Controls", "Menu", KeyCode.F1, "Open or close the helper menu.");
        MasterKey = config.Bind("Controls", "Master", KeyCode.F2, "Enable or disable all selected assists.");
        for (int i = 0; i < Weapons.Length; i++)
        {
            Rapid[i] = config.Bind("Rapid fire", Weapons[i], true, "Apply rapid fire to this weapon family and its color variants.");
            Unlimited[i] = config.Bind("Unlimited weapons", Weapons[i], true, "Remove cooldowns and refill this weapon family's charges.");
        }
        Outline = config.Bind("ESP appearance", "Outline style", PRankHelper.OutlineStyle.Box);
        EspColor = config.Bind("ESP appearance", "Outline color", new Color(1f, 0.2f, 0.25f, 0.85f));
        OutlineThickness = config.Bind("ESP appearance", "Thickness", 1f, new ConfigDescription("Outline thickness in pixels.", new AcceptableValueRange<float>(1f, 6f)));
        InfoOpacity = config.Bind("ESP appearance", "Info background opacity", 0.65f, new ConfigDescription("Label background opacity.", new AcceptableValueRange<float>(0f, 1f)));
        Names = config.Bind("ESP", "Names", true);
        Health = config.Bind("ESP", "Health", false);
        Conditions = config.Bind("ESP", "Conditions", false);
        Boxes = config.Bind("ESP", "Boxes", true);
        HideOverlap = config.Bind("ESP", "Hide overlapping labels", true);
        Sanded = config.Bind("ESP conditions", "Sanded", true);
        Enraged = config.Bind("ESP conditions", "Enraged", true);
        Blessed = config.Bind("ESP conditions", "Blessed", true);
        Radiant = config.Bind("ESP conditions", "Radiant", true);
        Puppet = config.Bind("ESP conditions", "Puppet", true);
        ActiveTools = config.Bind("Interface", "Active tools HUD", false);
        ParryMelee = config.Bind("Auto parry", "Enemy melee windows", true);
        ParryObjects = config.Bind("Auto parry", "Projectiles and objects", true);
    }

    internal static int Weapon(Component component)
    {
        if (component is Revolver revolver) return revolver.altVersion ? 1 : 0;
        if (component is Shotgun) return 2;
        if (component is ShotgunHammer) return 3;
        if (component is Nailgun nailgun) return nailgun.altVersion ? 5 : 4;
        if (component is Railcannon) return 6;
        if (component is RocketLauncher) return 7;
        return -1;
    }

    internal static int EquippedWeapon()
    {
        var guns = MonoSingleton<GunControl>.Instance;
        if (!guns || !guns.currentWeapon) return -1;
        foreach (var component in guns.currentWeapon.GetComponents<MonoBehaviour>())
        {
            int index = Weapon(component);
            if (index >= 0) return index;
        }
        return -1;
    }
}
