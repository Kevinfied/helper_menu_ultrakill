using BepInEx.Configuration;
using UnityEngine;

namespace EasyMode;

internal sealed class HelperSettings
{
    internal static readonly string[] Weapons = { "Revolver", "Slab revolver", "Shotgun", "Jackhammer", "Nailgun", "Sawblade launcher", "Railcannon", "Rocket launcher" };
    internal readonly ConfigEntry<bool>[] Rapid = new ConfigEntry<bool>[8], Unlimited = new ConfigEntry<bool>[8];
    internal readonly ConfigEntry<KeyCode> MenuKey, MasterKey;
    internal readonly ConfigEntry<bool> Names, Health, Conditions, Boxes, HideOverlap, Sanded, Enraged, Blessed, Radiant, Puppet, ActiveTools, ParryMelee, ParryObjects;
    internal HelperSettings(ConfigFile config)
    {
        MenuKey = config.Bind("Controls", "Menu", KeyCode.F1, "Open or close the helper menu.");
        MasterKey = config.Bind("Controls", "Master", KeyCode.F2, "Enable or disable all selected assists.");
        for (int i = 0; i < Weapons.Length; i++)
        {
            Rapid[i] = config.Bind("Rapid fire", Weapons[i], true, "Apply rapid fire to this weapon family and its color variants.");
            Unlimited[i] = config.Bind("Unlimited weapons", Weapons[i], true, "Remove cooldowns and refill this weapon family's charges.");
        }
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
