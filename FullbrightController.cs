using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace EasyMode;

internal sealed class FullbrightController
{
    GameObject lightObject;
    bool applied, fog;
    int scene;
    Color ambient;
    AmbientMode mode;
    float intensity;
    static readonly Color BrightAmbient = new Color(0.65f, 0.65f, 0.65f);

    internal void Tick()
    {
        var player = MonoSingleton<NewMovement>.Instance;
        var camera = MonoSingleton<CameraController>.Instance;
        if (!Plugin.On(9) || !player || !camera) { Restore(); return; }
        int currentScene = SceneManager.GetActiveScene().handle;
        if (!applied || scene != currentScene)
        {
            // A new scene owns new RenderSettings; never restore the previous scene over it.
            if (lightObject) Object.Destroy(lightObject);
            scene = currentScene;
            fog = RenderSettings.fog;
            ambient = RenderSettings.ambientLight;
            mode = RenderSettings.ambientMode;
            intensity = RenderSettings.ambientIntensity;
            applied = true;
            lightObject = new GameObject("HelperMenu Fullbright") { hideFlags = HideFlags.DontSave };
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = Color.white;
            light.intensity = 1f;
            light.shadows = LightShadows.None;
        }
        else
        {
            // Remember lighting changes made by level scripts while the override is active.
            if (RenderSettings.fog) fog = true;
            if (RenderSettings.ambientLight != BrightAmbient) ambient = RenderSettings.ambientLight;
            if (RenderSettings.ambientMode != AmbientMode.Flat) mode = RenderSettings.ambientMode;
            if (!Mathf.Approximately(RenderSettings.ambientIntensity, 1f)) intensity = RenderSettings.ambientIntensity;
        }
        RenderSettings.fog = false;
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = BrightAmbient;
        RenderSettings.ambientIntensity = 1f;
        if (lightObject) lightObject.transform.rotation = camera.cam.transform.rotation;
    }

    internal void Restore()
    {
        if (lightObject) Object.Destroy(lightObject);
        lightObject = null;
        if (applied && scene == SceneManager.GetActiveScene().handle)
        {
            RenderSettings.fog = fog;
            RenderSettings.ambientMode = mode;
            RenderSettings.ambientLight = ambient;
            RenderSettings.ambientIntensity = intensity;
        }
        applied = false;
    }
}
