using System;
using PRankHelper;

void Check(bool condition) { if (!condition) throw new Exception("Rank rule check failed."); }
Check(RankRules.TryThreshold(new[] { 300, 240, 180, 120 }, true, out var time) && time == 120);
Check(RankRules.TryThreshold(new[] { 10, 20, 30, 40 }, false, out var kills) && kills == 40);
Check(!RankRules.TryThreshold(null, true, out _));
Check(!RankRules.TryThreshold(new[] { 100, 50 }, true, out _));
Check(!RankRules.TryThreshold(new[] { 100, 50, 20, 0 }, true, out _));
Check(RankRules.TryThreshold(new[] { 0, 0, 0, 0 }, false, out _));
Check(RankRules.Status(120, 40, 1000, 120, 40, 1000, 0, false, false, false).StartsWith("ON TARGET"));
Check(RankRules.Status(120.01f, 40, 1000, 120, 40, 1000, 0, false, false, false).Contains("TIME LIMIT"));
Check(RankRules.Status(100, 39, 1000, 120, 40, 1000, 0, false, false, false) == "IN PROGRESS");
Check(RankRules.Status(100, 40, 999, 120, 40, 1000, 0, false, false, true) == "REQUIREMENTS NOT MET");
Check(RankRules.Status(100, 40, 1000, 120, 40, 1000, 1, false, false, true).Contains("RESTART"));
Check(RankRules.Status(100, 40, 1000, 120, 40, 1000, 0, true, false, true).Contains("MAJOR"));
Check(RankRules.Status(100, 40, 1000, 120, 40, 1000, 0, false, true, true).Contains("CHEATS"));
Check(RankRules.Status(100, 40, 1000, 120, 40, 1000, 0, false, false, true).StartsWith("P-RANK"));
Check(RankRules.Time(125.5f) == "2:05.5");
Console.WriteLine("PASS: 15 rank threshold, boundary, eligibility and formatting checks.");
