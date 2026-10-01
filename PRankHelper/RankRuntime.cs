using System.IO;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;

namespace PRankHelper;

public static class RankRuntime
{
    public static bool Integrated;
    public static bool ToggleIntegratedMenu;
    static RankTracker tracker;
    public static RankTracker Get(ManualLogSource log)
    {
        if (tracker == null)
            tracker = new RankTracker(new ConfigFile(Path.Combine(Paths.ConfigPath, "com.realk.ultrakill.prankhelper.settings.cfg"), true));
        return tracker;
    }
}
