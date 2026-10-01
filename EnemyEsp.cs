namespace EasyMode;

internal sealed class EnemyEsp
{
    readonly PRankHelper.EnemyEsp renderer = new PRankHelper.EnemyEsp();
    internal void Draw()
    {
        if (!Plugin.On(6)) return;
        var s = Plugin.Instance.Settings;
        renderer.Draw(s.Boxes.Value, s.Names.Value, s.Health.Value, s.Conditions.Value,
            s.Sanded.Value, s.Blessed.Value, s.Puppet.Value, s.Radiant.Value, s.Enraged.Value, s.HideOverlap.Value,
            s.Outline.Value, s.EspColor.Value, s.OutlineThickness.Value, s.InfoOpacity.Value);
    }
}
