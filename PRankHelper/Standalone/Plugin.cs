using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

namespace PRankHelper.Standalone;

[BepInPlugin("com.realk.ultrakill.prankhelper", "P-Rank Helper", "1.0.0")]
public sealed class Plugin : BaseUnityPlugin
{
    const string StateKey = "PRankHelper.Menu";
    static RankTracker tracker;
    readonly EnemyEsp esp = new EnemyEsp();
    ConfigEntry<KeyCode> menuKey;
    bool visible;
    GUIStyle title, subtitle;

    void Awake()
    {
        tracker = RankRuntime.Get(Logger);
        menuKey = Config.Bind("Interface", "Menu key", KeyCode.F5, "Open the standalone P-Rank Helper menu.");
        Logger.LogInfo("P-Rank Helper loaded. F5 standalone menu; integrates automatically with HelperMenu when installed.");
    }

    void Update()
    {
        if (RankRuntime.Integrated)
        {
            Close();
            if (Input.GetKeyDown(menuKey.Value)) RankRuntime.ToggleIntegratedMenu = true;
            return;
        }
        tracker.Tick();
        if (Input.GetKeyDown(menuKey.Value)) { if (visible) Close(); else visible = true; }
        if (visible && Input.GetKeyDown(KeyCode.Escape)) Close();
        var state = GameStateManager.Instance;
        if (visible && state && !state.IsStateActive(StateKey))
            state.RegisterState(new GameState(StateKey) { priority = 100, cursorLock = LockMode.Unlock,
                playerInputLock = LockMode.Lock, cameraInputLock = LockMode.Lock });
    }

    void OnGUI()
    {
        if (RankRuntime.Integrated || tracker == null) return;
        if (tracker.ExtremeActive) esp.Draw();
        tracker.Draw(visible);
        if (!visible) return;
        if (title == null)
        {
            title = new GUIStyle(GUI.skin.label) { fontSize = 24, fontStyle = FontStyle.Bold };
            title.normal.textColor = new Color(1, 0.73f, 0.22f);
            subtitle = new GUIStyle(GUI.skin.label) { fontSize = 12 };
            subtitle.normal.textColor = new Color(0.7f, 0.74f, 0.8f);
        }
        var oldMatrix = GUI.matrix;
        float scale = Mathf.Min(1, Screen.width / 660f, Screen.height / 560f);
        GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 620 * scale) / 2, (Screen.height - 510 * scale) / 2), Quaternion.identity, Vector3.one * scale);
        var oldColor = GUI.color;
        GUI.color = new Color(0.035f, 0.045f, 0.065f, 0.98f);
        GUI.DrawTexture(new Rect(0, 0, 620, 510), Texture2D.whiteTexture);
        GUI.color = new Color(1, 0.73f, 0.22f);
        GUI.DrawTexture(new Rect(0, 0, 620, 3), Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUI.Label(new Rect(24, 20, 500, 38), "P-RANK HELPER", title);
        GUI.Label(new Rect(24, 61, 550, 25), "LIVE REQUIREMENTS  /  ENEMIES ALIVE  /  EXTREME ASSIST", subtitle);
        if (GUI.Button(new Rect(556, 24, 40, 30), "X")) Close();
        Toggle(new Rect(24, 106, 570, 30), tracker.Enabled, "Always show requirements and progress");
        Toggle(new Rect(24, 146, 570, 30), tracker.Extreme, "Extreme assist - enemy outlines and names");
        tracker.DrawDetails(new Rect(24, 194, 572, 265));
        GUI.Label(new Rect(24, 475, 570, 24), menuKey.Value + " / ESC to close  |  Live enemy count - no route setup required", subtitle);
        GUI.color = oldColor;
        GUI.matrix = oldMatrix;
    }

    static void Toggle(Rect rect, ConfigEntry<bool> setting, string label)
    {
        bool value = GUI.Toggle(rect, setting.Value, "  " + label);
        if (value != setting.Value) setting.Value = value;
    }
    void Close()
    {
        visible = false;
        var state = GameStateManager.Instance;
        if (state && state.IsStateActive(StateKey)) state.PopState(StateKey);
    }
    void OnDisable() => Close();
    void OnDestroy() => Close();
}
