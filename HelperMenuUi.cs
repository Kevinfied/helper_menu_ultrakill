using System.Collections.Generic;
using System.Text;
using BepInEx.Configuration;
using UnityEngine;

namespace EasyMode;

internal sealed class HelperMenuUi
{
    const string StateKey = "helper-menu-settings";
    readonly Plugin plugin;
    readonly string[] tabs = { "Combat", "Movement", "Weapons", "Visuals", "Interface" };
    readonly List<(string name, ConfigEntry<bool> value, string hint)> rows = new List<(string, ConfigEntry<bool>, string)>();
    readonly StringBuilder active = new StringBuilder();
    readonly Vector3[] styleCorners = new Vector3[4];
    readonly string[] shortNames = { "", "Parry", "Invulnerable", "Jumps", "Stamina", "Rapid fire", "ESP", "Unlimited", "Noclip", "Fullbright" };
    Rect window = new Rect(28, 54, 620, 574);
    Vector2 scroll;
    bool visible;
    int tab, selected;
    int dropdown = -1, dropdownItem;
    GUIStyle title, text, muted, small, toggleText;
    static readonly Color Red = new Color(0.95f, 0.18f, 0.24f);

    internal HelperMenuUi(Plugin plugin) { this.plugin = plugin; BuildRows(); }

    internal void Update()
    {
        if (Input.GetKeyDown(plugin.Settings.MasterKey.Value)) plugin.options[0].Value = !plugin.options[0].Value;
        if (Input.GetKeyDown(plugin.Settings.MenuKey.Value)) { if (visible) Close(); else visible = true; }
        var state = GameStateManager.Instance;
        if (visible && state && !state.IsStateActive(StateKey))
            state.RegisterState(new GameState(StateKey) { priority = 100, cursorLock = LockMode.Unlock, playerInputLock = LockMode.Lock, cameraInputLock = LockMode.Lock });
        if (!visible) return;
        if (dropdown >= 0)
        {
            if (Input.GetKeyDown(KeyCode.Escape)) dropdown = -1;
            if (Input.GetKeyDown(KeyCode.DownArrow)) dropdownItem = (dropdownItem + 2) % HelperSettings.Weapons.Length;
            if (Input.GetKeyDown(KeyCode.UpArrow)) dropdownItem = (dropdownItem + HelperSettings.Weapons.Length - 2) % HelperSettings.Weapons.Length;
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow)) dropdownItem ^= 1;
            if (Input.GetKeyDown(KeyCode.Return))
            {
                var values = dropdown == 0 ? plugin.Settings.Rapid : plugin.Settings.Unlimited;
                values[dropdownItem].Value = !values[dropdownItem].Value;
            }
            return;
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow)) SetTab((tab + tabs.Length - 1) % tabs.Length);
        if (Input.GetKeyDown(KeyCode.RightArrow)) SetTab((tab + 1) % tabs.Length);
        if (Input.GetKeyDown(KeyCode.DownArrow)) Select((selected + 1) % rows.Count);
        if (Input.GetKeyDown(KeyCode.UpArrow)) Select((selected + rows.Count - 1) % rows.Count);
        if (Input.GetKeyDown(KeyCode.Return)) rows[selected].value.Value = !rows[selected].value.Value;
        if (tab == 2 && Input.GetKeyDown(KeyCode.Space)) { dropdown = selected; dropdownItem = 0; }
    }

    internal void Close()
    {
        visible = false;
        dropdown = -1;
        var state = GameStateManager.Instance;
        if (state && state.IsStateActive(StateKey)) state.PopState(StateKey);
    }
    void Select(int value) { selected = value; scroll.y = Mathf.Max(0, selected * 43 - 230); }
    void SetTab(int value) { tab = value; selected = 0; dropdown = -1; scroll = Vector2.zero; BuildRows(); }
    void Add(string name, ConfigEntry<bool> value, string hint) => rows.Add((name, value, hint));
    void BuildRows()
    {
        rows.Clear();
        var s = plugin.Settings;
        switch (tab)
        {
            case 0:
                Add("Auto-parry", plugin.options[1], "Blue arm equipped, nearby attacks in front of you.");
                Add("  Enemy attacks", s.ParryMelee, "Melee and boss parry windows; respects valid hitboxes.");
                Add("  Projectiles & objects", s.ParryObjects, "Projectiles, swords, cannonballs, mines and other parry objects.");
                Add("Invulnerability", plugin.options[2], "Blocks damage handled by the player's damage routine.");
                break;
            case 1:
                Add("Infinite jumps", plugin.options[3], "Press jump again in the air; normal jump debounce remains.");
                Add("Infinite stamina", plugin.options[4], "Keep dash stamina replenished while playing.");
                Add("Noclip", plugin.options[8], "Fly through walls. Jump rises, slide descends, dodge boosts speed.");
                break;
            case 2:
                Add("Rapid fire", plugin.options[5], "Faster primary fire on your selected weapons.");
                Add("Unlimited", plugin.options[7], "No cooldowns and infinite ammo on your selected weapons.");
                break;
            case 3:
                Add("Fullbright", plugin.options[9], "Brightens dark areas and removes fog. Original lighting returns when off.");
                Add("Enemy ESP", plugin.options[6], "See enemy hitboxes and optional labels through walls.");
                Add("Hitbox outlines", s.Boxes, "Thin boxes fitted to damage-collider bounds.");
                Add("Names", s.Names, "Small enemy-type labels.");
                Add("HP", s.Health, "Current enemy health.");
                Add("Conditions", s.Conditions, "Show only the active conditions selected below.");
                Add("  Sanded", s.Sanded, "Sanded enemies.");
                Add("  Enraged", s.Enraged, "Enemies reporting an active enrage state.");
                Add("  Blessed", s.Blessed, "Blessing protection.");
                Add("  Radiant", s.Radiant, "Active health, speed or damage buffs.");
                Add("  Puppet", s.Puppet, "Puppet enemies.");
                Add("Avoid overlapping labels", s.HideOverlap, "Hide colliding text panels while retaining all outlines.");
                break;
            case 4:
                Add("Active tools display", s.ActiveTools, "Tiny top-right list; moves aside when it overlaps the style HUD.");
                break;
        }
    }

    internal void Draw()
    {
        if (title == null)
        {
            title = new GUIStyle(GUI.skin.label) { fontSize = 24, fontStyle = FontStyle.Bold };
            text = new GUIStyle(GUI.skin.label) { fontSize = 13 };
            muted = new GUIStyle(GUI.skin.label) { fontSize = 11 };
            small = new GUIStyle(GUI.skin.label) { fontSize = 10, alignment = TextAnchor.MiddleRight };
            toggleText = new GUIStyle(text) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            title.normal.textColor = text.normal.textColor = small.normal.textColor = toggleText.normal.textColor = Color.white;
            muted.normal.textColor = new Color(0.56f, 0.6f, 0.67f);
        }
        DrawActive();
        if (!visible) return;
        float scale = Mathf.Min(1f, Mathf.Min(Screen.width / 680f, Screen.height / 650f));
        var oldMatrix = GUI.matrix;
        var oldColor = GUI.color;
        GUI.color = Color.white;
        GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1));
        window.x = Mathf.Clamp(window.x, 0, Mathf.Max(0, Screen.width / scale - window.width));
        window.y = Mathf.Clamp(window.y, 0, Mathf.Max(0, Screen.height / scale - window.height));
        window = GUI.Window(172934, window, DrawWindow, "", GUIStyle.none);
        GUI.matrix = oldMatrix;
        GUI.color = oldColor;
    }

    void DrawWindow(int id)
    {
        Fill(new Rect(0, 0, 620, 574), new Color(0.055f, 0.065f, 0.085f, 0.98f));
        Fill(new Rect(0, 0, 620, 3), Red);
        GUI.Label(new Rect(22, 15, 480, 34), "HELPER MENU", title);
        GUI.Label(new Rect(24, 51, 540, 20), "ultrakill_but_im_a_cs2_premier_cheater", muted);
        if (GUI.Button(new Rect(575, 16, 25, 25), "X", toggleText)) Close();
        Fill(new Rect(20, 82, 580, 38), new Color(0.1f, 0.115f, 0.14f));
        GUI.Label(new Rect(32, 89, 340, 25), "MASTER SWITCH", text);
        Toggle(new Rect(520, 88, 67, 25), plugin.options[0]);
        for (int i = 0; i < tabs.Length; i++)
        {
            var rect = new Rect(20 + i * 116, 135, 112, 32);
            Fill(rect, tab == i ? Red : new Color(0.1f, 0.115f, 0.14f));
            if (GUI.Button(rect, tabs[i], toggleText)) SetTab(i);
        }
        GUI.Label(new Rect(22, 177, 570, 22), "Settings save automatically. Sub-options apply when their feature is enabled.", muted);
        if (tab == 2) DrawWeapons();
        else
        {
        scroll = GUI.BeginScrollView(new Rect(20, 205, 580, 308), scroll, new Rect(0, 0, 556, rows.Count * 43));
        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            Fill(new Rect(0, i * 43, 556, 40), i == selected ? new Color(0.16f, 0.18f, 0.22f) : new Color(0.09f, 0.105f, 0.13f));
            if (i == selected) Fill(new Rect(0, i * 43, 2, 40), Red);
            GUI.Label(new Rect(12, i * 43 + 2, 455, 22), row.name, text);
            GUI.Label(new Rect(12, i * 43 + 21, 467, 18), row.hint, muted);
            Toggle(new Rect(480, i * 43 + 8, 65, 24), row.value);
        }
        GUI.EndScrollView();
        }
        GUI.Label(new Rect(22, 550, 580, 20), $"{plugin.Settings.MenuKey.Value} close  /  {plugin.Settings.MasterKey.Value} master  /  Left-Right tabs  /  Up-Down select  /  Enter toggle", muted);
        GUI.DragWindow(new Rect(0, 0, 560, 76));
    }
    void DrawWeapons()
    {
        bool previousEnabled = GUI.enabled;
        GUI.enabled = dropdown < 0;
        for (int i = 0; i < 2; i++)
        {
            float y = 205 + i * 124;
            var values = i == 0 ? plugin.Settings.Rapid : plugin.Settings.Unlimited;
            Fill(new Rect(20, y, 580, 110), new Color(0.09f, 0.105f, 0.13f));
            if (i == selected) Fill(new Rect(20, y, 2, 110), Red);
            GUI.Label(new Rect(32, y + 8, 440, 24), rows[i].name, text);
            GUI.Label(new Rect(32, y + 32, 530, 20), rows[i].hint, muted);
            Toggle(new Rect(520, y + 8, 65, 24), rows[i].value);
            var selector = new Rect(32, y + 60, 552, 34);
            Fill(selector, selector.Contains(Event.current.mousePosition) && dropdown < 0 ? new Color(0.22f, 0.24f, 0.29f) : new Color(0.14f, 0.16f, 0.20f));
            int count = 0, only = 0;
            for (int j = 0; j < values.Length; j++) if (values[j].Value) { count++; only = j; }
            string summary = count == values.Length ? "All weapons" : count == 0 ? "No weapons" :
                count == 1 ? HelperSettings.Weapons[only] : count + " weapons selected";
            GUI.Label(new Rect(44, y + 65, 70, 22), "WEAPONS", muted);
            GUI.Label(new Rect(126, y + 65, 390, 22), summary, text);
            Chevron(new Vector2(560, y + 77));
            if (GUI.Button(selector, GUIContent.none, GUIStyle.none))
            { dropdown = i; dropdownItem = 0; }
        }
        GUI.enabled = previousEnabled;
        GUI.Label(new Rect(22, 466, 570, 24), "Unlimited also removes firing delays on its selected weapons.", muted);
        GUI.Label(new Rect(22, 490, 570, 24), "Space opens the selected dropdown. Enter toggles a weapon. Esc closes it.", muted);
        if (dropdown < 0) return;
        var entries = dropdown == 0 ? plugin.Settings.Rapid : plugin.Settings.Unlimited;
        var popup = new Rect(32, 244, 552, 290);
        bool outside = Event.current.type == EventType.MouseDown && !popup.Contains(Event.current.mousePosition);
        if (outside) { dropdown = -1; Event.current.Use(); return; }
        Fill(new Rect(popup.x + 5, popup.y + 6, popup.width, popup.height), new Color(0, 0, 0, 0.45f));
        Fill(popup, new Color(0.24f, 0.27f, 0.33f));
        Fill(new Rect(popup.x + 1, popup.y + 1, popup.width - 2, popup.height - 2), new Color(0.07f, 0.085f, 0.11f));
        Fill(new Rect(popup.x + 1, popup.y + 1, popup.width - 2, 2), Red);
        GUI.Label(new Rect(46, 254, 295, 23), dropdown == 0 ? "RAPID FIRE / WEAPONS" : "UNLIMITED / WEAPONS", text);
        int selectedCount = 0;
        foreach (var entry in entries) if (entry.Value) selectedCount++;
        GUI.Label(new Rect(398, 254, 170, 23), selectedCount + " / " + entries.Length + " selected", small);
        GUI.Label(new Rect(46, 279, 470, 20), "Choose any combination. Your selection saves immediately.", muted);
        for (int i = 0; i < entries.Length; i++)
        {
            var item = new Rect(46 + i % 2 * 263, 306 + i / 2 * 43, 251, 37);
            bool hovered = item.Contains(Event.current.mousePosition);
            bool on = entries[i].Value;
            var color = on ? new Color(0.24f, 0.105f, 0.14f) : new Color(0.105f, 0.125f, 0.16f);
            if (hovered || dropdownItem == i) color += new Color(0.065f, 0.065f, 0.065f, 0);
            Fill(item, color);
            if (on) Fill(new Rect(item.x, item.y, 2, item.height), Red);
            var check = new Rect(item.x + 11, item.y + 11, 15, 15);
            Fill(check, on ? Red : new Color(0.33f, 0.37f, 0.44f));
            if (on)
            {
                Stroke(new Vector2(check.x + 3, check.y + 7), new Vector2(check.x + 6, check.y + 10), Color.white);
                Stroke(new Vector2(check.x + 6, check.y + 10), new Vector2(check.x + 12, check.y + 4), Color.white);
            }
            else Fill(new Rect(check.x + 1, check.y + 1, 13, 13), color);
            GUI.Label(new Rect(item.x + 36, item.y + 8, 208, 24), HelperSettings.Weapons[i], text);
            if (GUI.Button(item, GUIContent.none, GUIStyle.none)) { entries[i].Value = !entries[i].Value; dropdownItem = i; }
        }
        Fill(new Rect(46, 486, 524, 1), new Color(0.21f, 0.24f, 0.29f));
        if (DropdownAction(new Rect(46, 496, 88, 26), "Select all")) foreach (var entry in entries) entry.Value = true;
        if (DropdownAction(new Rect(142, 496, 88, 26), "Clear all")) foreach (var entry in entries) entry.Value = false;
        GUI.Label(new Rect(245, 500, 240, 20), "Arrows navigate / Enter toggles", muted);
        if (DropdownAction(new Rect(498, 496, 72, 26), "Done", true)) dropdown = -1;
    }

    bool DropdownAction(Rect rect, string caption, bool accent = false)
    {
        bool hover = rect.Contains(Event.current.mousePosition);
        Fill(rect, accent ? Red : hover ? new Color(0.27f, 0.3f, 0.36f) : new Color(0.17f, 0.20f, 0.25f));
        return GUI.Button(rect, caption, toggleText);
    }
    static void Chevron(Vector2 center)
    {
        Stroke(center + new Vector2(-4, -2), center + new Vector2(0, 2), Color.white);
        Stroke(center + new Vector2(0, 2), center + new Vector2(4, -2), Color.white);
    }
    static void Stroke(Vector2 start, Vector2 end, Color color)
    {
        var matrix = GUI.matrix;
        GUIUtility.RotateAroundPivot(Mathf.Atan2(end.y - start.y, end.x - start.x) * Mathf.Rad2Deg, start);
        Fill(new Rect(start.x, start.y, Vector2.Distance(start, end), 1.5f), color);
        GUI.matrix = matrix;
    }
    void Toggle(Rect rect, ConfigEntry<bool> value)
    {
        Fill(rect, value.Value ? Red : new Color(0.2f, 0.23f, 0.28f));
        if (GUI.Button(rect, value.Value ? "ON" : "OFF", toggleText)) value.Value = !value.Value;
    }
    static void Fill(Rect rect, Color color)
    {
        var previous = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = previous;
    }

    void DrawActive()
    {
        if (!plugin.Settings.ActiveTools.Value || !Plugin.On(0)) return;
        var player = MonoSingleton<NewMovement>.Instance;
        if (!player || player.dead || !player.activated) return;
        active.Clear();
        int count = 0;
        for (int i = 1; i < plugin.options.Length; i++)
        {
            if (!Plugin.On(i)) continue;
            if (i == 1 && !plugin.Settings.ParryMelee.Value && !plugin.Settings.ParryObjects.Value) continue;
            if (i == 5 && !Any(plugin.Settings.Rapid)) continue;
            if (i == 7 && !Any(plugin.Settings.Unlimited)) continue;
            if (i == 6 && !plugin.Settings.Boxes.Value && !plugin.Settings.Names.Value &&
                !plugin.Settings.Health.Value && !plugin.Settings.Conditions.Value) continue;
            if (count > 0) active.Append(count % 3 == 0 ? "\n" : "  /  ");
            active.Append(shortNames[i]);
            count++;
        }
        if (count == 0) return;
        float width = small.CalcSize(new GUIContent(active.ToString())).x + 14;
        var rect = new Rect(Screen.width - width - 12, 10, width, ((count + 2) / 3) * 13 + 8);
        // Respect the actual style panel's screen rectangle, including custom HUD placement.
        var style = MonoSingleton<StyleHUD>.Instance;
        if (style && style.transform.childCount > 0 && style.transform.GetChild(0) is RectTransform panel && panel.gameObject.activeInHierarchy)
        {
            panel.GetWorldCorners(styleCorners);
            var canvas = panel.GetComponentInParent<Canvas>();
            var camera = canvas && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
            var a = RectTransformUtility.WorldToScreenPoint(camera, styleCorners[0]);
            var b = RectTransformUtility.WorldToScreenPoint(camera, styleCorners[2]);
            var reserved = Rect.MinMaxRect(a.x, Screen.height - b.y, b.x, Screen.height - a.y);
            if (rect.Overlaps(reserved))
            {
                rect.x = reserved.xMin - width - 8;
                if (rect.x < 0) return; // Never draw over the style panel if no room is available.
            }
        }
        Fill(rect, new Color(0.035f, 0.04f, 0.055f, 0.75f));
        Fill(new Rect(rect.xMax - 2, rect.y, 2, rect.height), Red);
        GUI.Label(new Rect(rect.x + 5, rect.y + 2, width - 12, rect.height - 4), active.ToString(), small);
    }
    static bool Any(ConfigEntry<bool>[] entries)
    {
        foreach (var entry in entries) if (entry.Value) return true;
        return false;
    }
}


