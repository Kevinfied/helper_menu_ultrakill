namespace EasyMode;

internal static class ParryRules
{
    internal static bool EnemyWindow(bool dead, bool crashingDrone, bool fullWindow,
        bool partialWindow, bool correctPart, bool specialWindow) =>
        crashingDrone || (!dead && (fullWindow || (partialWindow && correctPart) || specialWindow));
}
