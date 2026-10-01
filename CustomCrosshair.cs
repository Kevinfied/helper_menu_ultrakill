using UnityEngine;
using UnityEngine.UI;

namespace EasyMode;

internal sealed class CustomCrosshair
{
    Crosshair hidden;
    bool playing;

    internal void Tick()
    {
        var settings = Plugin.Instance.Settings;
        playing = settings.CustomCrosshair.Value && Plugin.Playing(MonoSingleton<NewMovement>.Instance);
        var stats = MonoSingleton<StatsManager>.Instance;
        var current = stats && stats.crosshair ? stats.crosshair.GetComponent<Crosshair>() : null;
        if (!playing || !settings.HideDefaultCrosshair.Value || hidden != current) Restore();
        if (!playing || !settings.HideDefaultCrosshair.Value || !current) return;
        hidden = current;
        var main = hidden.GetComponent<Image>();
        if (main) main.enabled = false;
        foreach (var image in hidden.altchs) if (image) image.enabled = false;
    }

    internal void Restore()
    {
        if (hidden) hidden.CheckCrossHair();
        hidden = null;
    }

    internal void Draw()
    {
        if (playing && Event.current.type == EventType.Repaint)
            Render(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f), Plugin.Instance.Settings);
    }

    internal static void Render(Vector2 center, HelperSettings s)
    {
        var old = GUI.color;
        float t = s.CrossThickness.Value, size = s.CrossSize.Value, gap = s.CrossGap.Value;
        var color = s.CrossColor.Value;
        color.a = s.CrossOpacity.Value;
        if (s.CrossCircle.Value)
        {
            if (s.CrossOutline.Value) Ring(center, size + gap, t + 2, new Color(0, 0, 0, color.a));
            Ring(center, size + gap, t, color);
        }
        if (s.CrossLines.Value)
        {
            Part(new Rect(center.x - gap - size, center.y - t / 2, size, t), s, color);
            Part(new Rect(center.x + gap, center.y - t / 2, size, t), s, color);
            Part(new Rect(center.x - t / 2, center.y - gap - size, t, size), s, color);
            Part(new Rect(center.x - t / 2, center.y + gap, t, size), s, color);
        }
        if (s.CrossDot.Value) Part(new Rect(center.x - t / 2, center.y - t / 2, t, t), s, color);
        GUI.color = old;
    }

    static void Part(Rect rect, HelperSettings s, Color color)
    {
        if (s.CrossOutline.Value)
        {
            GUI.color = new Color(0, 0, 0, color.a);
            GUI.DrawTexture(new Rect(rect.x - 1, rect.y - 1, rect.width + 2, rect.height + 2), Texture2D.whiteTexture);
        }
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
    }

    static void Ring(Vector2 center, float radius, float thickness, Color color)
    {
        GUI.color = color;
        for (int i = 0; i < 64; i++)
        {
            float a = i * Mathf.PI * 2 / 64, b = (i + 1) * Mathf.PI * 2 / 64;
            var start = center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
            var end = center + new Vector2(Mathf.Cos(b), Mathf.Sin(b)) * radius;
            var matrix = GUI.matrix;
            GUIUtility.RotateAroundPivot(Mathf.Atan2(end.y - start.y, end.x - start.x) * Mathf.Rad2Deg, start);
            GUI.DrawTexture(new Rect(start.x - 0.5f, start.y - thickness / 2, Vector2.Distance(start, end) + 1, thickness), Texture2D.whiteTexture);
            GUI.matrix = matrix;
        }
    }
}
