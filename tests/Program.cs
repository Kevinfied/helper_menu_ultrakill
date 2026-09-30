using System;
using EasyMode;

static void Check(bool expected, bool dead = false, bool crash = false, bool full = false,
    bool partial = false, bool part = false, bool special = false)
{
    if (ParryRules.EnemyWindow(dead, crash, full, partial, part, special) != expected)
        throw new Exception($"Wrong parry eligibility: dead={dead}, crash={crash}, full={full}, partial={partial}, part={part}, special={special}");
}
Check(false); // Idle and non-parryable attacks must never trigger an ordinary punch.
Check(true, full: true);
Check(false, partial: true); // Boss partial windows require the right hitbox.
Check(true, partial: true, part: true);
Check(false, part: true);
Check(true, special: true); // Legacy husk and Malicious Face windows.
Check(false, dead: true, full: true, partial: true, part: true, special: true);
Check(true, dead: true, crash: true); // Dying drones can still be parried.
Console.WriteLine("PASS: 8 enemy parry-window checks.");
