using BepInEx.Configuration;
using UnityEngine;

namespace PRankHelper;

public sealed class RankTracker
{
    public readonly ConfigEntry<bool> Enabled, Extreme;
    StatsManager stats;
    int timeTarget, killTarget, styleTarget;
    bool valid;
    public int AliveEnemies { get; private set; }
    GUIStyle heading, text, faint, numbers;
    readonly ConfigEntry<float> opacity, scale, positionX, positionY;
    readonly ConfigEntry<Color> accent;
    readonly ConfigEntry<bool> showTitle, showLevel, showRestarts, showMajor, showStatus, showAlive;
    Vector2 settingsScroll;
    Rect hudRect;
    bool dragging;
    bool menuVisible;
    static readonly Color Gold = new Color(1f, 0.73f, 0.22f);

    public RankTracker(ConfigFile config)
    {
        Enabled = config.Bind("P-Rank Helper", "Progress HUD", true, "Always show level requirements and live progress while in a level, independent of the assist master switch.");
        Extreme = config.Bind("P-Rank Helper", "Extreme assist", false, "Outline-and-name-only enemy ESP. When integrated, also requires HelperMenu's master switch.");
        opacity = config.Bind("HUD appearance", "Background opacity", 0.94f, new ConfigDescription("", new AcceptableValueRange<float>(0, 1)));
        scale = config.Bind("HUD appearance", "Size", 1f, new ConfigDescription("", new AcceptableValueRange<float>(0.6f, 2f)));
        positionX = config.Bind("HUD appearance", "Position X", 0.01f, new ConfigDescription("", new AcceptableValueRange<float>(0, 1)));
        positionY = config.Bind("HUD appearance", "Position Y", 0.02f, new ConfigDescription("", new AcceptableValueRange<float>(0, 1)));
        accent = config.Bind("HUD appearance", "Accent", Gold);
        showTitle = config.Bind("HUD text", "Title", true);
        showLevel = config.Bind("HUD text", "Level", true);
        showRestarts = config.Bind("HUD text", "Restarts", true);
        showMajor = config.Bind("HUD text", "Major assists", true);
        showStatus = config.Bind("HUD text", "Rank status", true);
        showAlive = config.Bind("HUD text", "Enemies alive", true);
    }

    public bool ExtremeActive => Enabled.Value && Extreme.Value && valid && stats && !stats.endlessMode;

    public void Tick()
    {
        stats = MonoSingleton<StatsManager>.Instance;
        AliveEnemies = 0;
        var enemies = MonoSingleton<EnemyTracker>.Instance;
        if (stats && enemies)
            foreach (var enemy in enemies.enemies)
                if (enemy && !enemy.dead && enemy.gameObject.activeInHierarchy) AliveEnemies++;
        valid = stats && !stats.endlessMode && !SceneHelper.IsSceneRankless && !(stats.fr && stats.fr.casual) &&
            RankRules.TryThreshold(stats.timeRanks, true, out timeTarget) &&
            RankRules.TryThreshold(stats.killRanks, false, out killTarget) &&
            RankRules.TryThreshold(stats.styleRanks, false, out styleTarget);
    }

    void Styles()
    {
        if (heading != null) return;
        heading = new GUIStyle(GUI.skin.label) { fontSize = 15, fontStyle = FontStyle.Bold };
        text = new GUIStyle(GUI.skin.label) { fontSize = 12 };
        faint = new GUIStyle(GUI.skin.label) { fontSize = 10 };
        numbers = new GUIStyle(GUI.skin.label) { fontSize = 19, fontStyle = FontStyle.Bold };
        numbers.normal.textColor = Color.white;
        heading.normal.textColor = Gold;
        text.normal.textColor = Color.white;
        faint.normal.textColor = new Color(0.67f, 0.71f, 0.77f);
    }

    public void Draw(bool menuOpen)
    {
        if (!Enabled.Value || (!stats && !menuOpen)) return;
        Styles();
        heading.normal.textColor = accent.Value;
        menuVisible = menuOpen;
        float height = 16 + (showTitle.Value ? 27 : 0) + (showLevel.Value ? 20 : 0) + 126 +
            (showRestarts.Value ? 22 : 0) + (showMajor.Value ? 22 : 0) +
            (showStatus.Value ? 22 : 0) + (showAlive.Value ? 30 : 0);
        float zoom = Mathf.Min(scale.Value, Screen.width / 330f, Screen.height / height);
        float maxX = Mathf.Max(0, Screen.width / zoom - 330);
        float maxY = Mathf.Max(0, Screen.height / zoom - height);
        if (!dragging) hudRect = new Rect(positionX.Value * maxX, positionY.Value * maxY, 330, height);
        hudRect.width = 330;
        hudRect.height = height;
        hudRect.x = Mathf.Clamp(hudRect.x, 0, maxX);
        hudRect.y = Mathf.Clamp(hudRect.y, 0, maxY);
        var oldMatrix = GUI.matrix;
        GUI.matrix = Matrix4x4.Scale(new Vector3(zoom, zoom, 1));
        bool released = Event.current.rawType == EventType.MouseUp;
        hudRect = GUI.Window(172935, hudRect, DrawHud, "", GUIStyle.none);
        if (dragging && (released || !menuOpen))
        {
            positionX.Value = maxX == 0 ? 0 : Mathf.Clamp01(hudRect.x / maxX);
            positionY.Value = maxY == 0 ? 0 : Mathf.Clamp01(hudRect.y / maxY);
            dragging = false;
        }
        GUI.matrix = oldMatrix;
    }

    void DrawHud(int id)
    {
        Box(new Rect(0, 0, hudRect.width, hudRect.height), new Color(0.045f, 0.055f, 0.075f, opacity.Value));
        Box(new Rect(0, 0, hudRect.width, 2), accent.Value);
        float y = 10;
        if (showTitle.Value) { GUI.Label(new Rect(12, y, 306, 25), "P-RANK HELPER", heading); y += 27; }
        if (showLevel.Value) { GUI.Label(new Rect(12, y, 306, 20), stats ? SceneHelper.CurrentScene : "HUD PREVIEW", faint); y += 20; }
        if (valid || !stats)
        {
            Metric(y, "TIME", stats ? RankRules.Time(stats.seconds) + " / " + RankRules.Time(timeTarget) : "0:42 / 3:00", stats ? 1 - stats.seconds / timeTarget : 0.75f); y += 42;
            Metric(y, "KILLS", stats ? stats.kills + " / " + killTarget : "12 / 30", stats && killTarget > 0 ? (float)stats.kills / killTarget : 0.4f); y += 42;
            Metric(y, "STYLE", stats ? stats.stylePoints + " / " + styleTarget : "4500 / 8000", stats && styleTarget > 0 ? (float)stats.stylePoints / styleTarget : 0.56f); y += 42;
        }
        else { GUI.Label(new Rect(12, y, 306, 25), "P-rank unavailable for this mode/level.", text); y += 126; }
        var assist = MonoSingleton<AssistController>.Instance;
        bool major = stats && (stats.majorUsed || (assist && assist.majorEnabled));
        if (showRestarts.Value) { GUI.Label(new Rect(12, y, 306, 22), "Restarts: " + (stats ? stats.restarts : 0), text); y += 22; }
        if (showMajor.Value) { GUI.Label(new Rect(12, y, 306, 22), major ? "Major assists ON" : "Major assists OFF", text); y += 22; }
        if (showStatus.Value)
        {
            string status = valid ? RankRules.Status(stats.seconds, stats.kills, stats.stylePoints, timeTarget, killTarget,
                styleTarget, stats.restarts, major, assist && assist.cheatsEnabled, stats.infoSent) : "In progress";
            GUI.Label(new Rect(12, y, 306, 22), status, text); y += 22;
        }
        if (showAlive.Value) GUI.Label(new Rect(12, y, 306, 28), "Enemies alive: " + AliveEnemies, numbers);
        if (menuVisible)
        {
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 &&
                new Rect(0, 0, hudRect.width, hudRect.height).Contains(Event.current.mousePosition)) dragging = true;
            GUI.DragWindow(new Rect(0, 0, hudRect.width, hudRect.height));
        }
    }

    void Metric(float y, string name, string value, float fraction)
    {
        GUI.Label(new Rect(12, y + 3, 52, 22), name, faint);
        GUI.Label(new Rect(69, y - 2, 249, 28), value, numbers);
        Box(new Rect(12, y + 29, 306, 3), new Color(0.19f, 0.22f, 0.28f));
        Box(new Rect(12, y + 29, 306 * Mathf.Clamp01(fraction), 3), accent.Value);
    }

    public void DrawDetails(Rect area)
    {
        Styles();
        settingsScroll = GUI.BeginScrollView(area, settingsScroll, new Rect(0, 0, area.width - 22, 440));
        GUI.Label(new Rect(0, 0, 500, 23), "HUD APPEARANCE - drag the HUD while this menu is open", text);
        Slider(30, "Background opacity", opacity, 0, 1);
        Slider(62, "HUD size", scale, 0.6f, 2);
        var c = accent.Value;
        c.r = SliderValue(94, "Accent red", c.r, 0, 1);
        c.g = SliderValue(126, "Accent green", c.g, 0, 1);
        c.b = SliderValue(158, "Accent blue", c.b, 0, 1);
        if (c != accent.Value) accent.Value = c;
        Visibility(198, 0, "Title", showTitle);
        Visibility(198, 270, "Level name", showLevel);
        Visibility(230, 0, "Restarts", showRestarts);
        Visibility(230, 270, "Major assists", showMajor);
        Visibility(262, 0, "Rank status", showStatus);
        Visibility(262, 270, "Enemies alive", showAlive);
        GUI.Label(new Rect(0, 301, 530, 24), "Rank status indicates whether the run still meets P-rank requirements.", faint);
        if (GUI.Button(new Rect(0, 338, 200, 28), "Reset HUD position and size"))
        {
            dragging = false;
            positionX.Value = 0.01f; positionY.Value = 0.02f; scale.Value = 1;
        }
        GUI.Label(new Rect(0, 382, 530, 24), "Enemies alive excludes future spawns and inactive encounters.", faint);
        GUI.EndScrollView();
    }

    void Visibility(float y, float x, string label, ConfigEntry<bool> setting)
    {
        bool next = GUI.Toggle(new Rect(x, y, 250, 26), setting.Value, label);
        if (next != setting.Value) setting.Value = next;
    }
    void Slider(float y, string label, ConfigEntry<float> setting, float min, float max)
    {
        float next = SliderValue(y, label, setting.Value, min, max);
        if (next != setting.Value) setting.Value = next;
    }
    float SliderValue(float y, string label, float value, float min, float max)
    {
        GUI.Label(new Rect(0, y, 180, 24), label, text);
        float next = GUI.HorizontalSlider(new Rect(186, y + 7, 270, 20), value, min, max);
        GUI.Label(new Rect(470, y, 70, 24), Mathf.RoundToInt(next * 100) + "%", text);
        return next;
    }

    static void Box(Rect rect, Color color)
    {
        var previous = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = previous;
    }
}
