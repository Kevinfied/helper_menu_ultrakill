using System;

namespace PRankHelper;

public static class RankRules
{
    public static bool TryThreshold(int[] ranks, bool time, out int threshold)
    {
        threshold = 0;
        if (ranks == null || ranks.Length != 4) return false;
        threshold = time ? int.MaxValue : 0;
        foreach (int rank in ranks)
        {
            if (rank < 0) return false;
            threshold = time ? Math.Min(threshold, rank) : Math.Max(threshold, rank);
        }
        return !time || threshold > 0;
    }

    public static string Status(float seconds, int kills, int style, int timeTarget, int killTarget,
        int styleTarget, int restarts, bool major, bool cheats, bool complete)
    {
        if (cheats) return "BLOCKED / BUILT-IN CHEATS";
        if (major) return "BLOCKED / MAJOR ASSISTS";
        if (restarts > 0) return "BLOCKED / RESTART USED";
        if (seconds > timeTarget) return "BLOCKED / TIME LIMIT";
        if (kills < killTarget || style < styleTarget) return complete ? "REQUIREMENTS NOT MET" : "IN PROGRESS";
        return complete ? "P-RANK REQUIREMENTS MET" : "ON TARGET / FINISH THE LEVEL";
    }

    public static string Time(float seconds)
    {
        int tenths = Math.Max(0, (int)Math.Floor(seconds * 10));
        return (tenths / 600) + ":" + (tenths / 10 % 60).ToString("00") + "." + (tenths % 10);
    }
}
