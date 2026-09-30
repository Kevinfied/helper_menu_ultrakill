using UnityEngine;

namespace EasyMode;

internal sealed class NoclipController
{
    NewMovement player;
    Rigidbody body;
    KeepInBounds bounds;
    VerticalClippingBlocker vertical;
    bool movementEnabled, kinematic, collisions, boundsEnabled, verticalEnabled;

    internal void Tick()
    {
        var current = MonoSingleton<NewMovement>.Instance;
        if (!Plugin.On(8) || !current || current.dead || current.levelOver || !current.activated)
        {
            Restore();
            return;
        }
        if (player != current)
        {
            Restore();
            if (!current.rb) return;
            player = current;
            body = current.rb;
            bounds = current.GetComponent<KeepInBounds>();
            vertical = current.GetComponent<VerticalClippingBlocker>();
            movementEnabled = current.enabled;
            kinematic = body.isKinematic;
            collisions = body.detectCollisions;
            boundsEnabled = bounds && bounds.enabled;
            verticalEnabled = vertical && vertical.enabled;
            if (!body.isKinematic) body.velocity = Vector3.zero;
            current.enabled = false;
            body.isKinematic = true;
            body.detectCollisions = false;
            if (bounds) bounds.enabled = false;
            if (vertical) vertical.enabled = false;
        }
        // Keep hovering while paused or adjusting the helper menu.
        if (!Plugin.Playing(current)) return;
        var camera = MonoSingleton<CameraController>.Instance;
        var input = MonoSingleton<InputManager>.Instance;
        if (!camera || !input) return;
        var move = input.InputSource.Move.ReadValue<Vector2>();
        var direction = camera.cam.transform.forward * move.y + camera.cam.transform.right * move.x;
        if (input.InputSource.Jump.IsPressed) direction += current.transform.up;
        if (input.InputSource.Slide.IsPressed) direction -= current.transform.up;
        float speed = input.InputSource.Dodge.IsPressed ? 60f : 24f;
        body.position += Vector3.ClampMagnitude(direction, 1f) * speed * Time.deltaTime;
    }

    internal void Restore()
    {
        if (body)
        {
            body.isKinematic = kinematic;
            body.detectCollisions = collisions;
            if (!kinematic) body.velocity = Vector3.zero;
        }
        if (bounds) bounds.enabled = boundsEnabled;
        if (vertical) vertical.enabled = verticalEnabled;
        if (player) player.enabled = movementEnabled;
        player = null;
        body = null;
        bounds = null;
        vertical = null;
    }
}
