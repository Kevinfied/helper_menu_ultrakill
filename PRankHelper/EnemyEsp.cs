using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PRankHelper;

public enum OutlineStyle { Box, Corners }

public sealed class EnemyEsp
{
    GUIStyle label;
    readonly StringBuilder labelText = new StringBuilder();
    readonly Dictionary<EnemyIdentifier, MonoBehaviour[]> states = new Dictionary<EnemyIdentifier, MonoBehaviour[]>();
    readonly GUIContent content = new GUIContent();
    readonly Dictionary<EnemyIdentifier, Collider[]> hitboxes = new Dictionary<EnemyIdentifier, Collider[]>();
    readonly List<Rect> labels = new List<Rect>();
    readonly Vector3[] corners = new Vector3[8];
    float refreshAt;

    public void Draw(bool boxes = true, bool names = true, bool health = false, bool conditionsEnabled = false,
        bool sanded = true, bool blessed = true, bool puppet = true, bool radiant = true, bool enraged = true, bool hideOverlap = true,
        OutlineStyle outline = OutlineStyle.Box, Color? color = null, float thickness = 1f, float infoOpacity = 0.65f)
    {
        var player = MonoSingleton<NewMovement>.Instance;
        if (Event.current.type != EventType.Repaint || !player || !player.activated || player.dead ||
            player.levelOver || Time.timeScale <= 0 || GameStateManager.Instance.PlayerInputLocked) return;
        var tracker = MonoSingleton<EnemyTracker>.Instance;
        var camera = Camera.main;
        if (!tracker || !camera) return;
        if (label == null)
        {
            label = new GUIStyle(GUI.skin.label) {
                alignment = TextAnchor.MiddleCenter, fontSize = 10,
                padding = new RectOffset(3, 3, 1, 1), wordWrap = false
            };
            label.normal.textColor = Color.white;
        }
        if (Time.unscaledTime >= refreshAt)
        {
            hitboxes.Clear();
            states.Clear();
            refreshAt = Time.unscaledTime + 0.75f;
        }
        labels.Clear();
        var oldColor = GUI.color;
        foreach (var enemy in tracker.enemies)
        {
            if (!enemy || enemy.dead || !enemy.gameObject.activeInHierarchy) continue;
            if (!hitboxes.TryGetValue(enemy, out var colliders))
            {
                colliders = enemy.GetComponentsInChildren<Collider>();
                hitboxes.Add(enemy, colliders);
                states[enemy] = enemy.GetComponents<MonoBehaviour>();
            }
            float left = float.PositiveInfinity, top = float.PositiveInfinity;
            float right = float.NegativeInfinity, bottom = float.NegativeInfinity;
            bool found = false;
            foreach (var collider in colliders)
            {
                if (!collider || !collider.enabled || !collider.gameObject.activeInHierarchy) continue;
                var part = collider.GetComponent<EnemyIdentifierIdentifier>();
                // Only damage hitboxes; exclude attack volumes, sensors and detached limbs.
                if (!part || part.eid != enemy || !collider.transform.IsChildOf(enemy.transform)) continue;
                found |= Project(camera, collider, ref left, ref top, ref right, ref bottom);
            }
            if (!found)
            {
                // Some enemies have only a root body collider.
                foreach (var collider in colliders)
                    if (collider && collider.enabled && collider.gameObject.activeInHierarchy &&
                        !collider.isTrigger && collider.transform == enemy.transform)
                        found |= Project(camera, collider, ref left, ref top, ref right, ref bottom);
            }
            if (!found || right < 0 || bottom < 0 || left > Screen.width || top > Screen.height) continue;
            left = Mathf.Clamp(left, 0, Screen.width - 1);
            right = Mathf.Clamp(right, 0, Screen.width - 1);
            top = Mathf.Clamp(top, 0, Screen.height - 1);
            bottom = Mathf.Clamp(bottom, 0, Screen.height - 1);
            if (right - left < 2 || bottom - top < 2) continue;
            if (boxes)
            {
                GUI.color = color ?? new Color(1f, 0.2f, 0.25f, 0.85f);
                DrawOutline(new Rect(left, top, right - left, bottom - top), outline, thickness);
            }
            labelText.Clear();
            if (names) labelText.Append(enemy.enemyType);
            if (health)
            {
                if (labelText.Length > 0) labelText.Append("  |  ");
                labelText.Append("HP ").Append(enemy.health.ToString("0.#"));
            }
            if (conditionsEnabled)
            {
                var conditions = new List<string>();
                if (sanded && enemy.sandified) conditions.Add("SANDED");
                if (blessed && enemy.blessed) conditions.Add("BLESSED");
                if (puppet && enemy.puppet) conditions.Add("PUPPET");
                if (radiant && (enemy.healthBuff || enemy.speedBuff || enemy.damageBuff)) conditions.Add("RADIANT");
                if (enraged)
                    foreach (var state in states[enemy])
                        if (state && state is IEnrage enrage && enrage.isEnraged) { conditions.Add("ENRAGED"); break; }
                if (conditions.Count > 0)
                {
                    if (labelText.Length > 0) labelText.Append('\n');
                    labelText.Append(string.Join(" / ", conditions));
                }
            }
            if (labelText.Length == 0) continue;
            content.text = labelText.ToString();
            var size = label.CalcSize(content);
            var rect = new Rect(Mathf.Clamp((left + right - size.x) * 0.5f, 0, Mathf.Max(0, Screen.width - size.x)),
                Mathf.Max(0, top - size.y - 2), size.x, size.y);
            bool overlaps = false;
            foreach (var occupied in labels)
                if (occupied.Overlaps(rect)) { overlaps = true; break; }
            if (hideOverlap && overlaps) continue; // Keep every outline, but omit overlapping name labels.
            labels.Add(new Rect(rect.x - 2, rect.y - 2, rect.width + 4, rect.height + 4));
            GUI.color = new Color(0, 0, 0, Mathf.Clamp01(infoOpacity));
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(rect, content, label);
        }
        GUI.color = oldColor;
    }

    public static void DrawOutline(Rect rect, OutlineStyle style, float thickness)
    {
        float t = Mathf.Min(Mathf.Clamp(thickness, 1, 6), Mathf.Min(rect.width, rect.height) * 0.5f);
        float w = style == OutlineStyle.Corners ? rect.width * 0.25f : rect.width * 0.5f;
        float h = style == OutlineStyle.Corners ? rect.height * 0.25f : rect.height * 0.5f;
        for (int x = 0; x < 2; x++)
            for (int y = 0; y < 2; y++)
            {
                GUI.DrawTexture(new Rect(x == 0 ? rect.x : rect.xMax - w, y == 0 ? rect.y : rect.yMax - t, w, t), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(x == 0 ? rect.x : rect.xMax - t, y == 0 ? rect.y : rect.yMax - h, t, h), Texture2D.whiteTexture);
            }
    }

    bool Project(Camera camera, Collider collider, ref float left, ref float top, ref float right, ref float bottom)
    {
        // Preserve orientation for box hitboxes; other collider types use their world bounds.
        var box = collider as BoxCollider;
        var bounds = box ? new Bounds(box.center, box.size) : collider.bounds;
        for (int i = 0; i < 8; i++)
        {
            var point = bounds.center + Vector3.Scale(bounds.extents,
                new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
            if (box) point = box.transform.TransformPoint(point);
            corners[i] = camera.transform.InverseTransformPoint(point);
        }
        bool found = false;
        float near = camera.nearClipPlane + 0.001f;
        for (int i = 0; i < 8; i++)
        {
            if (corners[i].z >= near)
            {
                Include(camera, corners[i], ref left, ref top, ref right, ref bottom);
                found = true;
            }
            // Clip all twelve bounding-box edges against the near plane, avoiding
            // inverted or enormous boxes when an enemy straddles the camera.
            for (int bit = 1; bit <= 4; bit <<= 1)
            {
                int j = i ^ bit;
                if (j <= i || (corners[i].z >= near) == (corners[j].z >= near)) continue;
                var clipped = Vector3.Lerp(corners[i], corners[j], (near - corners[i].z) / (corners[j].z - corners[i].z));
                Include(camera, clipped, ref left, ref top, ref right, ref bottom);
                found = true;
            }
        }
        return found;
    }

    static void Include(Camera camera, Vector3 local, ref float left, ref float top, ref float right, ref float bottom)
    {
        var point = camera.WorldToScreenPoint(camera.transform.TransformPoint(local));
        left = Mathf.Min(left, point.x);
        right = Mathf.Max(right, point.x);
        top = Mathf.Min(top, Screen.height - point.y);
        bottom = Mathf.Max(bottom, Screen.height - point.y);
    }
}

