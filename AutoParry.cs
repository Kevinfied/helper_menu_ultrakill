using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace EasyMode;

internal sealed class AutoParry
{
    Collider[] nearby = new Collider[128];
    readonly HashSet<Transform> handled = new HashSet<Transform>();
    readonly HashSet<Transform> eligible = new HashSet<Transform>();
    readonly HashSet<EnemyIdentifier> hitEnemies = new HashSet<EnemyIdentifier>();
    static readonly MethodInfo parry = AccessTools.Method(typeof(Punch), "TryParryProjectile");
    static readonly MethodInfo punchHit = AccessTools.Method(typeof(Punch), "PunchSuccess");
    static readonly FieldInfo zombieAttacking = AccessTools.Field(typeof(Enemy), "attacking");
    static readonly FieldInfo spiderWindow = AccessTools.Field(typeof(MaliciousFace), "spiderParryable");
    static readonly FieldInfo oldSpiderWindow = AccessTools.Field(typeof(SpiderBody), "parryable");
    static readonly FieldInfo droneParried = AccessTools.Field(typeof(Drone), "parried");

    internal void Tick()
    {
        var player = MonoSingleton<NewMovement>.Instance;
        var fist = MonoSingleton<FistControl>.Instance;
        var punch = fist ? fist.currentPunch : null;
        var camera = MonoSingleton<CameraController>.Instance;
        if (!Plugin.On(1) || !Plugin.Playing(player) || !fist || !fist.activated || !punch ||
            !punch.isActiveAndEnabled || punch.type != FistType.Standard || !camera)
        {
            handled.Clear();
            return;
        }
        var origin = camera.GetDefaultPos();
        int count;
        // Grow only when saturated so dense arenas do not silently omit an attack.
        while ((count = Physics.OverlapSphereNonAlloc(origin, 6f, nearby, ~0, QueryTriggerInteraction.Collide)) == nearby.Length)
            Array.Resize(ref nearby, nearby.Length * 2);
        eligible.Clear();
        hitEnemies.Clear();
        for (int i = 0; i < count; i++)
        {
            var collider = nearby[i];
            if (!collider || !collider.enabled || !collider.gameObject.activeInHierarchy) continue;
            var point = collider.ClosestPoint(origin);
            var delta = point - origin;
            if (delta.sqrMagnitude > 0.01f && Vector3.Dot(camera.transform.forward, delta.normalized) < 0.25f) continue;
            if (Physics.Linecast(origin, point, LayerMaskDefaults.Get(LMD.Environment), QueryTriggerInteraction.Ignore)) continue;
            var target = collider.attachedRigidbody ? collider.attachedRigidbody.transform : collider.transform;
            var helper = collider.GetComponent<ParryHelper>();
            if (!helper) helper = target.GetComponent<ParryHelper>();
            if (helper && helper.target) target = helper.target;

            // Layer 14 is the game's punch/parry-object layer. Its active colliders
            // expose the real windows for mines, receivers, swords and other objects.
            if (collider.gameObject.layer == 14)
            {
                var projectile = target.GetComponent<Projectile>();
                if (projectile && (projectile.friendly || projectile.playerBullet || projectile.parried ||
                    projectile.decorative || projectile.unparryable || projectile.undeflectable)) continue;
                var receiver = target.GetComponent<ParryReceiver>();
                if (receiver && !receiver.isActiveAndEnabled) continue;
                bool objectTarget = projectile || receiver || target.GetComponent<Cannonball>() ||
                    target.GetComponent<ThrownSword>() || target.GetComponent<MassSpear>() ||
                    target.GetComponent<Landmine>() || target.GetComponent<Chainsaw>() || target.GetComponent<GroundWave>();
                if (objectTarget)
                {
                    if (!Plugin.Instance.Settings.ParryObjects.Value) continue;
                    eligible.Add(target);
                    if (!handled.Contains(target))
                    {
                        bool success = (bool)parry.Invoke(punch, new object[] { target, false });
                        // These two native branches act successfully but return false.
                        if (success || target.GetComponent<Chainsaw>() || target.GetComponent<GroundWave>()) handled.Add(target);
                    }
                    continue;
                }
            }

            if (!Plugin.Instance.Settings.ParryMelee.Value) continue;
            var part = target.GetComponent<EnemyIdentifierIdentifier>();
            // PunchSuccess routes enemy hits through this exact hitbox component.
            // A root proximity collider must not consume the enemy's one hit.
            var enemyId = part ? part.eid : null;
            if (!enemyId || hitEnemies.Contains(enemyId)) continue;
            var enemy = enemyId.GetComponent<Enemy>();
            var spider = enemyId.spider;
            var oldSpider = enemyId.GetComponent<SpiderBody>();
            var drone = enemyId.drone;
            bool crash = drone && drone.crashing && !(bool)droneParried.GetValue(drone);
            bool specialWindow = (enemy is Zombie && (bool)zombieAttacking.GetValue(enemy)) ||
                (spider && (bool)spiderWindow.GetValue(spider)) ||
                (oldSpider && (bool)oldSpiderWindow.GetValue(oldSpider));
            if (!ParryRules.EnemyWindow(enemyId.dead, crash, enemy && enemy.parryable,
                enemy && enemy.partiallyParryable,
                enemy && enemy.parryables != null && enemy.parryables.Contains(target), specialWindow)) continue;
            // Use the native punch hit so damage, stagger, rewards and boss reactions
            // go through the same handlers as a player's correctly timed parry.
            hitEnemies.Add(enemyId);
            punchHit.Invoke(punch, new object[] { point, target });
        }
        handled.IntersectWith(eligible); // Re-arm objects when their parry collider/window leaves the scan.
        Array.Clear(nearby, 0, count);
    }
}
